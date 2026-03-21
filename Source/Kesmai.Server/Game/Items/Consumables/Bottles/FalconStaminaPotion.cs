using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class FalconStaminaPotion : Bottle
{
	/// <inheritdoc />
	public override uint BasePrice => 40;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="FalconStaminaPotion"/> class.
	/// </summary>
	public FalconStaminaPotion() : base(276)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="FalconStaminaPotion"/> class.
	/// </summary>
	public FalconStaminaPotion(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	protected override void OnCreate()
	{
		base.OnCreate();

		if (_content is null)
			_content = ConsumableRestoreStamina.Full;
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200383); /* [a burnished steel bottle with the icon of a falcon.] [Inside is a clear jade-colored liquid that smells of jasmine.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
	}
}