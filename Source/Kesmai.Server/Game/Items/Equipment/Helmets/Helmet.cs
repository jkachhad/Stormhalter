using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public abstract class Helmet : Equipment
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000049;

	/// <summary>
	/// Gets the item category.
	/// </summary>
	public override int Category => 5;

	/// <summary>
	/// Gets or sets a value indication if this instance provides <see cref="NightVisionStatus"/>
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool ProvidesNightVision => false;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Helmet"/> class.
	/// </summary>
	protected Helmet(int helmetID) : base(helmetID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Helmet"/> class.
	/// </summary>
	protected Helmet(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		if (ProvidesNightVision)
		{
			if (!entity.GetStatus(typeof(NightVisionStatus), out var status))
			{
				status = new NightVisionStatus(entity)
				{
					Inscription = new SpellInscription { SpellId = 36 }
				};
				status.AddSource(new ItemSource(this));

				entity.AddStatus(status);
			}
			else
			{
				status.AddSource(new ItemSource(this));
			}
		}
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);

		if (ProvidesNightVision)
		{
			if (entity.GetStatus(typeof(NightVisionStatus), out var status))
				status.RemoveSource(this);
		}
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