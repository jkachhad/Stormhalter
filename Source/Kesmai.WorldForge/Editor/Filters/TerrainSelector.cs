using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Kesmai.WorldForge.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Editor
{
	public abstract class TerrainSelector : ObservableObject
	{
		#region Static
		
		public static TerrainSelector Default = new AllTerrainSelector();

		#endregion

		#region Fields

		private bool _isActive;
		
		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets the filter name.
		/// </summary>
		public virtual string Name { get; set; }
		
		public bool IsActive
		{
			get { return _isActive; }
			set { SetProperty(ref _isActive, value); }
		}
		
		public abstract BitmapImage Icon { get; }
		
		#endregion

		#region Constructors and Cleanup

		#endregion

		#region Methods

		/// <summary>
		/// Gets the query.
		/// </summary>
		public abstract Delegate GetQuery();

		/// <summary>
		/// Returns true if component is valid for this selector.
		/// </summary>
		public virtual bool IsValid(TerrainComponent component)
		{
			return (bool)GetQuery().DynamicInvoke(component);
		}

		public virtual ComponentRender TransformRender(SegmentTile tile, TerrainComponent component, ComponentRender render)
		{
			return render;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		public override string ToString()
		{
			return Name;
		}

		#endregion
	}

	public class AllTerrainSelector : TerrainSelector
	{
		#region Static

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		public override string Name => "Clears any filters.";

		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterNone.png"));

		#endregion

		#region Constructors and Cleanup

		#endregion

		#region Methods

		/// <summary>
		/// Gets the query.
		/// </summary>
		public override Delegate GetQuery()
		{
			return default(Delegate);
		}

		/// <inheritdoc />
		public override bool IsValid(TerrainComponent component)
		{
			return true;
		}

		#endregion
	}

/*	public class DynamicTerrainSelector : TerrainSelector
	{
		#region Static

		#endregion

		#region Fields

		private string _name;
		private string _query;

		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets the filter name.
		/// </summary>
		public override string Name
		{
			get => _name;
			set => _name = value;
		}

		/// <summary>
		/// Gets or sets the query.
		/// </summary>
		public string Query
		{
			get => _query;
			set => _query = value;
		}
		
		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterAll.png"));
		
		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicTerrainSelector"/> class.
		/// </summary>
		public DynamicTerrainSelector(string query)
		{
			_query = query;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the query.
		/// </summary>
		public override Delegate GetQuery()
		{
			var component = Expression.Parameter(typeof(TerrainComponent), "Component");
			var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] { component }, null, _query);

			return e.Compile();
		}

		#endregion
	}*/

	public class ComponentSelector<T> : TerrainSelector where T : TerrainComponent
	{
		#region Static

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets the filter name.
		/// </summary>
		public override string Name => typeof(T).Name;

		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterAll.png"));
		
		#endregion

		#region Constructors and Cleanup

		#endregion

		#region Methods

		/// <summary>
		/// Gets the query.
		/// </summary>
		public override Delegate GetQuery()
		{
			return (Func<TerrainComponent, bool>)(component => component is T);
		}

		#endregion
	}

	public class FloorSelector : ComponentSelector<FloorComponent>
	{
		public override string Name => "Filter for only floor components.";
		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterFloor.png"));
		
		public override ComponentRender TransformRender(SegmentTile tile, TerrainComponent component, ComponentRender render)
		{
			var wallComponent = tile.GetComponents<WallComponent>();

			if (wallComponent.Any(wall => wall.IsIndestructible))
				render.Color = Color.Red;

			if (render.Color.Equals(Color.Black))
				render.Color = Color.DimGray;

			return render;
		}
	}

	public class StaticSelector : ComponentSelector<StaticComponent>
	{
		public override string Name => "Filter for only static components.";
		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterStatic.png"));

		public override ComponentRender TransformRender(SegmentTile tile, TerrainComponent component, ComponentRender render)
		{
			//is there a better way to do this?
			var wallComponent = tile.GetComponents<WallComponent>();
			var floorComponent = tile.GetComponents<FloorComponent>();
			var obstructionComponent = tile.GetComponents<ObstructionComponent>();
			var counterComponent = tile.GetComponents<CounterComponent>();
			var altarComponent = tile.GetComponents<AltarComponent>();
			
			if (!floorComponent.Any() && !wallComponent.Any(wall => wall.IsIndestructible) && !obstructionComponent.Any() && !counterComponent.Any() && !altarComponent.Any())
				render.Color = Color.Red; // statics that are walkable without floors to go along with them.

			if (render.Color.Equals(Color.Black))
				render.Color = Color.DimGray;

			return render;
		}
	}

	public class WallSelector : ComponentSelector<WallComponent>
	{
		public override string Name => "Filter for destructible/indestructible walls.";
		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterWall.png"));

		public override ComponentRender TransformRender(SegmentTile tile, TerrainComponent component, ComponentRender render)
		{
			var floorComponents = tile.GetComponents<FloorComponent>();
			
			if (component is WallComponent wall && wall.IsIndestructible)
			{
				if (floorComponents.Any())
					render.Color = Color.Yellow;
				else
					render.Color = Color.Red;
			}
			else
            {
	            if (!floorComponents.Any())
		            render.Color = Color.Green;
            }
			
			return render;
		}
	}
	
	public class WaterSelector : ComponentSelector<WaterComponent>
	{
		public override string Name => "Filter for water components.";
		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterWater.png"));
	}


	public class StructureSelector : TerrainSelector
	{
		#region Static

		private static List<Type> _structures = new List<Type>()
		{
			typeof(WallComponent),
			typeof(DoorComponent),
			typeof(ObstructionComponent),
			typeof(CounterComponent),
			typeof(AltarComponent),
		};

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		public override string Name => "Filter visibility of only structural components.";
		public override BitmapImage Icon => new BitmapImage(new Uri(@"pack://application:,,,/Kesmai.WorldForge;component/Resources/FilterStructure.png"));

		#endregion

		#region Constructors and Cleanup

		#endregion

		#region Methods

		/// <summary>
		/// Gets the query.
		/// </summary>
		public override Delegate GetQuery()
		{
			return (Func<TerrainComponent, bool>)(component => _structures.Contains(component.GetType()));
		}

		#endregion
	}
}
