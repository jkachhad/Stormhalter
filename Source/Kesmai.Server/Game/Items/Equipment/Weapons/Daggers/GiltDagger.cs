using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class GiltDagger : Dagger
{
	/// <inheritdoc />
	public override uint BasePrice => 500;

	/// <inheritdoc />
	public override int MinimumDamage => 3;

	/// <inheritdoc />
	public override int MaximumDamage => 7;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override int BaseAttackBonus => 3;
		
	/// <inheritdoc />
	protected override int PoisonedItemId => 312;

	/// <summary>
	/// Initializes a new instance of the <see cref="GiltDagger"/> class.
	/// </summary>
	public GiltDagger() : base(171)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="GiltDagger"/> class.
	/// </summary>
	public GiltDagger(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200155)); /* [You are looking at] [a gilt-handled dagger with a fine steel blade.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250007)); /* The combat adds for this weapon are +3. */
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