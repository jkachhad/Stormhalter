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
	private Segment _activeSegment;
	
	public CustomRoslynHost Host { get; set; }
	
	public SegmentWorkspace()
	{
		WeakReferenceMessenger.Default.Register<ActiveSegmentChanged>(this, (_, message) =>
		{
			_activeSegment = message.Value;
			
			Reset();
			Start();
		});

		WeakReferenceMessenger.Default.Register<SegmentChanged>(this, (_, message) =>
		{
			if (_activeSegment is null || !ReferenceEquals(_activeSegment, message.segment))
				return;

			Reset();
			Start();
		});
	}

	public async void Start(Segment segment)
	{
		var packageReader = await NuGetResolver.Resolve("Kesmai.Server.Reference", "net8.0-windows8.0");
		var packageReferences = await NuGetResolver.ResolveMetadataReferences(packageReader);
		
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
			"System.Runtime.CompilerServices.Unsafe",
			"System.Runtime.Intrinsics.dll",
			"System.Runtime.Loader.dll",
			"System.Runtime.Serialization.Primitives.dll",
			"System.Runtime.Numerics.dll",
			"System.Runtime.Serialization.Json.dll",
			"System.Runtime.Serialization.Xml.dll",
			"System.Runtime.Serialization.Formatters.dll",
			"System.Security.Cryptography",
			"System.Security.Claims",
			"System.Security.Principal.Windows",
			"System.Threading",
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

		metadataReferences.AddRange(packageReferences);
		
		var serviceAssemblies = new[]
		{
			Assembly.Load("RoslynPad.Roslyn.Windows"),
			Assembly.Load("RoslynPad.Editor.Windows")
		};
		
		var segmentClassName = segment.Name.Replace(" ", String.Empty);

		var namespaceImports = new string[]
		{
			"System.Drawing",
			
			$"static Kesmai.Server.Segments.{segmentClassName}",
			$"static Kesmai.Server.Segments.Editor",
			"Kesmai.Server.Game",
			"Kesmai.Server.Items",
			"Kesmai.Server.Miscellaneous",
			"Kesmai.Server.Network",
			"Kesmai.Server.Spells",
			"SpanReader = DotNext.Buffers.SpanReader<byte>",
			"SpanWriter = DotNext.Buffers.PoolingArrayBufferWriter<byte>",

			$"static Kesmai.Server.Internal.{segmentClassName}.Cache"
		};
		
		var roslynReferences = RoslynHostReferences.NamespaceDefault
			.With(references: metadataReferences, imports: namespaceImports);
		
		Host = new CustomRoslynHost(segment, serviceAssemblies, roslynReferences);
		Host.OnSegmentChanged();
	}

	public void Reset()
	{
		if (Host is null)
			return;

		Host = null;
	}

	private void Start()
	{
		if (_activeSegment is null)
			return;

		Start(_activeSegment);
	}
}
