using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class GriffinJacket : Jacket, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 30;

	/// <inheritdoc />
	public override int Weight => 1500;
		
	/// <inheritdoc />
	/// <remarks>Robes have a default <see cref="Hindrance"/> value of 1.</remarks>
	public override int Hindrance => 0;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="GriffinJacket"/> class.
	/// </summary>
	public GriffinJacket() : base(260)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GriffinJacket"/> class.
	/// </summary>
	public GriffinJacket(Serial serial) : base(serial)
	{
	}
        
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200195)); /* [You are looking at] [a vest made from the feathers of a griffin.] */
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		if (!entity.GetStatus(typeof(BlindResistanceStatus), out var resistance))
		{
			resistance = new BlindResistanceStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 47 }
			};
			resistance.AddSource(new ItemSource(this));
				
			entity.AddStatus(resistance);
		}
		else
		{
			resistance.AddSource(new ItemSource(this));
		}
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);

		if (entity.GetStatus(typeof(BlindResistanceStatus), out var resistance))
			resistance.RemoveSource(this);
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