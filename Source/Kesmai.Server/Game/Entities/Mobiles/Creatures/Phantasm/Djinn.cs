using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Djinn : CreatureEntity, IPhantasm
	{
#if (Alpha)
		[GameCommand("Summon", AccessLevel.Player, "Summons a target du- I mean djinn.", "")]
		private static bool Summon(GameCommandEventArgs args)
		{
			var client = args.Client;

			if (client.State != State.World)
				return false;

			var character = client.Character;

			var group = CreatureEntity.SpawnGroup<Djinn>(1);

			group.MoveToWorld(character.Facet, character.Segment, character.Location);

			foreach (var creature in group)
				creature.Director = character;

			/* The client locks actions after any command. Since this doesn't
			 * consume the round, we free the client. */
			character.ClearRound();

			return true;
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="Djinn"/> class.
		/// </summary>
		public Djinn()
		{
			Name = "djinn";
			Body = 5;

			Alignment = Alignment.Lawful;

			Health = MaxHealth = 5;
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			
			_brain = new CombatAI(this);
		}
		
		public override int GetNearbySound() => 129;
		public override int GetAttackSound() => 148;
		public override int GetDeathSound() => 167;
		
		public override Corpse GetCorpse() => default(Corpse);
	}
}
