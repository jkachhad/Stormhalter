using System;
using System.Collections.Generic;
using Kesmai.Server.Engines.Quests;
using Kesmai.Server.Engines.Spawners;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Segments
{
	public partial class Kesmai : Segment
	{
		public static bool Enabled = true;
		
		private SegmentRegion _surface;
		private SegmentRegion _lockers;
		private SegmentRegion _legendsHall;
		private SegmentRegion _dungeon1;
		private SegmentRegion _dungeon2;
		private SegmentRegion _dungeon3;
		private SegmentRegion _dungeon4;

		private LairSubregion _elderTrollCrypt;
		private LairSubregion _elderTrollLair;
		private LairSubregion _ydnacLair;
		private LairSubregion _dragonLair;
		private LairSubregion _dragonCrypt;
		
		public Kesmai(int index, SegmentCache cache, Facet facet) : base(index, cache, facet)
		{
		}
		
		protected override void OnInitialize()
		{
			base.OnInitialize();

			_surface = this[1];
			_lockers = this[2];
			_legendsHall = this[9];

			_dungeon1 = this[11];
			_dungeon2 = this[12];
			_dungeon3 = this[13];
			_dungeon4 = this[14];
			
			
			_ydnacLair = new LairSubregion("Ydnac's Lair")
			{
				{ new Rectangle2D(139, 3, 2, 6, _surface) }
			};
			_surface.Subregions.Add(_ydnacLair);
			
			
			_elderTrollCrypt = new LairSubregion("Troll Crypt")
			{
				new Rectangle2D(14, 25, 12, 13, _dungeon2)
			};
			_elderTrollLair = new LairSubregion("Troll Lair")
			{
				{ new Rectangle2D(15, 34, 4, 6, _dungeon1) }
			};
			_dungeon1.Subregions.Add(_elderTrollCrypt);
			_dungeon1.Subregions.Add(_elderTrollLair);
			
			_dragonCrypt = new LairSubregion("Dragon's Crypt")
            {
            	{ new Rectangle2D(12, 2, 5, 5, _dungeon4) }
            };
			_dragonLair = new LairSubregion("Dragon's Lair")
			{
				{ new Rectangle2D(12, 2, 16, 5, _dungeon4) }
			};
			
			_dungeon4.Subregions.Add(_dragonCrypt);
			_dungeon4.Subregions.Add(_dragonLair);
			
			_dungeon1.IsDungeon = _dungeon2.IsDungeon = _dungeon3.IsDungeon = _dungeon4.IsDungeon = true;
		}

		protected override void OnSpawn()
		{
			if (!Enabled)
				return;
			
#if (DEBUG)
			_spawns.Add(new LocationSpawner(Facet, this, new Point2D(3, 2, _lockers))
			{
				new SpawnEntry(() =>
				{
					var conjurer = new TreasureConjurer()
					{
						Name = "Treasure.Conjurer",
						MaxHealth = 5000, Health = 5000,
						BaseDodge = 10,
						Experience = 50,
					};

					return conjurer;
				})
			});
#endif
		
			Respawn();
		}
	}
}