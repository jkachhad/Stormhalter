using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Kesmai.Server.Game
{
	public partial class VendorEntity : Humanoid
	{
		[WorldForge]
		protected static Regex _sell = new Regex(@"^sell\s([\w\s]*?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		[WorldForge]
		protected static Regex _craft = new Regex(@"^craft\s(\w*?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		protected List<Point2D> _counters;

		[WorldForge]
		public List<Point2D> Counters
		{
			get => _counters;
			set => _counters = value;
		}

		protected VendorEntity()
		{
			Alignment = Alignment.Lawful;

			CanLoot = false;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_counters = new List<Point2D>();
		}
		
		public override void OnSpawn()
		{
			_brain = new IdleAI(this);
			
			base.OnSpawn();
		}
		
		public override void HandleOrder(OrderEventArgs args)
		{
			base.HandleOrder(args);

			if (args.Handled)
				return;
			
			var source = args.Source;

			if (!source.IsAlive || !CanSee(source))
			{
				args.Handled = true;
				
				SayTo(source, 6300235); /* I cannot serve you if I cannot see you. */
			}
		}

		[WorldForge]
		public void AddCounter(Point2D point)
		{
			if (!_counters.Contains(point))
				_counters.Add(point);
		}

		[WorldForge]
		public bool AtCounter(MobileEntity entity, out Point2D counter)
		{
			counter = default(Point2D);

			var vendorLocation = Location;
			var sourceLocation = entity.Location;

			if (vendorLocation != sourceLocation)
			{
				foreach (var direction in Direction.Cardinal)
				{
					var counterLocation = sourceLocation + direction;

					if (_counters.Contains(counterLocation))
					{
						counter = counterLocation;
						break;
					}
				}
			}
			else
			{
				counter = vendorLocation;
			}

			return (counter != default(Point2D));
		}
	}
}
