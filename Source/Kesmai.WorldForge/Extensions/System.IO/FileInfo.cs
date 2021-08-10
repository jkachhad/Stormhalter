using System;
using System.Linq;

namespace System.IO
{
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

		public static bool IsZipFile(this FileInfo file)
		{
			return file.HasLeadingBytes(new byte[]
			{
				0x50, 0x4b, 0x03, 0x04,
			});
		}
	}
}