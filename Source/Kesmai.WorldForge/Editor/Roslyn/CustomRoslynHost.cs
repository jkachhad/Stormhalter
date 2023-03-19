using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommonServiceLocator;
using ICSharpCode.AvalonEdit.Document;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Roslyn
{
	public class CustomRoslynHost : RoslynHost
	{
		private CustomResolver _resolver;
		
		public CustomRoslynHost(Segment segment) : base(
			additionalAssemblies: new[]
			{
				Assembly.Load("RoslynPad.Roslyn.Windows"),
				Assembly.Load("RoslynPad.Editor.Windows"),
			}, 
			RoslynHostReferences.NamespaceDefault.With(imports: new []
			{
				"WorldForge",
			}))
		{
			_resolver = new CustomResolver(segment);
		}
        
		protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null)
		{
			var name = "WorldForge";
			var id = ProjectId.CreateNewId(name);
			var parseOptions = new CSharpParseOptions(
				kind: SourceCodeKind.Script,
				languageVersion: LanguageVersion.CSharp8
			);
			
			compilationOptions = compilationOptions
				.WithScriptClassName(name)
				.WithSourceReferenceResolver(_resolver);
			
			if (compilationOptions is CSharpCompilationOptions csharpCompilationOptions)
			{
				compilationOptions = csharpCompilationOptions
					.WithNullableContextOptions(NullableContextOptions.Disable);
			}
			
			var references = new List<MetadataReference>()
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
			};

			var scriptingData = Core.ScriptingData;
			
			if (scriptingData != null)
				MetadataReference.CreateFromImage(scriptingData);
			
			solution = solution.AddProject(ProjectInfo.Create(
				id, VersionStamp.Create(),
				name, name,
				LanguageNames.CSharp,
				isSubmission: true,
				parseOptions: parseOptions,
				compilationOptions: compilationOptions,
				metadataReferences: previousProject != null ? ImmutableArray<MetadataReference>.Empty : references,
				projectReferences: previousProject != null ? new[] { new ProjectReference(previousProject.Id) } : null
			));
			
			return solution.GetProject(id);
		}
	}

	public class CustomResolver : SourceReferenceResolver
	{
		private readonly SourceText _cache;

		public CustomResolver(Segment segment)
		{
			var builder = new StringBuilder();

			if (segment.Internal != null)
				builder.AppendLine(segment.Internal.ToString());

			foreach (var lootTemplate in segment.Treasures.Select(t => t.Name))
				builder.AppendLine($"Func<MobileEntity, Container, ItemEntity> {lootTemplate};");

			foreach (var entities in segment.Entities.Select(t => t.Name))
				builder.AppendLine($"Func<CreatureEntity> {entities};");
			
			_cache = SourceText.From(builder.ToString());
		}

		public override SourceText ReadText(string resolvedPath) => _cache;

		public override string NormalizePath(string path, string baseFilePath) => path;
		public override string ResolveReference(string path, string baseFilePath) => path;
		public override Stream OpenRead(string resolvedPath) => null;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			
			if (ReferenceEquals(this, obj)) 
				return true;
			
			if (obj.GetType() != this.GetType()) 
				return false;

			if (obj is CustomResolver other && other._cache != _cache)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return _cache.GetHashCode();
		}
	}
}