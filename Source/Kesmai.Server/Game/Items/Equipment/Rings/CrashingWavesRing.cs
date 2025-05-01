using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public partial class CrashingWavesRing : Ring, ITreasure
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 300;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 20;

	/// <summary>
	/// Initializes a new instance of the <see cref="CrashingWavesRing"/> class.
	/// </summary>
	public CrashingWavesRing() : base(395)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CrashingWavesRing"/> class.
	/// </summary>
	public CrashingWavesRing(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200283)); /* [You are looking at] [a ring with crashing ocean waves.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250129)); /* The ring contains a spell of Water Walking. */
	}

	protected override bool OnEquip(MobileEntity entity)
	{
		if (!base.OnEquip(entity))
			return false;

		if (!entity.GetStatus(typeof(WaterWalkingStatus), out var status))
		{
			status = new WaterWalkingStatus(entity);
			status.AddSource(new ItemSource(this));
				
			entity.AddStatus(status);
		}
		else
		{
			status.AddSource(new ItemSource(this));
		}

		return true;
	}

	protected override bool OnUnequip(MobileEntity entity)
	{
		if (!base.OnUnequip(entity))
			return false;
			
		if (entity.GetStatus(typeof(WaterWalkingStatus), out var status))
			status.RemoveSource(this);

		return true;
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
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
	
public class WaterWalkingStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;

	public override bool Hidden => true;

	public WaterWalkingStatus(MobileEntity entity) : base(entity)
	{
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, "Water Walking"); /* The spell of [Water Walking] has worn off. */
		}
	}
}