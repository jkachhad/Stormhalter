using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

public sealed record RoslynConstructorParameter(string Name, string TypeName, bool IsOptional, bool IsParams, string DefaultValue,
    IReadOnlyList<string> Suggestions);
public sealed record RoslynConstructorDescriptor(string TypeName, string Signature,
    IReadOnlyList<RoslynConstructorParameter> Parameters);

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
        var segmentDocuments = Directory.GetFiles(segment.Directory, "*.cs", SearchOption.AllDirectories)
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
        WeakReferenceMessenger.Default.Register<SegmentSpawnChanged>(this, (_, _) => OnSegmentChanged());
        
        // watch for file changes
        WeakReferenceMessenger.Default.Register<SegmentFileCreatedMessage>(this, (_, message) => OnSegmentFileCreated(message.Value));
        WeakReferenceMessenger.Default.Register<SegmentFileDeletedMessage>(this, (_, message) => OnSegmentFileDeleted(message.Value));
        WeakReferenceMessenger.Default.Register<SegmentFileRenamedMessage>(this, (_, message) => OnSegmentFileRenamed(message.Value));
        WeakReferenceMessenger.Default.Register<SegmentFileChangedMessage>(this, (_, message) => OnSegmentFileChanged(message.Value));
    }

    public override RoslynWorkspace CreateWorkspace()
    {
        return _workspace;
    }

    public async Task<IReadOnlyList<string>> GetAttackComponentTypeNamesAsync()
    {
        var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
        var compilation = project == null ? null : await project.GetCompilationAsync();
        if (compilation == null)
            return Array.Empty<string>();

        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
            CollectAttackComponentTypes(assembly.GlobalNamespace, names);
        CollectAttackComponentTypes(compilation.GlobalNamespace, names);
        return names.ToArray();
    }

    public async Task<IReadOnlyList<string>> GetWieldableTypeNamesAsync()
    {
        var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
        var compilation = project == null ? null : await project.GetCompilationAsync();
        if (compilation == null) return Array.Empty<string>();

        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
            CollectWieldableTypes(assembly.GlobalNamespace, names);
        CollectWieldableTypes(compilation.GlobalNamespace, names);
        return names.ToArray();
    }

    public async Task<IReadOnlyList<string>> GetEquipmentTypeNamesAsync()
    {
        var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
        var compilation = project == null ? null : await project.GetCompilationAsync();
        if (compilation == null) return Array.Empty<string>();

        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
            CollectEquipmentTypes(assembly.GlobalNamespace, names);
        CollectEquipmentTypes(compilation.GlobalNamespace, names);
        return names.ToArray();
    }

    public async Task<IReadOnlyList<string>> GetCreatureDefenseSuggestionsAsync()
    {
        var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
        var compilation = project == null ? null : await project.GetCompilationAsync();
        if (compilation == null) return Array.Empty<string>();

        var suggestions = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols.Append(compilation.Assembly))
            CollectCreatureDefenseSuggestions(assembly.GlobalNamespace, suggestions);
        return suggestions.ToArray();
    }

    public async Task<IReadOnlyList<string>> GetSpellStatusSuggestionsAsync()
    {
        var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
        var compilation = project == null ? null : await project.GetCompilationAsync();
        if (compilation == null) return Array.Empty<string>();
        var suggestions = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols.Append(compilation.Assembly))
            CollectSpellStatusSuggestions(assembly.GlobalNamespace, suggestions);
        return suggestions.ToArray();
    }

    public async Task<IReadOnlyList<string>> GetCreatureSpellTypeNamesAsync()
	{
		var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
		var compilation = project == null ? null : await project.GetCompilationAsync();
		if (compilation == null) return Array.Empty<string>();
		var names = new SortedSet<string>(StringComparer.Ordinal);
		foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols.Append(compilation.Assembly))
			CollectCreatureSpellTypes(assembly.GlobalNamespace, names);
		return names.ToArray();
	}

	public async Task<IReadOnlyList<string>> GetEntityStatNamesAsync()
	{
		var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
		var compilation = project == null ? null : await project.GetCompilationAsync();
		if (compilation == null) return Array.Empty<string>();
		var names = new SortedSet<string>(StringComparer.Ordinal);
		foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols.Append(compilation.Assembly))
		{
			var type = FindType(assembly.GlobalNamespace, "EntityStat");
			if (type?.TypeKind != TypeKind.Enum) continue;
			foreach (var field in type.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue))
				names.Add(field.Name);
		}
		return names.ToArray();
	}

	private static void CollectCreatureSpellTypes(INamespaceSymbol namespaceSymbol, ISet<string> names)
	{
		foreach (var type in namespaceSymbol.GetTypeMembers()) CollectCreatureSpellType(type, names);
		foreach (var child in namespaceSymbol.GetNamespaceMembers()) CollectCreatureSpellTypes(child, names);
	}

	private static void CollectCreatureSpellType(INamedTypeSymbol type, ISet<string> names)
	{
		var isCreatureSpell = false;
		for (var current = type.BaseType; current != null; current = current.BaseType)
			if (current.Name is "DelayedSpell" or "InstantSpell") { isCreatureSpell = true; break; }
		if (isCreatureSpell && !type.IsAbstract &&
			type.DeclaredAccessibility is not (Microsoft.CodeAnalysis.Accessibility.Private or
				Microsoft.CodeAnalysis.Accessibility.Protected))
			names.Add(type.Name);
		foreach (var nestedType in type.GetTypeMembers()) CollectCreatureSpellType(nestedType, names);
	}

    private static void CollectSpellStatusSuggestions(INamespaceSymbol namespaceSymbol, ISet<string> suggestions)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers()) CollectSpellStatusSuggestions(type, suggestions);
        foreach (var child in namespaceSymbol.GetNamespaceMembers()) CollectSpellStatusSuggestions(child, suggestions);
    }

    private static void CollectSpellStatusSuggestions(INamedTypeSymbol type, ISet<string> suggestions)
    {
        var isSpellStatus = false;
        for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            if (baseType.Name == "SpellStatus") { isSpellStatus = true; break; }
        if (isSpellStatus && !type.IsAbstract &&
            type.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public)
            foreach (var constructor in type.InstanceConstructors.Where(item =>
                         item.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public))
                suggestions.Add($"new {type.Name}({String.Join(", ", constructor.Parameters.Select(parameter =>
                    GetStatusParameterDefault(parameter.Type)))})");
        foreach (var nestedType in type.GetTypeMembers()) CollectSpellStatusSuggestions(nestedType, suggestions);
    }

    private static string GetStatusParameterDefault(ITypeSymbol type)
    {
        if (type.Name.EndsWith("Entity", StringComparison.Ordinal)) return "{entity}";
        if (type.Name == "TimeSpan") return "TimeSpan.Zero";
        if (type.SpecialType == SpecialType.System_Boolean) return "false";
        if (type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or
            SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or
            SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or
            SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) return "0";
        if (type.TypeKind == TypeKind.Enum && type is INamedTypeSymbol enumType)
        {
            var first = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(field => field.HasConstantValue);
            if (first != null) return $"{enumType.Name}.{first.Name}";
        }
        return "null";
    }

    private static void CollectCreatureDefenseSuggestions(INamespaceSymbol namespaceSymbol,
        ISet<string> suggestions)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
            CollectCreatureDefenseSuggestions(type, suggestions);
        foreach (var child in namespaceSymbol.GetNamespaceMembers())
            CollectCreatureDefenseSuggestions(child, suggestions);
    }

    private static void CollectCreatureDefenseSuggestions(INamedTypeSymbol type, ISet<string> suggestions)
    {
        if (type.TypeKind == TypeKind.Enum && type.Name is "CreatureWeakness" or "CreatureImmunity")
            foreach (var member in type.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue))
                suggestions.Add($"{type.Name}.{member.Name}");

        var isSpell = false;
        for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            if (baseType.Name is "DelayedSpell" or "InstantSpell")
            { isSpell = true; break; }
		var isDefenseItem = IsWieldable(type) || ImplementsInterface(type, "IWeapon");
		if ((isSpell || isDefenseItem) && !type.IsAbstract &&
            type.DeclaredAccessibility is not (Microsoft.CodeAnalysis.Accessibility.Private or
                Microsoft.CodeAnalysis.Accessibility.Protected))
            suggestions.Add($"typeof({type.Name})");

        foreach (var nestedType in type.GetTypeMembers())
            CollectCreatureDefenseSuggestions(nestedType, suggestions);
    }

    private static bool IsWieldable(INamedTypeSymbol type)
		=> ImplementsInterface(type, "IWieldable");

	private static bool ImplementsInterface(INamedTypeSymbol type, string interfaceName)
    {
        for (var current = type; current != null; current = current.BaseType)
            if (current.Interfaces.Concat(current.AllInterfaces).Any(@interface =>
					String.Equals(@interface.Name, interfaceName, StringComparison.OrdinalIgnoreCase)))
                return true;
        return false;
    }

    public async Task<bool> TypeHasMethodAsync(string typeName, string methodName)
    {
        var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
        var compilation = project == null ? null : await project.GetCompilationAsync();
        if (compilation == null || String.IsNullOrWhiteSpace(typeName)) return false;

        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols.Append(compilation.Assembly))
        {
            var type = FindType(assembly.GlobalNamespace, typeName.Split('.').Last());
            for (var current = type; current != null; current = current.BaseType)
                if (current.GetMembers(methodName).OfType<IMethodSymbol>().Any(method =>
                        method.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public))
                    return true;
        }
        return false;
    }

    private static INamedTypeSymbol? FindType(INamespaceSymbol namespaceSymbol, string typeName)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
            if (FindType(type, typeName) is { } match) return match;
        foreach (var child in namespaceSymbol.GetNamespaceMembers())
            if (FindType(child, typeName) is { } found) return found;
        return null;
    }

    private static INamedTypeSymbol? FindType(INamedTypeSymbol type, string typeName)
    {
        if (String.Equals(type.Name, typeName, StringComparison.Ordinal)) return type;
        foreach (var nestedType in type.GetTypeMembers())
            if (FindType(nestedType, typeName) is { } found) return found;
        return null;
    }

    private static void CollectEquipmentTypes(INamespaceSymbol namespaceSymbol, ISet<string> names)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
            CollectEquipmentType(type, names);
        foreach (var child in namespaceSymbol.GetNamespaceMembers())
            CollectEquipmentTypes(child, names);
    }

    private static void CollectEquipmentType(INamedTypeSymbol type, ISet<string> names)
    {
        var isEquipment = false;
        for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            if (String.Equals(baseType.Name, "Equipment", StringComparison.OrdinalIgnoreCase))
            { isEquipment = true; break; }
        if (isEquipment && !type.IsAbstract &&
            type.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public)
            names.Add(type.Name);
        foreach (var nestedType in type.GetTypeMembers())
            CollectEquipmentType(nestedType, names);
    }

    private static void CollectWieldableTypes(INamespaceSymbol namespaceSymbol, ISet<string> names)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
            CollectWieldableType(type, names);
        foreach (var child in namespaceSymbol.GetNamespaceMembers())
            CollectWieldableTypes(child, names);
    }

    private static void CollectWieldableType(INamedTypeSymbol type, ISet<string> names)
    {
        var isWieldable = IsWieldable(type);
        if (isWieldable && !type.IsAbstract &&
            type.DeclaredAccessibility is not (Microsoft.CodeAnalysis.Accessibility.Private or
                Microsoft.CodeAnalysis.Accessibility.Protected))
            names.Add(type.Name);
        foreach (var nestedType in type.GetTypeMembers())
            CollectWieldableType(nestedType, names);
    }

    public async Task<IReadOnlyList<RoslynConstructorDescriptor>> GetCreatureConstructorsAsync()
    {
        var project = _workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == "Segment");
        var compilation = project == null ? null : await project.GetCompilationAsync();
        if (compilation == null) return Array.Empty<RoslynConstructorDescriptor>();

        var constructors = new List<RoslynConstructorDescriptor>();
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
            CollectCreatureConstructors(assembly.GlobalNamespace, constructors);
        CollectCreatureConstructors(compilation.GlobalNamespace, constructors);
        return constructors.GroupBy(item => $"{item.TypeName}|{item.Signature}", StringComparer.Ordinal)
            .Select(group => group.First()).OrderBy(item => item.TypeName, StringComparer.Ordinal)
            .ThenBy(item => item.Parameters.Count).ToArray();
    }

    private static void CollectCreatureConstructors(INamespaceSymbol namespaceSymbol,
        ICollection<RoslynConstructorDescriptor> constructors)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
			if (type.Name is "CreatureAttack" or "CreatureBasicAttack" or "CreatureBlock" or "CreatureSpell" or "CreatureSpellCollection" or "LootPackEntry")
            {
                var typeName = type.Name == "CreatureSpell" && type.Arity == 1 ? "CreatureSpell<>" : type.Name;
                foreach (var constructor in type.InstanceConstructors.Where(item =>
                             item.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public))
                {
                    var parameters = constructor.Parameters.Select(parameter => new RoslynConstructorParameter(
                        parameter.Name, parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
						parameter.IsOptional, parameter.IsParams, FormatDefaultValue(parameter), GetSuggestions(parameter.Type))).ToArray();
                    var signature = $"{type.Name}({String.Join(", ", parameters.Select(parameter =>
                        $"{parameter.TypeName} {parameter.Name}{(parameter.IsOptional ? $" = {parameter.DefaultValue}" : "")}"))})";
                    constructors.Add(new RoslynConstructorDescriptor(typeName, signature, parameters));
                }
            }
        }
        foreach (var child in namespaceSymbol.GetNamespaceMembers())
            CollectCreatureConstructors(child, constructors);
    }

    private static IReadOnlyList<string> GetSuggestions(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_Boolean) return new[] { "true", "false" };
        if (type.TypeKind == TypeKind.Enum && type is INamedTypeSymbol enumType)
            return enumType.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue)
                .Select(field => $"{enumType.Name}.{field.Name}").ToArray();
        return Array.Empty<string>();
    }

    private static string FormatDefaultValue(IParameterSymbol parameter)
    {
        if (!parameter.HasExplicitDefaultValue) return "";
        return parameter.ExplicitDefaultValue switch
        {
            null => "null", bool value => value ? "true" : "false",
            string value => $"\"{value.Replace("\"", "\\\"")}\"",
            _ => Convert.ToString(parameter.ExplicitDefaultValue, System.Globalization.CultureInfo.InvariantCulture) ?? ""
        };
    }

    private static void CollectAttackComponentTypes(INamespaceSymbol namespaceSymbol, ISet<string> names)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (type.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public && !type.IsAbstract &&
                type.Name.StartsWith("Attack", StringComparison.Ordinal) &&
                type.Name.EndsWith("Component", StringComparison.Ordinal))
                names.Add(type.Name);
        }

        foreach (var child in namespaceSymbol.GetNamespaceMembers())
            CollectAttackComponentTypes(child, names);
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
        var path = args.FullPath;
        var extension = Path.GetExtension(path).ToLower();
        
        // we only process C# files.
        if (!extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            return;
        
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
        var path = args.FullPath;
        var extension = Path.GetExtension(path).ToLower();
        
        // we only process C# files.
        if (!extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            return;
        
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
        var path = args.FullPath;
        var extension = Path.GetExtension(path).ToLower();
        
        // we only process C# files.
        if (!extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            return;
        
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
        var path = args.FullPath;
        var extension = Path.GetExtension(path).ToLower();
        
        // we only process C# files.
        if (!extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            return;
        
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

        if (segment is null)
            return;

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
