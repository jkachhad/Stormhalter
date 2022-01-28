using System;
using System.IO;

using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Phantom : CreatureEntity, IPhantasm
	{
		private Timer _dispelTimer;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Phantom"/> class.
		/// </summary>
		public Phantom()
		{
			Name = "phantom";
			Body = 5;

			Alignment = Alignment.Lawful;

			RandomHealth(100, 200);
		}

		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			if (_dispelTimer is null)
				_dispelTimer = Timer.DelayCall(TimeSpan.FromMinutes(10.0), Kill); // TODO: Scale to facet time?

			base.OnLoad();
		}

		public override int GetNearbySound() => 129;
		public override int GetAttackSound() => 148;
		public override int GetDeathSound() => 167;
		
		public override Corpse GetCorpse() => default(Corpse);
	}
}
