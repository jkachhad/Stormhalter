using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Apatosaur : AnimalEntity
	{
		public Apatosaur()
		{
			Name = "apatosaur";
			Body = 74;

			Alignment = Alignment.Chaotic;
			CanSwim = true;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			if (_brain != null)
				return;

			_brain = new CombatAI(this);
		}
		
		//TODO: right sounds with files
		public override int GetNearbySound() => 197;
		public override int GetAttackSound() => 29;
		public override int GetDeathSound() => 41;

		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}