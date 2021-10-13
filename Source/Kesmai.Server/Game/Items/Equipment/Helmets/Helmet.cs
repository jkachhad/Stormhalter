using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public abstract partial class Helmet : Equipment
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000049;

		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 5;

		/// <summary>
		/// Gets or sets a value indication if this instance provides <see cref="NightVisionStatus"/>
		/// </summary>
		public virtual bool ProvidesNightVision => false;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Helmet"/> class.
		/// </summary>
		protected Helmet(int helmetID) : base(helmetID)
		{
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (ProvidesNightVision)
			{
				if (!entity.GetStatus(typeof(NightVisionStatus), out var status))
				{
					status = new NightVisionStatus(entity)
					{
						Inscription = new SpellInscription() { SpellId = 36 }
					};
					status.AddSource(new ItemSource(this));

					entity.AddStatus(status);
				}
				else
				{
					status.AddSource(new ItemSource(this));
				}
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (ProvidesNightVision)
			{
				if (entity.GetStatus(typeof(NightVisionStatus), out var status))
					status.RemoveSourceFor(this);
			}

			return true;
		}
	}
}
