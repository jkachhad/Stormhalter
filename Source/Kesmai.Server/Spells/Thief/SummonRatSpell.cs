using System;
using System.IO;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class SummonRatSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(91, "Summon Rat", typeof(SummonRatSpell), 13);
		
	public static TimeSpan Duration = TimeSpan.FromMinutes(20.0);
		
	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || _caster.IsSteering)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();
				
			var facet = _caster.Facet;
			var segment = _caster.Segment;
			var location = _caster.Location;

			var rat = new SummonedRat()
			{
				MaxHealth = 1, Health = 1,
				Movement = 3,

				Alignment = Alignment.Neutral,

				VisibilityDistance = 0,
					
				CanSwim = true,
			};
			rat.Brain = new IdleAI(rat);
			rat.Summoned = true;

			CreatureGroup.Instantiate(rat, facet, segment, location);
				
			if (_caster is PlayerEntity player)
			{
				/* We'll have to delay the transfer while the server processes delta packets. If we transfer
				 * the control instantly, there creature list becomes corrupted. The server will send an
				 * incoming entity after control transfer, resulting in extra player or creature entries.
				 */
				Timer.DelayCall(TimeSpan.FromMilliseconds(5), () => player.Peek(rat));

				if (_item == null)
					player.AwardMagicSkill(this);
			}

			_caster.Steering = rat;
			_caster.EmitSound(237, 3, 6);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}
	
public class SummonedRat : BlackRat
{
	private Timer _deathTimer;
		
	public SummonedRat()
	{
	}
		
	public SummonedRat(Serial serial) : base(serial)
	{
	}

	public override void OnEnterWorld()
	{
		base.OnEnterWorld();

		if (_deathTimer != null)
			_deathTimer.Stop();
			
		_deathTimer = Timer.DelayCall(Facet.TimeSpan.FromTimeSpan(SummonRatSpell.Duration), Kill);
	}

	public override void OnDeath()
	{
		base.OnDeath();

		if (_deathTimer != null)
			_deathTimer.Stop();

		_deathTimer = null;
	}

	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
	}

	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}
