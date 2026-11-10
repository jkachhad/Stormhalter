using System;
using System.Globalization;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public abstract class Dagger : MeleeWeapon
{
	/// <inheritdoc />
	public override int LabelNumber => 6000032;

	/// <inheritdoc />
	public override int Weight => 100;
		
	/// <inheritdoc />
	public override int Category => 14;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 4;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;

	/// <inheritdoc />
	public override Skill Skill => Skill.Dagger;

	public double PoisonMultiplier { get; set; } = 1.0;

	/// <inheritdoc />
	public override void GetTooltipEntries(PlayerEntity beholder, Tooltip tooltip)
	{
		base.GetTooltipEntries(beholder, tooltip);

		// OnHit multiplies the potency of the venom it delivers.
		if (PoisonMultiplier > 1)
			tooltip.AddEffect(TooltipEffectTrigger.Hit, 6610041, PoisonMultiplier.ToString("0.#", CultureInfo.InvariantCulture)); /* Venom on this dagger poisons at {0} times its potency. */
	}

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Piercing | WeaponFlags.Throwable | WeaponFlags.QuickThrow;

	/// <inheritdoc />
	public override int AttackSound => 45;

	public override int ItemId
	{
		get
		{
			if (_poison != null)
				return PoisonedItemId;
					
			return base.ItemId;
		}
	}
	public override TimeSpan GetSwingDelay(MobileEntity entity)
	{
		return entity.GetRoundDelay(0.75);
	}
	
	public override void OnHit(MobileEntity attacker, MobileEntity defender)
	{
		if (IsPoisoned && PoisonMultiplier > 1)
		{
			var newPotency = Poison.Potency * PoisonMultiplier;
			Poison.Potency = (int)newPotency;
		}

  	        base.OnHit(attacker,defender);
	}
		
	public override double GetSkillMultiplier()
	{
		return 0.75;
	}
		
	/// <summary>
	/// Gets the item id for this weapon when poisoned.
	/// </summary>
	protected abstract int PoisonedItemId { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Dagger"/> class.
	/// </summary>
	protected Dagger(int daggerID) : base(daggerID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Dagger"/> class.
	/// </summary>
	protected Dagger(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
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
