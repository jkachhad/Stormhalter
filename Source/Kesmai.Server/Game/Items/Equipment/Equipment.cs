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
		public virtual int ProtectionFromStun => 0;
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProtectionFromFire => 0;
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProtectionFromIce => 0;
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProtectionFromConcussion => 0;

		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool BlockSpellcast => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="Equipment"/> class.
		/// </summary>
		protected Equipment(int equipmentId) : base(equipmentId)
		{
		}
	}
}
