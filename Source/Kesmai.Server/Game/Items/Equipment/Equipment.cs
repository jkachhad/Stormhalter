using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Equipment : ItemEntity
	{
		/// <summary>
		/// Gets the hindrance penalty for this <see cref="Equipment"/>.
		/// </summary>
		public virtual int Hindrance => 0;

		public virtual int ProtectionFromStun => 0;
		public virtual int ProtectionFromFire => 0;
		public virtual int ProtectionFromIce => 0;
		public virtual int ProtectionFromConcussion => 0;

		public virtual bool BlockSpellcast => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="Equipment"/> class.
		/// </summary>
		protected Equipment(int equipmentId) : base(equipmentId)
		{
		}
	}
}
