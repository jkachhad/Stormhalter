using System;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

/*
Thaumaturges use this spell to chase away Undead creatures such as skeletons, wights, and wraiths.  Undead 
creatures can actually be destroyed by this spell, depending on the magic skill level of the Thaumaturge.  

First warm the spell, and then double left click once on the spell icon to cast the spell.  All the Undead within 
view of the Thaumaturge can be affected.  The higher the magic skill level of the Thaumaturge, the more powerful 
the type of Undead that can be influenced by this spell. 
*/
public class TurnUndeadSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(55, "Turn Undead", typeof(TurnUndeadSpell), 5);
		
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
				
			_caster.EmitSound(229, 3, 6);
				
			/* Calculate health difference */
			var missingHealth = (_caster.MaxHealth - _caster.Health);
			var missingMana = (_caster.MaxMana - _caster.Mana);

			var damageMultiplier = 2;
			var damage = missingHealth * damageMultiplier;

			if (damage > 0)
			{
				var mobiles = _caster.GetBeheldInVisibility()
					.SelectMany(g => g.Members)
					.Where(e => e is not PlayerEntity && e is IUndead).ToList();

				foreach (var defender in mobiles)
					defender.ApplySpellDamage(_caster, this, damage, true);

				/* Apply return mana. */
				var returnMana = Math.Min(missingHealth / 3, mobiles.Count);

				if ((_caster.Mana + returnMana) > _caster.MaxMana)
					returnMana = missingMana;

				_caster.Mana += returnMana;
					
				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}