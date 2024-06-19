using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Kesmai.WorldForge.Editor;
using Lidgren.Network;
using CommunityToolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.UI.Windows;

public partial class CompileWindow : Window
{
	public CompileWindow()
	{
		InitializeComponent();
		
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		Network.OnIncoming -= OnIncoming;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		Network.OnIncoming += OnIncoming;
		
		var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var segment = segmentRequest.Response;

		if (segment is null)
			return;
		
		/* Save the segment */
		var projectFile = new XDocument();
		var segmentElement = new XElement("segment",
			new XAttribute("name", segment.Name ?? "(Unknown)"),
			new XAttribute("version", Core.Version));

		segment.Save(segmentElement);

		projectFile.Add(segmentElement);

		var compressedDocument = new MemoryStream();
		var compressedInternal = new MemoryStream();
		
		using (var uncompressed = new MemoryStream())
		{
			projectFile.Save(uncompressed);
			
			using (var gzipStream = new GZipStream(compressedDocument, CompressionMode.Compress, false))
			{
				gzipStream.Write(uncompressed.ToArray(), 0, (int)uncompressed.Length);
				gzipStream.Flush();
			}
		}
		
		using (var gzipStream = new GZipStream(compressedInternal, CompressionMode.Compress, false))
		{
			var data = Encoding.UTF8.GetBytes(segment.Definition.Blocks[0]);
			
			gzipStream.Write(data, 0, (int)data.Length);
			gzipStream.Flush();
		}
		
		Network.RequestCompile(compressedDocument.ToArray(), compressedInternal.ToArray());
	}
	
	private void OnIncoming(NetIncomingMessage message)
	{
		var command = message.ReadInt16();

		switch (command)
		{
			case 0x04:
			{
				var errorCount = message.ReadByte();
				var errors = new List<SegmentCompileError>();

				for (var i = 0; i < errorCount; i++)
				{
					errors.Add( new SegmentCompileError()
					{
						Class = message.ReadString(),
						Error = message.ReadString(),
						Syntax = message.ReadString(),
					});
				}

				Dispatcher.Invoke(() =>
				{
					_status.Text = $"{errorCount} Error(s).";
					_dataGrid.ItemsSource = errors;
				});
				break;
			}
		}
	}
	
	private record SegmentCompileError
	{
		public string Class { get; set; }
		public string Error { get; set; }
		public string Syntax { get; set; }
	}
}