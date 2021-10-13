using System.IO;

namespace Kesmai.Server.Game
{
	public partial class RedBerries : Consumable
	{
		private static ConsumableHeal content = new ConsumableHeal(5);
		
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 5;
		
		/// <inheritdoc />
		public override int LabelNumber => 6000008;

		/// <summary>
		/// Initializes a new instance of the <see cref="RedBerries"/> class.
		/// </summary>
		public RedBerries() : base(26)
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
