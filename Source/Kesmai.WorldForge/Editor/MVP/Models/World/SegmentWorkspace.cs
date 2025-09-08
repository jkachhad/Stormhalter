using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Roslyn;
using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Editor;

public class SegmentWorkspace
{
	public CustomRoslynHost Host { get; set; }
	
	public SegmentWorkspace()
	{
		WeakReferenceMessenger.Default.Register<SegmentChangedMessage>(this, (_, message) =>
		{
			var segment = message.Value;
			
			Reset();

			if (segment != null)
				Start(segment);
		});
		
		WeakReferenceMessenger.Default.Register<SegmentFileCreatedMessage>(this, 
			(_, message) => Host.OnSegmentFileCreated(message.Value));
		
		WeakReferenceMessenger.Default.Register<SegmentFileDeletedMessage>(this, 
			(_, message) => Host.OnSegmentFileDeleted(message.Value));
		
		WeakReferenceMessenger.Default.Register<SegmentFileRenamedMessage>(this,
			(_, message) => Host.OnSegmentFileRenamed(message.Value));
		
		WeakReferenceMessenger.Default.Register<SegmentFileChangedMessage>(this,
			(_, message) => Host.OnSegmentFileChanged(message.Value));
	}

	public void Start(Segment segment)
	{
		var refs = Task.Run(() => NuGetResolver.Resolve("Kesmai.Server.Reference", "net8.0-windows8.0"));
		
		var blacklistedAssemblies = new[]
		{
			"RoslynPad.Roslyn.Windows",
			"RoslynPad.Editor.Windows",
			"DigitalRune",
			"MonoGame",
			"SharpDX",
			"WindowsDesktop",
			"WorldForge",
			
			"Microsoft.Win32",
			
			"System.ComponentModel",
			"System.Diagnostics",
			"System.Reflection",
			"System.Threading",
			"System.Net",
			"System.IO",
			
			"System.Private.Uri",
			"System.Private.Xml",
			
			"System.Runtime.Extensions",
			"System.Runtime.InteropServices",
			"System.Runtime.CompilerServices.VisualC",
			
			"System.Collections.NonGeneric",
			"System.Collections.Specialized",
			"System.Memory",
			"System.Xml",
			"System.Resources",
		};
		
		var metadataReferences = AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => !a.IsDynamic && !String.IsNullOrEmpty(a.Location))
			.Where(a => blacklistedAssemblies.All(b => !a.Location.Contains(b)))
			.Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
			.ToList();

		foreach (var metadataReference in refs.Result)
			metadataReferences.Add(metadataReference);
		
		var serviceAssemblies = new[]
		{
			Assembly.Load("RoslynPad.Roslyn.Windows"),
			Assembly.Load("RoslynPad.Editor.Windows")
		};

		var namespaceImports = new string[]
		{
			$"static Kesmai.Server.Segments.{segment.Name}",
			$"static Kesmai.Server.Segments.Editor",
			"Kesmai.Server.Game",
			"Kesmai.Server.Items",
			"Kesmai.Server.Miscellaneous",
			"Kesmai.Server.Network",
			"Kesmai.Server.Spells",
			"SpanReader = DotNext.Buffers.SpanReader<byte>",
			"SpanWriter = DotNext.Buffers.PoolingArrayBufferWriter<byte>",
		};
		
		var roslynReferences = RoslynHostReferences.NamespaceDefault
			.With(references: metadataReferences, imports: namespaceImports);
		
		Host = new CustomRoslynHost(segment, serviceAssemblies, roslynReferences);
		Host.UpdateEditorDocument();
	}

	public void Reset()
	{
		if (Host is null)
			return;

		Host = null;
	}
}