using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GriffinJacket : Jacket
	{
		/// <inheritdoc />
		public override uint BasePrice => 30;

        /// <inheritdoc />
		public override int Weight => 1500;
		
		/// <inheritdoc />
		/// <remarks>Robes have a default <see cref="Hindrance"/> value of 1.</remarks>
		public override int Hindrance => 0;
		
		/// <inheritdoc />
		public override int ProtectionFromFire => 10;

        /// <summary>
		/// Initializes a new instance of the <see cref="GriffinJacket"/> class.
		/// </summary>
		public GriffinJacket() : base(260)
		{
		}
        
        /// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200195)); /* [You are looking at] [a vest made from the feathers of a griffin.] */
		}
	}
}
