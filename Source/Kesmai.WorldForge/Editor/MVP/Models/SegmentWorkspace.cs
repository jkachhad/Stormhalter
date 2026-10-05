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

	// incremented by every start and reset, so a start still awaiting package resolution
	// can tell it has been superseded.
	private int _startGeneration;
	
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
		var generation = ++_startGeneration;
		
		var packageReader = await NuGetResolver.Resolve("Kesmai.Server.Reference", "net8.0-windows8.0");
		var packageReferences = await NuGetResolver.ResolveMetadataReferences(packageReader);

		// a newer start or reset happened while resolving (e.g. another segment was opened);
		// creating this host would replace the current one with a host for a stale segment.
		if (generation != _startGeneration)
			return;
		
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
			"Kesmai.Server",
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
		_startGeneration++;
		
		if (Host is null)
			return;

		// the messenger holds recipients weakly, so a discarded host keeps receiving segment changes until it
		// is collected; while the next segment loads, that rebuilt its editor stub once per treasure added.
		WeakReferenceMessenger.Default.UnregisterAll(Host);

		Host = null;
	}

	private void Start()
	{
		if (_activeSegment is null)
			return;

		Start(_activeSegment);
	}
}
