using System;
using System.IO;
using System.Windows;

namespace Kesmai.WorldForge.UI;

/// <summary>View > Region tools position: where the region editor's tool bars sit in their row. Kept in .storage.</summary>
public static class RegionToolsPosition
{
	private static readonly string _path = Path.Combine(".storage", "RegionTools.cfg");
	private static HorizontalAlignment? _alignment;

	/// <summary>Raised when the position changes, so every open region editor moves its tools.</summary>
	public static event EventHandler Changed;

	public static HorizontalAlignment Alignment
	{
		get
		{
			if (_alignment is { } alignment)
				return alignment;

			_alignment = HorizontalAlignment.Left;

			try
			{
				if (File.Exists(_path))
					_alignment = Parse(File.ReadAllText(_path).Trim().Replace("position=", String.Empty));
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
				// the tools start on the left
			}

			return _alignment.Value;
		}
	}

	public static void Set(string position)
	{
		_alignment = Parse(position);

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
			File.WriteAllText(_path, $"position={position}");
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// the position just isn't remembered
		}

		Changed?.Invoke(null, EventArgs.Empty);
	}

	private static HorizontalAlignment Parse(string position) => position switch
	{
		"Center" => HorizontalAlignment.Center,
		"Right" => HorizontalAlignment.Right,
		_ => HorizontalAlignment.Left,
	};
}
