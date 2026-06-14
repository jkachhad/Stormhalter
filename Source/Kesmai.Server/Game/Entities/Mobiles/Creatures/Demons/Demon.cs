using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Game.Demons;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Demon : CreatureEntity, IDemon
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Demon"/> class.
	/// </summary>
	public Demon()
	{
		Name = "Demon";
		Body = 63;

		Alignment = Alignment.Lawful;

		Health = MaxHealth = 5;
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		_brain = new CombatAI(this);
	}

	public override int GetNearbySound() => 117;
	public override int GetAttackSound() => 124;
	public override int GetDeathSound() => 7;

	public override Corpse GetCorpse() => default(Corpse);
}