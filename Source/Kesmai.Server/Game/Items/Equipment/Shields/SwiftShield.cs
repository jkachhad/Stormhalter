using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public partial class SwiftShield : Shield, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 2000;

	/// <inheritdoc />
	public override int Weight => 3000;

	/// <inheritdoc />
	public override int Category => 1;

	/// <summary>
	/// Gets or sets the shield-value provided by this ring.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Shield => 3;
		
	/// <inheritdoc />
	public override int BaseArmorBonus => 2;

	/// <inheritdoc />
	public override int ProjectileProtection => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="SwiftShield"/> class.
	/// </summary>
	public SwiftShield() : base(979)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200385)); /* [You are looking at] [a red and black shield adorned with a griffin. Magical properties are embued within.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6300429)); /* The shield contains the spell of Medium Shield. */
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SwiftShield"/> class.
	/// </summary>
	public SwiftShield(Serial serial) : base(serial)
	{
	}

	public override void OnWield(MobileEntity entity)
	{
		base.OnWield(entity);

		if (!entity.GetStatus(typeof(ShieldStatus), out var status))
		{
			status = new ShieldStatus(entity)
			{
				Inscription = new SpellInscription() { SpellId = 52 }
			};
			status.AddSource(new ShieldStatus.ShieldItemSource(Shield, this));
				
			entity.AddStatus(status);
		}
		else
		{
			status.AddSource(new ShieldStatus.ShieldItemSource(Shield, this));
		}
	}

	public override void OnUnwield(MobileEntity entity)
	{
		base.OnUnwield(entity);

		if (entity.GetStatus(typeof(ShieldStatus), out var status))
			status.RemoveSource(this);
	}
}