
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using RoslynPad.Editor;
using System.Collections.Generic;
using System.Linq;

public class RoslynFoldingBuilder
{
	/*
	public void UpdateFoldings(FoldingManager manager, TextDocument document)
	{
		var tree = CSharpSyntaxTree.ParseText(document.Text);
		var root = tree.GetRoot();

		var foldings = new List<NewFolding>();
		AddFoldings(root, document, foldings);
		manager.UpdateFoldings(foldings.OrderBy(f => f.StartOffset).ToList(), -1);
	}

	private void AddFoldings(SyntaxNode node, TextDocument doc, IList<NewFolding> foldings)
	{
		foreach (var desc in node.DescendantNodes(descendIntoTrivia: true))
		{
			if (desc is NamespaceDeclarationSyntax ns)
				AddBlock(ns, doc, foldings, $"namespace {ns.Name}");

			else if (desc is ClassDeclarationSyntax cls)
				AddBlock(cls, doc, foldings, $"class {cls.Identifier.Text}");

			else if (desc is MethodDeclarationSyntax m)
				AddBlock(m, doc, foldings, $"{m.Identifier.Text}()");

			else if (desc is RegionDirectiveTriviaSyntax region)
			{
				var endRegion = region.GetRelatedDirectives()
					.OfType<EndRegionDirectiveTriviaSyntax>()
					.FirstOrDefault();

				if (endRegion != null)
				{
					var label = region.ToString().Replace("#region", "").Trim();
					if (string.IsNullOrWhiteSpace(label)) label = "#region";

					foldings.Add(new NewFolding(region.SpanStart, endRegion.Span.End)
					{
						Name = label
					});
				}
			}
		}
	}

	private void AddBlock(SyntaxNode node, TextDocument doc, IList<NewFolding> foldings, string label)
	{
		var startLine = doc.GetLineByOffset(node.Span.Start);
		var endLine = doc.GetLineByOffset(node.Span.End);

		if (startLine.LineNumber < endLine.LineNumber)
		{
			foldings.Add(new NewFolding(startLine.Offset, endLine.EndOffset)
			{
				Name = label
			});
		}
	}
	*/
}

