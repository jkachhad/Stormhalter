using System;
using System.Collections.Generic;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
A Knight may magically enchant a weapon, dramatically increasing its effectiveness.  This spell need only be 
thrown once on a weapon to bring the weapon to its maximum magical potency.  The spell will work only on an 
item that originally was a normal weapon.  

It will fail on armor, flame staves, potions and any other esoteric items.  You must be holding the weapon you 
wish to enchant in your right hand when you cast the spell.  Double left click on the spell icon to cast the spell.

The minimum experience level required to enchant an item depends on the type of item; in essence, the power 
required depends on the amount and kind of metal in the weapon:

Exp.

Level	Weapons that can be enchanted

8		Mace, hammer
9		Sword, dagger, rapier, shuriken
10		Greatsword
11		Halberd

Be forewarned that casting enchant weapon on an already enchanted weapon can have dire consequences.  Enchanting 
feeds the weapon with part of the Knight’s soul, and therefore, that weapon can be used by only that Knight.  
Excessive use of the Enchant spell will lead to premature aging.
*/
public class InstantEnchantWeaponSpell : InstantSpell
{
	private static SpellInfo _info = new SpellInfo(13, "Enchant Weapon", typeof(InstantEnchantWeaponSpell), 3);

	private Dictionary<Type, int> _enchantTable = new Dictionary<Type, int>()
	{
		[typeof(Axe)] = 8,
		[typeof(Mace)] = 8,
			
		[typeof(Sword)] = 9,
		[typeof(Dagger)] = 9,
		[typeof(Rapier)] = 9,
		[typeof(Shuriken)] = 9,
			
		[typeof(Greatsword)] = 10,
			
		[typeof(Halberd)] = 11,
		[typeof(Flail)] = 11,
		[typeof(Staff)] = 11,
			
		[typeof(ProjectileWeapon)] = 12,
	};
	
	public override string Name => _info.Name;
		
	public override bool AllowInterrupt => false;

	protected override void OnCast()
	{
		_caster.Target = new InternalTarget(this);
	}
		
	public void CastOn(ItemEntity item)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || item.Deleted)
			return;
			
		if (CheckSequence())
		{
			if (item is Weapon weapon && _caster is PlayerEntity player 
			                          && GetRequiredLevel(weapon, out var levelRequired) && player.Level >= levelRequired)
			{
				base.OnCast();

				_caster.SendLocalizedMessage(6300308);
				_caster.EmitSound(233, 3, 6);
					
				weapon.IsEnchanted = true;
			}
			else
			{
				Fizzle();
			}
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}

	private bool GetRequiredLevel(Weapon weapon, out int levelRequired)
	{
		var weaponType = weapon.GetType();
			
		foreach (var (type, level) in _enchantTable)
		{
			if (!type.IsAssignableFrom(weaponType))
				continue;
			
			levelRequired = level;
			return true;
		}
		
		levelRequired = Int32.MaxValue;
		return false;
	}
		
	private class InternalTarget : Target
	{
		private InstantEnchantWeaponSpell _spell;
			
		public InternalTarget(InstantEnchantWeaponSpell spell) : base(0, TargetFlags.Items)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, object target)
		{
			if (source.Spell != _spell)
				return;
				
			if (target is ItemEntity item)
				_spell.CastOn(item);
		}
			
		protected override void OnTargetCancel(MobileEntity source, TargetCancel cancelType)
		{
			base.OnTargetCancel(source, cancelType);

			if (source.Spell != _spell)
				return;
				
			_spell.Cancel();
		}
	}
}
