using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class HummingbirdSword : ShortSword, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 2000;
		
	/// <inheritdoc />
	public override int BaseAttackBonus => 4;

	/// <inheritdoc />
	public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.Lawful;
		
	/// <inheritdoc />
	public override bool CanBind => true;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="HummingbirdSword"/> class.
	/// </summary>
	public HummingbirdSword() : base(306)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HummingbirdSword"/> class.
	/// </summary>
	public HummingbirdSword(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200281); /* [a fine sword made of an otherworldly metal. Looking at the blade makes you dizzy, as if you were looking at the wings of a hummingbird. The longsword is emitting a faint blue glow.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250003); /* The combat adds for this weapon are +4. */
	}

#if (Alpha)
		protected override bool OnDroppedInto(MobileEntity entity, Container container, int slot)
		{
			if (entity != null)
				entity.Delta(MobileDelta.Body);
			
			return base.OnDroppedInto(entity, container, slot);
		}
#endif

	/* The swing delay for the sword is 1/2 the round timer. The skill gained per swing should
	 * be adjusted. */

	public override TimeSpan GetSwingDelay(MobileEntity entity)
	{
		return entity.GetRoundDelay(0.5);
	}

	public override double GetSkillMultiplier()
	{
		return 0.5;
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
