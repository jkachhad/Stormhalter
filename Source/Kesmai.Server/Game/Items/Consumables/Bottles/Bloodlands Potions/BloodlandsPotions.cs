using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game;

public class ForestBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="ForestBlood"/> class.
	/// </summary>
	public ForestBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ForestBlood"/> class.
	/// </summary>
	public ForestBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200375); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class KeepBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="KeepBlood"/> class.
	/// </summary>
	public KeepBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="KeepBlood"/> class.
	/// </summary>
	public KeepBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200362); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class LandBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="LandBlood"/> class.
	/// </summary>
	public LandBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LandBlood"/> class.
	/// </summary>
	public LandBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200363); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class ShadowBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="ShadowBlood"/> class.
	/// </summary>
	public ShadowBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ShadowBlood"/> class.
	/// </summary>
	public ShadowBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200364); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class CorruptedBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="CorruptedBlood"/> class.
	/// </summary>
	public CorruptedBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CorruptedBlood"/> class.
	/// </summary>
	public CorruptedBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200365); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class MakersBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="MakersBlood"/> class.
	/// </summary>
	public MakersBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MakersBlood"/> class.
	/// </summary>
	public MakersBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200366); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class UshersBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="UshersBlood"/> class.
	/// </summary>
	public UshersBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UshersBlood"/> class.
	/// </summary>
	public UshersBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200367); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class FeralBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="FeralBlood"/> class.
	/// </summary>
	public FeralBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FeralBlood"/> class.
	/// </summary>
	public FeralBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200368); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class FlameBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="FlameBlood"/> class.
	/// </summary>
	public FlameBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FlameBlood"/> class.
	/// </summary>
	public FlameBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200369); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class AirBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="AirBlood"/> class.
	/// </summary>
	public AirBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AirBlood"/> class.
	/// </summary>
	public AirBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200370); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class IceBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="IceBlood"/> class.
	/// </summary>
	public IceBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IceBlood"/> class.
	/// </summary>
	public IceBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200371); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class WaterBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="WaterBlood"/> class.
	/// </summary>
	public WaterBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WaterBlood"/> class.
	/// </summary>
	public WaterBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200372); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class RegenerationBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="RegenerationBlood"/> class.
	/// </summary>
	public RegenerationBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RegenerationBlood"/> class.
	/// </summary>
	public RegenerationBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200373); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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

public class DeadBlood : Bottle
{
	private static ConsumableWater content = new ConsumableWater();

	/// <inheritdoc />
	public override uint BasePrice => 5;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeadBlood"/> class.
	/// </summary>
	public DeadBlood() : base(210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeadBlood"/> class.
	/// </summary>
	public DeadBlood(Serial serial) : base(serial)
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
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200374); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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