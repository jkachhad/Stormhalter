using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	[WorldForge]
	public partial class OakGroveBow : Bow, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000058;

		/// <inheritdoc />
		public override uint BasePrice => 6000;

		/// <inheritdoc />
		public override int Weight => 800;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 6;

		/// <inheritdoc />
		public override int MaximumDamage => 17;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.Silver 
		                                                | WeaponFlags.BlueGlowing | WeaponFlags.Neutral;

		/// <inheritdoc />
		public override bool CanBind => true;

		//todo pick a new bow model
		/// <inheritdoc />
		public override int NockedID => 113;

		/// <summary>
		/// Initializes a new instance of the <see cref="OakGroveBow"/> class.
		/// </summary>
		public OakGroveBow() : base(229)
		{
			Hue = Color.Yellow;
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6300405)); /* [You are looking at] [a longbow made of faint blue wood. The string appears to be woven from strands of silver.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6300406)); /* It has combat adds of +5 and is neutral in alignment. */
		}

		public override TimeSpan GetSwingDelay(MobileEntity entity)
		{
			return entity.GetRoundDelay(0.75);
		}
	}
}