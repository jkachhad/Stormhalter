using System;
using System.Linq;

namespace System.IO;

public static class FileInfoExtensions
{
	public static bool HasLeadingBytes(this FileInfo file, byte[] data)
	{
		using (var stream = file.OpenRead())
		{
			byte[] bytes = new byte[4];

			stream.Read(bytes, 0, 4);

			if (bytes.SequenceEqual(data))
				return true;
		}

		return false;
	}
}