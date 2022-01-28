using System.IO;

namespace Kesmai.Server.Game
{
	public abstract partial class Rat : AnimalEntity
	{
		protected Rat()
		{
			Name = "rat";
			Alignment = Alignment.Chaotic;
		}
		
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_brain != null)
				return;

			_brain = new CombatAI(this);
		}
		
		public override bool CanHide()
		{
			return true;
		}
		
		public override int GetNearbySound() => 142;
		public override int GetAttackSound() => 161;
		public override int GetDeathSound() => 180;
	}

	public partial class BlackRat : Rat
	{
		public BlackRat() : base()
		{
			Body = 72;
		}
	}
	
	public partial class WhiteRat : Rat
	{
		public WhiteRat() : base()
		{
			Body = 207;
		}
	}
}