using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public abstract partial class Armor : Equipment, IArmored
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000004; /* armor */
		
		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 9;
		
		/// <inheritdoc />
		public override int Hindrance => 1;
		
		#region IArmored
		
		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BaseArmorBonus => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int SlashingMitigation => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int PiercingMitigation => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BashingMitigation => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProjectileMitigation => 0;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Armor"/> class.
		/// </summary>
		protected Armor(int armorID) : base(armorID)
		{
		}
		
		/// <summary>
		/// Gets the armor bonus against the specified <see cref="ItemEntity"/>.
		/// </summary>
		public int GetArmorBonus(ItemEntity item)
		{
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;

			var armorBonus = 0;

			if ((flags & WeaponFlags.Projectile) != 0)
			{
				armorBonus += ProjectileProtection;
			}
			else
			{
				var armorProtections = new List<int>();

				if ((flags & WeaponFlags.Piercing) != 0)
					armorProtections.Add(PiercingProtection);

				if ((flags & WeaponFlags.Slashing) != 0)
					armorProtections.Add(SlashingProtection);

				if ((flags & WeaponFlags.Bashing) != 0)
					armorProtections.Add(BashingProtection);

				if (armorProtections.Any())
					armorBonus += armorProtections.Min();
			}

			return armorBonus + BaseArmorBonus;
		}

		protected ItemQuality _armorQuality = ItemQuality.Common;

		[CommandProperty(AccessLevel.GameMaster)]
		public override ItemQuality Quality
		{
			get => _armorQuality;
			set
			{
				_armorQuality = value;

				Delta(ItemDelta.UpdateQuality);
			}
		}

		public override void GetDescriptionSuffix(List<LocalizationEntry> entries)
		{
			var quality = Quality;
			
			if (Identified && quality > ItemQuality.Common)
				entries.Add(new LocalizationEntry(6301000, quality.Color.ToHex(), quality.Localization));
		}

		/// <summary>
		/// Serializes this instance into binary data for persistence.
		/// </summary>
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)2); /* version */
			
			var flags = ArmorSaveFlag.None;
			
			SetSaveFlag(ref flags, ArmorSaveFlag.Quality, (Quality != ItemQuality.Common));
			
			writer.Write((int)flags);

			if (GetSaveFlag(flags, ArmorSaveFlag.Quality))
				writer.Write((byte)_armorQuality.Value);
		}

		/// <summary>
		/// Deserializes this instance from persisted binary data.
		/// </summary>
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt16();

			switch (version)
			{
				case 2:
				{
					var flags = (ArmorSaveFlag)reader.ReadInt32();
					
					if (GetSaveFlag(flags, ArmorSaveFlag.Quality))
						_armorQuality = ItemQuality.GetQuality(reader.ReadByte());
					
					goto case 1;
				}
				case 1:
				{
					break;
				}
			}
		}
		
		[Flags]
		private enum ArmorSaveFlag : int
		{
			None 		= 0x00000000,
			
			Quality		= 0x00000001,
		}
		
		private static void SetSaveFlag(ref ArmorSaveFlag flags, ArmorSaveFlag toSet, bool setIf)
		{
			if (setIf)
				flags |= toSet;
		}

		private static bool GetSaveFlag(ArmorSaveFlag flags, ArmorSaveFlag toGet)
		{
			return ((flags & toGet) != 0);
		}
	}
}
