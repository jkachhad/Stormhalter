using System;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial interface IEmpowered
	{
		Type ContainedSpell { get; }
		
		/// <summary>
		/// Attempts to retrieve a spell contained within this instance.
		/// </summary>
		bool ContainsSpell(out Spell spell);
	}
}