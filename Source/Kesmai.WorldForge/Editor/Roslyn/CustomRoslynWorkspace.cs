using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
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
	
	protected override void ApplyDocumentTextChanged(DocumentId id, SourceText text)
	{
		if (OpenDocumentId != id)
		{
			OnDocumentTextChanged(id, text, PreservationMode.PreserveIdentity);
			return;
		}

		base.ApplyDocumentTextChanged(id, text);
	}
}