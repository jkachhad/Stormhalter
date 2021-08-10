namespace Kesmai.WorldForge.Editor
{
#if (CanImport)
	public class MapGENRegion : MapGENArea, IImportedRegion
	{
		/// <summary>
		/// Gets or sets the converted region identifier.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="LomRegion"/> is imported.
		/// </summary>
		public bool Import { get; set; }
		
		/// <summary>
		/// Gets or sets the converted region.
		/// </summary>
		public SegmentRegion Region { get; set; }

		public MapGENRegion(string line) : base(line)
		{
			var lastIndex = line.LastIndexOf(',');
			var name = line.Substring(lastIndex + 1).Trim();

			Name = name;
		}
	}
#endif
}