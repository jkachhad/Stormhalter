using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Peacekeeper : Humanoid
{
	public Peacekeeper()
	{
		Alignment = Alignment.Lawful;
			
		CanLoot = false;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
	}

	/// <inheritdoc/>
	public override AIBrain GetBrain() => AIBrain.FromWeapon(this, RightHand);
}
