using System;
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Roslyn;

public class SegmentRoslynHost : IRoslynHost, IDisposable
{
	private readonly Segment _segment;
	private bool _disposed;

	public SegmentRoslynHost(Segment segment) : base()
	{
		_segment = segment;
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		GC.SuppressFinalize(this);
	}

	public TService GetService<TService>()
	{
		throw new NotImplementedException();
	}

	public TService GetWorkspaceService<TService>(DocumentId documentId) where TService : IWorkspaceService
	{
		throw new NotImplementedException();
	}

	public DocumentId AddDocument(DocumentCreationArgs args)
	{
		throw new NotImplementedException();
	}

	public Document GetDocument(DocumentId documentId)
	{
		throw new NotImplementedException();
	}

	public void CloseDocument(DocumentId documentId)
	{
		throw new NotImplementedException();
	}

	public MetadataReference CreateMetadataReference(string location)
	{
		throw new NotImplementedException();
	}

	public ParseOptions ParseOptions { get; }
}