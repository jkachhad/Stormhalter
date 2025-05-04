using System.Drawing;
using Kesmai.Server.Game;
using Kesmai.Server.Items;

namespace Kesmai.Server.Spells;

public class MakeRecallRingSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(34, "Make Recall Ring", typeof(MakeRecallRingSpell), 7)
	{
		Intensities = (entity) =>
		{
			var minimum = (entity.GetSkillLevel(Skill.Magic) >= 10 ? 1 : default(int?));
			var maximum = (entity.GetSkillLevel(Skill.Magic) >= 10 ? 2 : default(int?));

			return (minimum, 1, maximum);
		},
	};
	
	public override string Name => _info.Name;

	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();
				
			var recallRing = new RecallRing(_intensity)
			{
				IsConjured = true,
			};
				
			var leftHand = _caster.LeftHand;

			_caster.EmitSound(232, 3, 6);

			if (_caster is PlayerEntity player && _item == null)
				player.AwardMagicSkill(this);

			if (leftHand != null || !_caster.CanCarry(recallRing, 1))
			{
				var segment = _caster.Segment;
				var location = _caster.Location;

				recallRing.Move(location, true, segment);
			}
			else
			{
				var hands = _caster.Hands;

				if (hands != null)
					hands.AddItem(recallRing, 0);
			}
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}