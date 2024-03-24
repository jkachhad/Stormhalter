using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract partial class Food : Consumable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Food"/> class.
	/// </summary>
	public Food(int foodId) : base(foodId)
	{
	}
}