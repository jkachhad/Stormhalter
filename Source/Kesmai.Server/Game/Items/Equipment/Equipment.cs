using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Equipment : ItemEntity
	{
		/// <summary>
		/// Gets the hindrance penalty for this <see cref="Equipment"/>.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int Hindrance => 0;

		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProtectionFromDaze => 0;
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProtectionFromFire => 0;
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProtectionFromIce => 0;
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProtectionFromConcussion => 0;
		
		/// <summary>
		/// Gets the mana regeneration provided by this <see cref="Equipment"/>
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ManaRegeneration => 0;

		/// <summary>
		/// Gets a value indicating if this instance restricts spell casting for certain professions.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool RestrictSpellcast => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="Equipment"/> class.
		/// </summary>
		protected Equipment(int equipmentId) : base(equipmentId)
		{
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;
			
			if (CanUse(entity))
			{
				if (ProtectionFromFire > 0)
					entity.Stats[EntityStat.FireProtection].Add(+ProtectionFromFire, ModifierType.Constant);
				
				if (ProtectionFromIce > 0)
					entity.Stats[EntityStat.IceProtection].Add(+ProtectionFromIce, ModifierType.Constant);
				
				if (ProtectionFromDaze > 0)
					entity.Stats[EntityStat.DazeProtection].Add(+ProtectionFromDaze, ModifierType.Constant);

				if (ManaRegeneration > 0)
					entity.Stats[EntityStat.ManaRegenerationRate].Add(+ManaRegeneration, ModifierType.Constant);
			}

			return true;
		}
		
		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;
			
			if (CanUse(entity))
			{
				if (ProtectionFromFire > 0)
					entity.Stats[EntityStat.FireProtection].Remove(+ProtectionFromFire, ModifierType.Constant);
				
				if (ProtectionFromIce > 0)
					entity.Stats[EntityStat.IceProtection].Remove(+ProtectionFromIce, ModifierType.Constant);
				
				if (ProtectionFromDaze > 0)
					entity.Stats[EntityStat.DazeProtection].Remove(+ProtectionFromDaze, ModifierType.Constant);

				if (ManaRegeneration > 0)
					entity.Stats[EntityStat.ManaRegenerationRate].Remove(+ManaRegeneration, ModifierType.Constant);
			}

			return true;
		}
	}
}
