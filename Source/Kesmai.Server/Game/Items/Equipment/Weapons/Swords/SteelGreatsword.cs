using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class SteelGreatsword : Greatsword
{
	/// <inheritdoc />
	public override uint BasePrice => 2000;
		
	/// <inheritdoc />
	public override int BaseAttackBonus => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="SteelGreatsword"/> class and serializes it.
	/// </summary>
	public SteelGreatsword() : base(161)
	{
	}
	public SteelGreatsword(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200153); /* [an unusually] [light shining steel greatsword.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250003); /* The combat adds for this weapon are +4. */
	}

}
