using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public partial class TemporaryBullStrengthPotion : Bottle, ITreasure
{
	private static ConsumableStrengthSpell content = new ConsumableStrengthSpell();
		
	/// <inheritdoc />
	public override uint BasePrice => 200;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="TemporaryBullStrengthPotion"/> class.
	/// </summary>
	public TemporaryBullStrengthPotion() : this(234, 109)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="TemporaryBullStrengthPotion"/> class.
	/// </summary>
	public TemporaryBullStrengthPotion(int closedId, int openId) : base(closedId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="TemporaryBullStrengthPotion"/> class.
	/// </summary>
	public TemporaryBullStrengthPotion(Serial serial) : base(serial)
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
	{			entries.Add(new LocalizationEntry(6200000, 6200384)); /* [You are looking at] [a steel bottle stamped with the icon of a bull.] [Inside is a dark crimson liquid that smells of earth and mushrooms.] */

		base.GetDescription(entries);
	}
}