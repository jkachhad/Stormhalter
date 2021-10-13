using System.IO;

namespace Kesmai.Server.Game
{
	public partial class YellowBerries : Consumable
	{
		private static ConsumableDamage content = new ConsumableDamage(5);
		
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 5;
		
		/// <inheritdoc />
		public override int LabelNumber => 6000008;

		/// <summary>
		/// Initializes a new instance of the <see cref="YellowBerries"/> class.
		/// </summary>
		public YellowBerries() : base(25)
		{
		}

		/// <inheritdoc />
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_content is null)
				_content = content;
		}
	}
}
