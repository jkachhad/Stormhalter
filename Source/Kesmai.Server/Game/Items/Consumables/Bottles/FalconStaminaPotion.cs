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
	public FalconStaminaPotion(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: base(entityProvider, entityDependencies, 276)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="FalconStaminaPotion"/> class.
	/// </summary>
	public FalconStaminaPotion(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200383)); /* [You are looking at] [a burnished steel bottle with the icon of a falcon.] [Inside is a clear jade-colored liquid that smells of jasmine.] */

		base.GetDescription(entries);
	}
}
