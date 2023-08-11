using System;
using System.Linq;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge;

public class Lightphase
{
	public static Lightphase Dawn			= new Lightphase(1, "dawn", 		6300278, TimeSpan.FromMinutes(5));
	public static Lightphase Sunrise		= new Lightphase(2, "sunrise", 		6300279, TimeSpan.FromMinutes(15));
	public static Lightphase Daytime		= new Lightphase(3, "daytime", 		6300280, TimeSpan.FromMinutes(40));
	public static Lightphase Sunset			= new Lightphase(4, "sundown", 		6300281, TimeSpan.FromMinutes(15));
	public static Lightphase Midnight		= new Lightphase(5, "midnight", 	6300282, TimeSpan.FromMinutes(5));
	public static Lightphase Nighttime		= new Lightphase(6, "nighttime", 	6300283, TimeSpan.FromMinutes(40));

	public static Lightphase[] All = new Lightphase[]
	{
		Dawn, Sunrise, Daytime, Sunset, Midnight, Nighttime,
	};

	public static Lightphase Get(int index)
	{
		return All.FirstOrDefault(p => p.Index == index);
	}
		
	public int Index { get; set; }
	public string Name { get; set; }
	public int Localization { get; set; }
	public TimeSpan Duration { get; set; }
		
	public Lightphase(int index, string name, int localization, TimeSpan duration)
	{
		Index = index;
			
		Name = name;
		Localization = localization;
		Duration = duration;
	}
}
	
public class LightphaseItemsSource : IItemsSource
{
	public ItemCollection GetValues()
	{
		var sizes = new ItemCollection();

		foreach (var phase in Lightphase.All)
			sizes.Add(phase, phase.Name);

		return sizes;
	}
}