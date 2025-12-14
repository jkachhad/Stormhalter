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
public class InstantCureSpell : InstantSpell
{
	private static SpellInfo _info = new SpellInfo(100, "Cure", typeof(InstantCureSpell), 3);
	
	public override string Name => _info.Name;

	public override bool AllowInterrupt => false;

	protected override void OnCast()
	{
		_caster.Target = new InternalTarget(this);
	}

	public virtual void CastAt(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || !target.IsAlive)
			return;
			
		if (CheckSequence())
		{
			var distance = _caster.GetDistanceToMax(target.Location);

			if (distance > 0)
			{
				Fizzle();
			}
			else
			{
				base.OnCast();

				var heal = 0d;

				if (_caster is PlayerEntity p)
				{
					heal = (p.Profession != Profession.Knight) ? 2 * _skillLevel : p.Level;
				}

				var missingHealth = target.MaxHealth - target.Health;

				if (missingHealth > heal)
					missingHealth = (int)(heal + (missingHealth - heal) * Utility.RandomBetween(0.6, 0.8));

				_caster.EmitSound(234, 3, 6);

				if (missingHealth > 0)
				{
					target.Health += missingHealth;

					if (target != _caster)
					{
						target.SendLocalizedMessage(6300306, _caster.Name); /* You have been healed by {0}. */

						/* Healing message should be sent to entities in LOS. */
						var beholders = target.GetBeholdersInVisibility().OfType<PlayerGroup>();

						foreach (var player in beholders.SelectMany(g => g.Members))
							player.SendLocalizedMessage(6300305, target.Name, _caster.Name);
					}
					else
					{
						target.SendLocalizedMessage(6300307);
					}
				}
			}
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
				entity = source.FindMobileByName(name);
			else
				entity = source;

			if (entity == default(MobileEntity))
				return false;

			Warm(source);
			CastAt(entity);
			return true;
		}

		return base.OnCastCommand(source, arguments);
	}
		
	private class InternalTarget : MobileTarget
	{
		private InstantCureSpell _spell;

		public InternalTarget(InstantCureSpell spell) : base(flags: TargetFlags.Beneficial)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastAt(target);
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
