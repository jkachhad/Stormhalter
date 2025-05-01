using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class SummonPhantasmSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(38, "Summon Phantasm", typeof(SummonPhantasmSpell), 20)
	{
		Intensities = (entity) =>
		{
			var spellPower = (int)entity.GetSkillLevel(Skill.Magic);
			var minimum = 1;
			var maximum = 5;
				
			switch (spellPower)
			{
				case 11:
				case 12: maximum = 5; break;
				case 13: maximum = 6; break;
				case 14: maximum = 7; break;
				case 15:
				case 16: maximum = 8; break;
				case 17: 
				case 18: maximum = 9; break;
				case 19: maximum = 10; break;
			}

			return (minimum, 5, maximum);
		}
	};
	
	public override string Name => _info.Name;
		
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			var segment = _caster.Segment;
			var location = _caster.Location;
			var tile = segment.FindTile(location);

			if (tile != null && tile.AllowsMovementPath())
			{
				var intensity = 5;

				if (_intensity > 0)
					intensity = _intensity;

				var phantasm = default(CreatureEntity);

				switch (intensity)
				{
					case int value when value <= 5: phantasm = new SummonedPhantom(); break; 
					case 6: phantasm = new SummonedDjinn(new CreatureSpell<IceStormSpell>(10)); break;
					case 7: phantasm = new SummonedDjinn(new CreatureSpell<ConcussionSpell>(10)); break;
					case 8: phantasm = new SummonedSalamander(new CreatureSpell<FirewallSpell>(8)); break;
					case 9:
					{
						phantasm = new SummonedEfreet(new CreatureSpell<WhirlwindSpell>(10, 20)
						{
							OnConstructed = (spell) => { spell.Strength = 2; }
						});
						break;
					}
					case 10: phantasm = new SummonedEfreet(new CreatureSpell<DragonBreathFireSpell>(10)); break;
				}

				if (phantasm != default(CreatureEntity))
				{
					base.OnCast();
						
					if (_caster is PlayerEntity pCaster)
					{
						var summons = pCaster.Followers.Where(f => f.Summoned).ToList();

						if (summons.Count >= 3)
						{
							/* Take an additional summon since we will be adding one. */
							foreach (var entity in summons.Take(summons.Count - 2))
								entity.Kill();
						}
							
						phantasm.Director = pCaster;
					}

					phantasm.Alignment = _caster.Alignment;
					phantasm.Summoned = true;
						
					CreatureGroup.Instantiate(phantasm, _caster.Facet, segment, location);
						
					phantasm.EmitSound(phantasm.GetNearbySound(), 3, 6);
						
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
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}