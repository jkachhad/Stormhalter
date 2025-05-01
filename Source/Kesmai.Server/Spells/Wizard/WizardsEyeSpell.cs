using System;
using System.Drawing;
using System.IO;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class WizardsEyeSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(58, "Wizard's Eye", typeof(WizardsEyeSpell), 13);
		
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

			var frog = new SummonedFrog()
			{
				MaxHealth = 1, Health = 1,
				Movement = 3,

				Alignment = Alignment.Lawful,
					
				VisibilityDistance = 0,
					
				CanSwim = true,
			};
			frog.Brain = new IdleAI(frog);

			frog.Summoned = true;
			frog.Alignment = _caster.Alignment;

			CreatureGroup.Instantiate(frog, facet, segment, location);

			if (_caster is PlayerEntity player)
			{
				/* We'll have to delay the transfer while the server processes delta packets. If we transfer
				 * the control instantly, there creature list becomes corrupted. The server will send an
				 * incoming entity after control transfer, resulting in extra player or creature entries.
				 */
				facet.Schedule(TimeSpan.FromMilliseconds(5), () => player.Peek(frog));

				if (_item == null)
					player.AwardMagicSkill(this);
			}

			_caster.Steering = frog;
			_caster.EmitSound(226, 3, 6);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}

public class SummonedFrog : Frog
{
	private Timer _deathTimer;
		
	public SummonedFrog()
	{
	}
		
	public SummonedFrog(Serial serial) : base(serial)
	{
	}

	public override void OnEnterWorld()
	{
		base.OnEnterWorld();

		if (_deathTimer != null)
			_deathTimer.Stop();
			
		_deathTimer = Facet.Schedule(WizardsEyeSpell.Duration, Kill);
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