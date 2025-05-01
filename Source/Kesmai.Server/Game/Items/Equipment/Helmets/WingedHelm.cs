using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class WingedHelm : Helmet, ITreasure
{
	private bool _provideNightVision;
		
	/// <inheritdoc />
	public override int LabelNumber => 6000049;

	/// <inheritdoc />
	public override uint BasePrice => 800;

	/// <inheritdoc />
	public override int Weight => 80;

	/// <inheritdoc />
	public override int ProtectionFromDaze => 10;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 0;
		
	/// <inheritdoc />
	public override int ProtectionFromIce => 0;
		
	/// <inheritdoc />
	public override bool ProvidesNightVision => _provideNightVision;

	/// <summary>
	/// Initializes a new instance of the <see cref="WingedHelm"/> class.
	/// </summary>
	public WingedHelm() : this(true)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="WingedHelm"/> class.
	/// </summary>
	public WingedHelm(Serial serial) : base(serial)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="WingedHelm"/> class.
	/// </summary>
	public WingedHelm(bool provideNightVision) : base(33)
	{
		_provideNightVision = provideNightVision;
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200043)); /* [You are looking at] [a steel helm set with white wings.] */

		if (Identified)
		{
			entries.Add(
				new LocalizationEntry(ProvidesNightVision 
					? 6250104 /* The helm contains the spell of Night Vision. */
					: 6250033)); /* The helm appears quite ordinary. */
		}
	}
	
	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)2); /* version */
			
		writer.Write(_provideNightVision);
	}

	/// <inheritdoc />
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 2:
			{
				_provideNightVision = reader.ReadBoolean();
				goto case 1;
			}
			case 1:
			{
				break;
			}
		}
	}
}