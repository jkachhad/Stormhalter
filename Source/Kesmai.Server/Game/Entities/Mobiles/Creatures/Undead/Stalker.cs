using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Stalker : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Stalker"/> class.
		/// </summary>
		public Stalker()
		{
			Name = "stalker";
			Body = 57;

			Alignment = Alignment.Chaotic;
		}

		/// <inheritdoc/>
		public override void OnSpawn()
		{
			base.OnSpawn();
			
			if (_brain != null)
				return;
			
			_brain = new CombatAI(this);
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>

		public override int GetNearbySound() => 125;
		public override int GetAttackSound() => 121;
		public override int GetDeathSound() => 182;

		public override Corpse GetCorpse() => default(Corpse);
	}
}