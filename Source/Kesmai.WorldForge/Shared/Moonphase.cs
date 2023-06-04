using System;
using System.Linq;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge;

public class Moonphase
{
	public static Moonphase New				= new Moonphase(1, "new", 				6300284, TimeSpan.FromHours(7));
	public static Moonphase WaxingCrescent	= new Moonphase(2, "waxing crescent", 	6300285, TimeSpan.FromHours(7));
	public static Moonphase FirstHalf		= new Moonphase(3, "first half", 		6300286, TimeSpan.FromHours(7));
	public static Moonphase WaxingGibbous	= new Moonphase(4, "waxing gibbous", 	6300287, TimeSpan.FromHours(7));
	public static Moonphase Full			= new Moonphase(5, "full", 				6300288, TimeSpan.FromHours(7));
	public static Moonphase WaningGibbous	= new Moonphase(6, "waning gibbous", 	6300289, TimeSpan.FromHours(7));
	public static Moonphase LastHalf		= new Moonphase(7, "last half", 		6300290, TimeSpan.FromHours(7));
	public static Moonphase WaningCrescent	= new Moonphase(8, "waning crescent", 	6300291, TimeSpan.FromHours(7));
		
	public static Moonphase[] All = new Moonphase[]
	{
		New, WaxingCrescent, FirstHalf, WaxingGibbous, Full, WaningGibbous, LastHalf, WaningCrescent
	};
		
	public static Moonphase Get(int index)
	{
		return All.FirstOrDefault(p => p.Index == index);
	}
		
	public int Index { get; set; }
	public string Name { get; set; }
	public int Localization { get; set; }
	public TimeSpan Duration { get; set; }

	public Moonphase(int index, string name, int localization, TimeSpan duration)
	{
		Index = index;
			
		Name = name;
		Localization = localization;
		Duration = duration;
	}
}
	
public class MoonphaseItemsSource : IItemsSource
{
	public ItemCollection GetValues()
	{
		var sizes = new ItemCollection();

		foreach (var phase in Moonphase.All)
			sizes.Add(phase, phase.Name);

		return sizes;
	}
}