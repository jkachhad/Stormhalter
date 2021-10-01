using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public class RazLongbow : Bow, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000058;

		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int Weight => 800;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 6;

		/// <inheritdoc />
		public override int MaximumDamage => 16;

		/// <inheritdoc />
		public override int BaseAttackBonus => 7;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.Silver;

		/// <inheritdoc />
		public override bool CanBind => true;

		//todo pick a new bow model
		/// <inheritdoc />
		public override int NockedID => 113;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazLongbow"/> class.
		/// </summary>
		public RazLongbow() : base(229)
		{
			Hue = Color.Yellow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazLongbow"/> class.
		/// </summary>
		public RazLongbow(Serial serial) : base(serial)
		{
		}

		public override bool CanUse(MobileEntity entity)
		{

			var flags = Flags;

			if (entity.LeftHand != null && flags.HasFlag(WeaponFlags.TwoHanded))
				return false; */


			if (!CanUse(entity.Alignment))
				return false;

			if (entity is PlayerEntity player && player.Profession is Thief)
				return true;

			return false;
		}

		public override bool HandleInteraction(MobileEntity entity, ActionType action)
		{
			if (action != ActionType.Nock && action != ActionType.Shoot)
				return base.HandleInteraction(entity, action);

			if (action == ActionType.Nock && !IsNocked)
			{
				if (!entity.CanPerformAction)
					return false;

				if (!entity.HasFreeHand(out var slot))
				{
					entity.SendLocalizedMessage(Color.Red, 6100020); /* You must have a free hand to do that. */
					return false;
				}

				Nock();

				var nockSound = NockSound;

				if (nockSound > 0)
					entity.EmitSound(nockSound, 3, 6);

				entity.GetRoundDelay(0.3);
				return true;
			}

			if (action == ActionType.Shoot && IsNocked)
			{
				entity.Target = new ShootItemTarget(this);
				return true;
			}

			return false;
		}

		public override void OnHit(MobileEntity attacker, MobileEntity defender)
		{
			if (Utility.RandomDouble() <= 0.25)
            {
				var spell = new LightningBoltSpell()
				{
					SkillLevel = 12,
					Cost = 0,
				};
				spell.Warm(attacker);
				spell.CastAt(defender);
			}
			base.OnHit(attacker, defender);
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500005)); /* [You are looking at] [a yew longbow. It has a faint blue glow and tingles to touch it.] */

			//todo The longbow brings lightning from the heavens
			if (Identified)
				entries.Add(new LocalizationEntry(6500006)); /* The longbow has some magical properties, electricity crackles around the string. */
		}

		/// <inheritdoc />
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)1); /* version */
		}

		/// <inheritdoc />
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

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
}