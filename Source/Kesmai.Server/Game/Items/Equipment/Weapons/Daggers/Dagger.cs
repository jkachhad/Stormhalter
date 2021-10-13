using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Dagger : MeleeWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000032;

		/// <inheritdoc />
		public override int Weight => 100;
		
		/// <inheritdoc />
		public override int Category => 14;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <inheritdoc />
		public override Skill Skill => Skill.Dagger;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Piercing | WeaponFlags.Throwable | WeaponFlags.QuickThrow;
		
		public override int ItemId
		{
			get
			{
				if (_poison != null)
					return PoisonedItemId;
					
				return base.ItemId;
			}
		}

		/// <summary>
		/// Gets the item id for this weapon when poisoned.
		/// </summary>
		protected abstract int PoisonedItemId { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Dagger"/> class.
		/// </summary>
		protected Dagger(int daggerID) : base(daggerID)
		{
		}
	}
}
