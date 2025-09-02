using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Roslyn;

public class CustomRoslynWorkspace : RoslynWorkspace
{
	public CustomRoslynWorkspace(HostServices hostServices, string workspaceKind, RoslynHost roslynHost) : base(hostServices, workspaceKind, roslynHost)
	{
	}

	public override bool CanApplyChange(ApplyChangesKind feature)
	{
		return feature switch
		{
			ApplyChangesKind.RemoveProject or ApplyChangesKind.AddProject => true,
			ApplyChangesKind.AddSolutionAnalyzerReference => true,
			_ => base.CanApplyChange(feature),
		};
	}
}