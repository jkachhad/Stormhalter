using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Lich : CreatureEntity, IUndead
	{
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Piercing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Slashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Bashing;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Projectile;
		public override CreatureImmunity Immunity { get; set; } = CreatureImmunity.Poison;
		public override CreatureWeakness Weakness { get; set; } = CreatureWeakness.BlueGlowing;
		/// <summary>
		/// Initializes a new instance of the <see cref="Lich"/> class.
		/// </summary>
		public Lich()
		{
			Name = "lich";
			Body = 83;

			Alignment = Alignment.Chaotic;
			DeathResistance = 100;
			IceProtection = 100;
			HideDetection = 100;
			StunProtection = 100;
			AddStatus(new BreatheWaterStatus(this));
			AddStatus(new BlindFearProtectionStatus(this));
			AddImmunity(typeof(StunSpell));
			AddImmunity(typeof(PoisonCloudSpell));
		}

		/// <inheritdoc/>
		public override void OnSpawn()
		{
			base.OnSpawn();
			
			if (_brain != null)
				return;
			
			if (RightHand is ProjectileWeapon)
				_brain = new RangedAI(this);
			else
				_brain = new CombatAI(this);
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>
		public override int GetNearbySound() => 131;
		public override int GetAttackSound() => 150;
		public override int GetDeathSound() => 169;

		public override Corpse GetCorpse() => default(Corpse);
	}

	public partial class AncientLich : Lich
	{
		public int Enrage { get; set; }
		
		public bool IsEnraged => (Enrage > 0);

		public override void OnDeath()
		{
			var liches = GetBeholdersInVisibility().SelectMany(g => g.Members).OfType<AncientLich>();
			
			foreach (var lich in liches.Where(l => l.IsAlive))
				lich.DoEnrage(this);
			
			base.OnDeath();
		}

		public void DoEnrage(AncientLich deadLich)
		{
			var nearbyPlayers = GetBeholdersInVisibility().OfType<PlayerGroup>().SelectMany(g => g.Members);

			foreach (var player in nearbyPlayers)
				player.SendLocalizedMessage(Color.Orange, 6300364, Name, deadLich.Name);

			Enrage++;
			
			Body = 83;
			Hue = Color.Red;
		}

		public override int CalculateMeleeDamage(ItemEntity item, MobileEntity defender)
		{
			var damage = (double)base.CalculateMeleeDamage(item, defender);

			if (IsEnraged)
				damage *= 1.5;
			
			return (int)damage;
		}

		public override CreatureSpellEntry OnSelectSpell(List<CreatureSpellEntry> availableSpells)
		{
			var beholder = Group;
			var liches = GetBeheldInVisibility().SelectMany(g => g.Members).OfType<AncientLich>();
			
			foreach (var lich in liches.Where(l => l.IsAlive))
			{
				if (lich.Spells.Any(sp => sp.Next > Server.Now && 
				                          sp.Next - Server.Now > Facet.TimeSpan.FromRounds(2)))
					return default(CreatureSpellEntry);
			}

			return base.OnSelectSpell(availableSpells);
		}
	}
}