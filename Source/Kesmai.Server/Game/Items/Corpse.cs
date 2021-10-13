using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Humanizer;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Corpse : ItemEntity
	{
		/// <inheritdoc />
		public override int LabelNumber => 6005000; /* corpse */

		/// <inheritdoc />
		public override int Category => 16;

		/// <inheritdoc />
		public override bool CanDisintegrate => false;

		/// <summary>
		/// Get a value indicating if this corpse can be stripped by fire.
		/// </summary>
		public bool CanBurn { get; set; } = true;

		/* Corpses are not serialized. Player corpses should not exist when the world loads. All corpses
		 * should be only available after deserialization. Should a player die, and is not able to recovery
		 * their corpse prior to shutdown, those items would be lost anyways. */
		/// <inheritdoc />
		public override bool IsSerialized => false;

		public bool Stripped
		{
			get
			{
				var owner = Owner;

				if (owner != null && owner.GetItems().Any())
					return false;
				
				return true;
			}
		}

		public Corpse(MobileEntity owner, int body) : base(body)
		{
			Owner = owner;
		}

		protected override void OnDelete()
		{
			base.OnDelete();

			var owner = Owner;

			if (owner is CreatureEntity creature && !creature.Deleted)
				creature.Delete();
		}

		public override void OnLocationChange(Point2D oldLocation, Point2D newLocation)
		{
			base.OnLocationChange(oldLocation, newLocation);

			var owner = Owner;
			
			/* Attempt to move the owner to the corpse as it moves. */
			/* Do not move any creatures, as they are likely dead. */
			if (owner is null || owner is CreatureEntity || owner.IsAlive)
				return;

			/* Only teleport the entity if their location is changing. If they are already here,
			 * no need to teleport them to us. */
			if (owner.Location != newLocation)
				owner.Teleport(newLocation);
		}

		public override void GetDescription(List<LocalizationEntry> entries)
		{
			var description = String.Empty;

			var owner = Owner;
			
			if (owner is CreatureEntity creature)
				description = $"the body of {creature.Name.WithArticle()}.";
			else if (owner is PlayerEntity player)
				description = $"the body of a {player.Profession.Info.Name}.";

			if (!String.IsNullOrEmpty(description))
				entries.Add(new LocalizationEntry(6200000, description)); /* [You are looking at] [the body of ..] */
		}

		/// <inheritdoc />
		public override ActionType GetAction()
		{
			var parent = Parent;

			/* If there exists an entity that is holding us, return the default action. */
			if (parent != default(MobileEntity))
				return base.GetAction();
			
			return ActionType.Search;
		}

		/// <inheritdoc />
		public override bool HandleInteraction(MobileEntity entity, ActionType action)
		{
			if (action != ActionType.Search)
				return base.HandleInteraction(entity, action);

			if (!entity.CanPerformAction)
				return false;
			
			Strip();
			
			entity.QueueRoundTimer();
			return true;
		}

		public override bool ThrowAt(MobileEntity source, MobileEntity entity)
		{
			source.SendLocalizedMessage(6300357); /* Even given the strength of the ghods, you could not throw this. */
			return true;
		}

		public override bool ThrowAt(MobileEntity source, Point2D location)
		{
			source.SendLocalizedMessage(6300357); /* Even given the strength of the ghods, you could not throw this. */
			return true;
		}

		public void Burn()
		{
			if (Deleted || !CanBurn)
				return;
			
			var owner = Owner;
			var segment = Segment;
			
			Strip();
			Delete();

			if (owner is PlayerEntity && !owner.IsAlive)
			{
				var resurrectionLocation = segment.GetResurrectionLocation();

				if (Location != resurrectionLocation)
					owner.Teleport(resurrectionLocation);
				
				owner.Resurrect(ResurrectType.Ghods);
			}
		}

		public void Strip()
		{
			var owner = Owner;
			var items = owner.GetItems().ToList();

			foreach (var item in items)
			{
				item.OnStrip(this);
				item.Move(Location, true, Segment);
			}

			Move(Location, true);
		}

		public ItemEntity Tan()
		{
			var owner = Owner;
			
			if (owner is CreatureEntity creature)
				return creature.OnCorpseTanned();

			return default(ItemEntity);
		}
	}
}