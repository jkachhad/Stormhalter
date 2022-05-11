using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells
{
	public partial class Venom : Poison
	{
		public Venom(int potency) : base(TimeSpan.Zero, potency, true)
		{
		}

		public override Poison Clone()
		{
			return new Venom(Potency);
		}
	}
}