using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Gold : Currency
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000028;

		/// <summary>
		/// Gets or sets the weight.
		/// </summary>
		public override int Weight => 1;

		/// <summary>
		/// Gets or sets the price.
		/// </summary>
		public override uint BasePrice => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="Gold" /> class.
		/// </summary>
		public Gold() : this(1U)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Gold" /> class.
		/// </summary>
		public Gold(int amount) : this((uint)amount)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Gold"/> class.
		/// </summary>
		public Gold(uint amount = 1) : base(73)
		{
			Amount = amount;
		}

		public override bool RespondsTo(string noun)
		{
			if (base.RespondsTo(noun))
				return true;

			return (noun.Matches("coins", true) || noun.Matches("gold", true));
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200001)); /* [You are looking at] [a gold coin.]*/
		}
	}
}
