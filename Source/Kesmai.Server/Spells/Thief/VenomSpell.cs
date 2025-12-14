using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
A Thief may cast this spell upon a dagger or shuriken to tip the weapon with a lethal venom.  Whether wielded or 
thrown, the weapon will be enhanced by the effects of the poison.  For most victims, this promises a slow and 
painful death, unless an antidote – either magical or natural – is applied.  The lethality of the poison will 
increase as the Thief’s magic skill improves.

To obtain this deadly spell, the Thief must visit a particular master thief trainer in one of the advanced lands.  
It is said that a poisoned dagger marks the secret doorway to the cave where this reclusive trainer resides.

To cast this spell, first warm it, then double click on the spell icon in the warmed-spell rack while holding 
an appropriate weapon in your right hand.  The venom will be visible on the tip of the weapon, where it will 
remain until the weapon draws blood.  Lawful citizens are strictly forbidden from using poisoned weapons.  
Violation of this law will change the offender’s alignment to neutral, thus making them subject to attack by 
the sheriff and his deputies.
*/
public class VenomSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(85, "Venom", typeof(VenomSpell), 10);

	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		OnCast(_castFlags);
	}

	private void OnCast(SpellCastFlags flags)
	{
		if ((flags & SpellCastFlags.Alt) != 0)
			OnCastCommand(_caster, "cast");
		else
			_caster.Target = new InternalTarget(this);
	}
		
	public override void OnReset()
	{
		if (_caster.Target is InternalTarget)
			Target.Cancel(_caster);
	}

	public void CastAt(ItemEntity item)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || item.Deleted)
			return;

		if (CheckSequence())
		{
			if (item is Weapon weapon)
			{
				base.OnCast();
					
				var potency = _skillLevel + 3;

				// TODO: What else can be poisoned?? Black Rapier??
				if (weapon is Dagger || weapon is Shuriken)
					weapon.Poison = new Venom((int)potency);

				_caster.EmitSound(233, 3, 6);

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
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
		
	private static Regex _castMatch = new Regex(@"^on (left|right)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var rightHand = _caster.RightHand;
		var leftHand = _caster.LeftHand;
		var belt = _caster.Belt;

		var target = default(ItemEntity);
			
		if (_castMatch.TryGetMatch(arguments, out var match))
		{
			var hand = match.Groups[1].Value;

			if (hand.Matches("left", true))
				target = leftHand;
			else
				target = rightHand;
		}
		else
		{
			if (rightHand is Weapon rWeapon && !rWeapon.IsPoisoned)
			{
				target = rWeapon;
			}
			else if (leftHand is Weapon lWeapon && !lWeapon.IsPoisoned)
			{
				target = lWeapon;
			}
			else if (belt != null)
			{
				if (belt.OfType<Dagger>().Any(d => !d.IsPoisoned, out Dagger dagger))
					target = dagger;
				else if (belt.OfType<MeleeWeapon>().Any(w => !w.IsPoisoned, out MeleeWeapon weapon))
					target = weapon;
			}
		}
			
		if (target is null)
			OnCast(SpellCastFlags.None);
		else
			CastAt(target);
		
		return true;
	}

	private class InternalTarget : Target
	{
		private VenomSpell _spell;
			
		public InternalTarget(VenomSpell spell) : base(0, TargetFlags.Items)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, object target)
		{
			if (source.Spell != _spell)
				return;
				
			if (target is ItemEntity item)
				_spell.CastAt(item);
		}
	}
}

public class Venom : Poison
{
	public Venom(int potency) : base(TimeSpan.Zero, potency, true)
	{
	}

	public override Poison Clone()
	{
		return new Venom(Potency);
	}
}
