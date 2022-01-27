using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public partial class ScorpionAmulet : Amulet, ITreasure, ICharged
	{
		private int _chargesCurrent;
		private int _chargesMax;

		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public int ChargesCurrent
		{
			get => _chargesCurrent;
			set => _chargesCurrent = value.Clamp(0, _chargesMax);
		}
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public int ChargesMax
		{
			get => _chargesMax;
			set => _chargesMax = value;
		}
		
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 250;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 100;

		public ScorpionAmulet() : this(3)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ScorpionAmulet"/> class.
		/// </summary>
		public ScorpionAmulet(int charges = 3) : base(118)
		{
			_chargesCurrent = charges;
			_chargesMax = charges;
		}
		
		/// <inheritdoc />
		public override ActionType GetAction()
		{
			var container = Container;
			
			if (container is Hands || container is Paperdoll) 
				return ActionType.Use;

			return base.GetAction();
		}
		
		/// <inheritdoc />
		public override bool HandleInteraction(MobileEntity entity, ActionType action)
		{
			if (action != ActionType.Use)
				return base.HandleInteraction(entity, action);
			
			if (_chargesCurrent > 0)
				entity.Target = new InternalTarget(this);

			return true;
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200067)); /* [You are looking at] [a silver chain with a silver and onyx scorpion.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250051)); /* The amulet contains the spell of Neutralize. */
		}

		private class InternalTarget : MobileTarget
		{
			private ScorpionAmulet _amulet;
			
			public InternalTarget(ScorpionAmulet amulet) : base(flags: TargetFlags.Beneficial)
			{
				_amulet = amulet;
			}

			protected override void OnTarget(MobileEntity source, MobileEntity target)
			{
				if (target != null)
				{
					var spell = new NeutralizePoisonSpell()
					{
						Item = _amulet,
						Cost = 0,
					};

					spell.Warm(source);
					spell.CastAt(target);
				}
			}
		}
	}
}