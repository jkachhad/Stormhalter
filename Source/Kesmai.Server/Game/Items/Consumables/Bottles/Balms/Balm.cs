using System;
using System.IO;

namespace Kesmai.Server.Game;

public abstract class Balm : Bottle
{
	private static ConsumableBalm content = new ConsumableBalm();
		
	/// <inheritdoc />
	public override int Category => 12;

	/// <summary>
	/// Initializes a new instance of the <see cref="Balm"/> class.
	/// </summary>
	protected Balm(int closedId, int openId) : base(closedId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Balm"/> class.
	/// </summary>
	protected Balm(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	protected override void OnCreate()
	{
		base.OnCreate();

		if (_content is null)
			_content = content;
	}
	
	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
	}

	/// <inheritdoc />
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

public class BalmTimer : FacetTimer
{
	private int _restored;
	private int _healthPerTick;

	private MobileEntity _entity;

	/// <summary>
	/// Initializes a new instance of the <see cref="BalmTimer"/> class.
	/// </summary>
	public BalmTimer(MobileEntity entity, int healthPerTick = 14) : base(entity.Facet, TimeSpan.Zero, TimeSpan.FromSeconds(1.0))
	{
		_entity = entity;
		_healthPerTick = healthPerTick;
	}

	/// <summary>
	/// Called when this timer has been triggered.
	/// </summary>
	protected override void OnExecute()
	{
		if (_entity != null && !_entity.Deleted)
		{
			/* If the balm can't heal anymore, we clear the timer. */
			var clearTimer = (_healthPerTick is 0);
				
			var currentHealth = _entity.Health;
			var maxHealth = _entity.MaxHealth;

			/* Only heal if there are ticks remaining, or not at max health.*/
			if (currentHealth < maxHealth && _healthPerTick > 0 && _entity.IsAlive)
			{
				_entity.Health += _healthPerTick;
				_restored += _healthPerTick;

				if (_restored >= maxHealth)
					_healthPerTick--;

				if (_entity.Health > maxHealth)
					_entity.Health = maxHealth;
			}

			if (_entity.Health >= maxHealth)
				clearTimer = true;

			if (clearTimer)
				_entity.BalmTimer = null;
		}
	}
}