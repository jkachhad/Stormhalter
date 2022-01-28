using System;
using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Wyvern : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Wyvern"/> class.
		/// </summary>
		public Wyvern()
		{
			Name = "wyvern";
			Body = 65;

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
		public override int GetDeathSound() => 34;
		public override int GetNearbySound() => 10;
		public override int GetAttackSound() => 22;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new WyvernScales();
		}
	}
}