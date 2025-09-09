using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.Diagnostics;

namespace Kesmai.WorldForge.Roslyn;

public class CustomRoslynHost : RoslynHost
{
    private CustomRoslynWorkspace _workspace;

    private DocumentId _editorDocumentId;

    private Dictionary<string, DocumentId> _segmentDocuments 
        = new Dictionary<string, DocumentId>();
    
    public CustomRoslynHost(Segment segment, IEnumerable<Assembly> additionalAssemblies, RoslynHostReferences references) : base(additionalAssemblies, references)
    {
        _workspace = new CustomRoslynWorkspace(HostServices, WorkspaceKind.Host, this);
        _workspace.Services.GetRequiredService<IDiagnosticsUpdater>()
            .DisabledDiagnostics = DisabledDiagnostics;
        
        // create segment project
        var segmentSolution = _workspace.CurrentSolution;
		
        var segmentProject = segmentSolution.AddProject($"Segment", $"Kesmai.Server.Segments.{segment.Name}", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            /* C# minimum to support global usings. */
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.CSharp10))
            /* Minimum references to prevent overloading */
            .WithMetadataReferences(DefaultReferences);

        segmentSolution = segmentProject.Solution;
        
        // add documents to segment project.
        var segmentDocuments = Directory.GetFiles(segment.Path, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains(@"\obj\") && !p.Contains(@"\bin\"));

        foreach (var segmentDocument in segmentDocuments)
        {
            var documentId = DocumentId.CreateNewId(segmentProject.Id);
            var documentName = Path.GetFileName(segmentDocument);
            var documentText = File.ReadAllText(segmentDocument);
            
            segmentSolution = segmentSolution.AddDocument(documentId, documentName, 
                SourceText.From(documentText), filePath: segmentDocument);
            
            _segmentDocuments[segmentDocument] = documentId;
        }
        
        _workspace.TryApplyChanges(segmentSolution);
        
        // create editor project
        var editorSolution = _workspace.CurrentSolution;
		
        var editorProject = editorSolution.AddProject($"Editor", $"Kesmai.Server.Segments.Editor", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            /* C# minimum to support global usings. */
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.CSharp10))
            /* Minimum references to prevent overloading */
            .WithMetadataReferences(DefaultReferences);
        
        _editorDocumentId = DocumentId.CreateNewId(editorProject.Id);
        
        editorSolution = editorProject.Solution.AddDocument(_editorDocumentId, "Editor.g.cs", 
            SourceText.From("namespace Kesmai.Server.Segments; public static class Editor { }"));
        
        _workspace.TryApplyChanges(editorSolution);
        
        // bind events
        WeakReferenceMessenger.Default.Register<SegmentLocationChanged>(this, (_, _) => OnSegmentChanged());
        WeakReferenceMessenger.Default.Register<SegmentEntityChanged>(this, (_, _) => OnSegmentChanged());
        WeakReferenceMessenger.Default.Register<SegmentTreasuresChanged>(this, (_, _) => OnSegmentChanged());
        WeakReferenceMessenger.Default.Register<SegmentSpawnerChanged>(this, (_, _) => OnSegmentChanged());
    }

    public override RoslynWorkspace CreateWorkspace()
    {
        return _workspace;
    }
    
    protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null)
    {
        var projectName = "Script";
        var projectId = ProjectId.CreateNewId(projectName);

        var parseOptions = ParseOptions.WithKind(args.SourceCodeKind);
        
        var projectReferences = solution.ProjectIds
            .Select(p => new ProjectReference(p));
        
        solution = solution.AddProject(ProjectInfo.Create(
                projectId, VersionStamp.Create(), projectName, projectName,
                LanguageNames.CSharp,
                filePath: args.WorkingDirectory,
                isSubmission: false,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions,
                metadataReferences: previousProject != null ? [] : DefaultReferences,
                projectReferences: previousProject != null ? [new ProjectReference(previousProject.Id)] : projectReferences));
        
        var project = solution.GetProject(projectId);
        
        if (project is null)
            throw new InvalidOperationException("Could not create project.");
        
        if (GetUsings(project) is { Length: > 0 } usings)
            project = project.AddDocument("Usings.g.cs", usings).Project;

        return project;

        static string GetUsings(Project project)
        {
            if (project.CompilationOptions is CSharpCompilationOptions options)
                return String.Join(" ", options.Usings.Select(i => $"global using {i};"));

            return String.Empty;
        }
    }

    public void OnSegmentFileCreated(FileSystemEventArgs args)
    {
        var workspace = _workspace;
        var solution = workspace.CurrentSolution;

        if (_segmentDocuments.ContainsKey(args.FullPath))
            return;
        
        var segmentProject = solution.Projects.FirstOrDefault(p => p.Name.Equals("Segment"));

        if (segmentProject is null)
            return;
        
        var documentId = DocumentId.CreateNewId(segmentProject.Id);
        var documentName = Path.GetFileName(args.FullPath);
        var documentText = File.ReadAllText(args.FullPath);
            
        solution = solution.AddDocument(documentId, documentName, 
            SourceText.From(documentText), filePath: args.FullPath);
            
        _segmentDocuments[args.FullPath] = documentId;
                
        workspace.TryApplyChanges(solution);
    }

    public void OnSegmentFileDeleted(FileSystemEventArgs args)
    {
        if (!_segmentDocuments.TryGetValue(args.FullPath, out var documentId))
            return;
        
        var workspace = _workspace;
        var solution = workspace.CurrentSolution;

        solution = solution.RemoveDocument(documentId);
        
        _segmentDocuments.Remove(args.FullPath);

        workspace.TryApplyChanges(solution);
    }
    
    public void OnSegmentFileRenamed(RenamedEventArgs args)
    {
        if (!_segmentDocuments.TryGetValue(args.OldFullPath, out var documentId)) 
            return;
        
        var workspace = _workspace;
        var solution = workspace.CurrentSolution;

        solution = solution.WithDocumentName(documentId, Path.GetFileName(args.FullPath));
            
        _segmentDocuments.Remove(args.OldFullPath);
        _segmentDocuments[args.FullPath] = documentId;
            
        workspace.TryApplyChanges(solution);
    }
    
    public void OnSegmentFileChanged(FileSystemEventArgs args)
    {
        if (!_segmentDocuments.TryGetValue(args.FullPath, out var documentId))
            return;
        
        var workspace = _workspace;
        var solution = workspace.CurrentSolution;

        var documentText = File.ReadAllText(args.FullPath);
            
        solution = solution.WithDocumentText(documentId, TextAndVersion.Create(
            SourceText.From(documentText),
            VersionStamp.Create()), PreservationMode.PreserveIdentity);
            
        workspace.TryApplyChanges(solution);
    }

    public void OnSegmentChanged()
    {
        var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
        var segment = presenter.Segment;

        var builder = new StringBuilder();
        
        builder.AppendLine($"namespace Kesmai.Server.Segments;");
        builder.AppendLine(String.Empty);
        builder.AppendLine($@"public static class Editor {{");

        foreach (var lootTemplate in segment.Treasures.Select(t => t.Name))
            builder.AppendLine($"\tpublic static Func<MobileEntity, Container, ItemEntity> {lootTemplate};");

        foreach (var entities in segment.Entities.Select(t => t.Name))
            builder.AppendLine($"\tpublic static Func<CreatureEntity> {entities};");
        
        builder.AppendLine($"}}");

        var workspace = _workspace;
        var solution = workspace.CurrentSolution;

        solution = solution.WithDocumentText(_editorDocumentId, TextAndVersion.Create(SourceText.From(builder.ToString()),
                VersionStamp.Create()), PreservationMode.PreserveIdentity);

        workspace.TryApplyChanges(solution);
    }
    
    // Workaround for multiple additions of GetSolutionAnalyzerReferences.
    private bool _initializedAnalyzers;
    
    protected override IEnumerable<AnalyzerReference> GetSolutionAnalyzerReferences()
    {
        if (_initializedAnalyzers)
            return [];
        
        _initializedAnalyzers = true;
        
        return base.GetSolutionAnalyzerReferences();
    }
}