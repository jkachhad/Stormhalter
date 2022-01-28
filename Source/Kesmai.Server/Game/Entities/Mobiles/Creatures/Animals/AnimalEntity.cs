using System.Collections.Generic;
using System.IO;

namespace Kesmai.Server.Game
{
	public abstract partial class AnimalEntity : CreatureEntity
	{
		protected AnimalEntity() : base()
		{
			CanLoot = false;
		}

		/// <inheritdoc />
		protected override Dictionary<InventoryGroup, Container> OnCreateInventory()
		{
			/* The inventory keeps track of all containers. */
			return new Dictionary<InventoryGroup, Container>()
			{
				/* Create a unrestricted backpack to allow for any content to be stored.
				 * LootPacks will generate items that may not be typically allowed in a
				 * strict backpack. */
				{ InventoryGroup.Backpack, new Backpack(this, false) },
			};
		}
	}
}