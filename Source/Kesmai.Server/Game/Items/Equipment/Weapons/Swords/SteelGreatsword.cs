using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class SteelGreatsword : Greatsword
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
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200153)); /* [You are looking at an unusually] [light shining steel greatsword.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
	}
}