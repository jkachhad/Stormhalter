using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BlueStaff : Staff, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1500;

		/// <inheritdoc />
		public override int Weight => 500;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BlueStaff"/> class.
		/// </summary>
		public BlueStaff() : base(38)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200140)); /* [You are looking at] [a long, surprisingly light staff composed of a bluish metal.] */
		}
	}
}