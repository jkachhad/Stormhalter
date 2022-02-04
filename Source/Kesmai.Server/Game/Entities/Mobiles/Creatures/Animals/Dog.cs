using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Dog : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Dog"/> class.
		/// </summary>
		public Dog()
		{
			Name = "dog";
			Body = 21;

			Alignment = Alignment.Chaotic;
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>
		public override int GetDeathSound() => 37;
		public override int GetNearbySound() => 13;
		public override int GetAttackSound() => 25;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new DogJacket();
		}
	}
}