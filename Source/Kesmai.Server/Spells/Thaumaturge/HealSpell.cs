using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
	Thaumaturges and Knights can heal physical damage with this spell.  Each time the Cure spell is cast successfully, 
	the recipient of the spell will immediately regain missing hit points.  
	
	When the spell is cast by a Thaumaturge, the recipient will gain (if the gods are willing) a minimum of 60-80% of 
	missing hit points, or a number of hit points equal to twice the Thaumaturge's magic skill level, whichever is 
	greater.  
	
	When the spell is cast by a Knight, the recipient will gain a minimum of 60-80% of missing hit points, or a number 
	of hit points equal to the Knight's experience level, whichever is greater.  The Knight's Cure spell will not fail,
	except for a lack of sufficient magic points.
	
	To cast this spell on yourself, double left click on the spell icon.  The mouse cursor will change to a crosshair; 
	place the crosshair over your character's icon, and left click once to cast the spell. 
	
	To cast the spell on another person, place the crosshair over the target character's icon, and left click once to 
	cast the spell.  Knights, unlike Thaumaturges, must be in the same hex as the player or creature they are healing.
	*/
public class HealSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(6, "Heal", typeof(HealSpell), 3);
		
	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		_caster.Target = new InternalTarget(this);
	}

	public override void OnReset()
	{
		if (_caster.Target is InternalTarget)
			Target.Cancel(_caster);
	}

	public void CastAt(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || !target.IsAlive)
			return;

		if (CheckSequence())
		{
			base.OnCast();
				
			var skillLevel = _skillLevel;
			var heal = 2 * skillLevel;

			var missingHealth = target.MaxHealth - target.Health;

			if (missingHealth > heal)
				missingHealth = (int)(heal + (missingHealth - heal) * Utility.RandomBetween(0.6, 0.8));

			if (missingHealth > 0)
			{
				target.Health += missingHealth;

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);

				if (target != _caster)
				{
					target.SendLocalizedMessage(6300306, _caster.Name);

					var groups = target.GetBeholdersInVisibility();
						
					foreach (var beholder in groups.OfType<PlayerGroup>().SelectMany(g => g.Members))
						beholder.SendLocalizedMessage(6300305, target.Name, _caster.Name);
				}
				else
				{
					target.SendLocalizedMessage(6300307);
				}
			}

			_caster.EmitSound(233, 3, 6);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var match = Regex.Match(arguments, @"^at (.*)$", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			var name = match.Groups[1].Value;
			var entity = default(MobileEntity);

			if (!name.Matches("self", true))
				entity = _caster.FindMobileByName(name);
			else
				entity = _caster;

			if (entity == default(MobileEntity))
				return false;

			CastAt(entity);
		}
		else
		{
			CastAt(_caster);
		}

		return true;
	}
		
	private class InternalTarget : MobileTarget
	{
		private HealSpell _spell;

		public InternalTarget(HealSpell spell) : base(flags: TargetFlags.Beneficial)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastAt(target);
		}
	}
}