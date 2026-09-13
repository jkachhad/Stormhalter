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
	public ForestBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ForestBlood"/> class.
	/// </summary>
	public ForestBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200375)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public KeepBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="KeepBlood"/> class.
	/// </summary>
	public KeepBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200362)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public LandBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LandBlood"/> class.
	/// </summary>
	public LandBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200363)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public ShadowBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ShadowBlood"/> class.
	/// </summary>
	public ShadowBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200364)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public CorruptedBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CorruptedBlood"/> class.
	/// </summary>
	public CorruptedBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200365)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public MakersBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MakersBlood"/> class.
	/// </summary>
	public MakersBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200366)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public UshersBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UshersBlood"/> class.
	/// </summary>
	public UshersBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200367)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public FeralBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FeralBlood"/> class.
	/// </summary>
	public FeralBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200368)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public FlameBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FlameBlood"/> class.
	/// </summary>
	public FlameBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200369)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public AirBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AirBlood"/> class.
	/// </summary>
	public AirBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200370)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public IceBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IceBlood"/> class.
	/// </summary>
	public IceBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200371)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public WaterBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WaterBlood"/> class.
	/// </summary>
	public WaterBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200372)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public RegenerationBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RegenerationBlood"/> class.
	/// </summary>
	public RegenerationBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200373)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
	public DeadBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 210)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeadBlood"/> class.
	/// </summary>
	public DeadBlood(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200374)); /* [You are looking at] [a clear glass bottle.] */

		base.GetDescription(entries);
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
