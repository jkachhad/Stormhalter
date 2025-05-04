using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Items;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to cause all items laying on the ground within view to be vaporized.  Disintegrate does 
not affect living creatures, nor does it affect recent corpses or very valuable items.  First warm the spell, 
then double left click on the spell icon in the warmed-spell rack to cast the spell.

Note that items being carried or worn by yourself or other creatures are not affected.  This spell is useful for 
getting rid of items that a Wizard has already looked at.
*/
public class DisintegrateSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(11, "Disintegrate", typeof(DisintegrateSpell), 12);
		
	public override string Name => _info.Name;
	
	protected override bool CheckSequence()
	{
		var segment = _caster.Segment;

		if (segment.GetSubregion(_caster.Location) is TownSubregion)
			return false;
			
		return base.CheckSequence();
	}
		
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();
				
			var segment = _caster.Segment;

			// TODO: Check if accounts for darkness
			var visibility = _caster.CurrentVisibility;
			var visibleTiles = visibility.GetVisibleLocations();

			foreach (var location in visibleTiles)
			{
				var tile = segment.FindTile(location);

				if (tile != null && tile.AllowsSpellPath())
				{
					var items = tile.Items.Where(i => i.CanDisintegrate).ToList();

					foreach (var item in items)
						item.Delete();
				}
			}

			if (_caster is PlayerEntity player && _item == null)
				player.AwardMagicSkill(this);

			_caster.EmitSound(232, 3, 6);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}