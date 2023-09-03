using System.ComponentModel;
using System.Xml.Linq;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge.Models;

public abstract class SpellTrap : TrapComponent
{
	private int _skill;
	
	[Browsable(true)]
	public int Skill
	{
		get => _skill;
		set => _skill = value;
	}

	protected SpellTrap(XElement element) : base(element)
	{
		var skillElement = element.Element("skill");

		if (skillElement != null)
			_skill = (int)skillElement;
	}
	
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (_skill > 0)
			element.Add(new XElement("skill", _skill));
		
		return element;
	}
}

public class WhirlwindTrap : SpellTrap
{
	[Browsable(true)]
	[ItemsSource(typeof(DirectionsItemsSource))]
	public Direction Direction { get; set; }
	
	public WhirlwindTrap(XElement element) : base(element)
	{
		if (element.TryGetElement("direction", out var directionElement))
			Direction = Direction.GetDirection((int)directionElement);
	}
	
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (Direction != null)
			element.Add(new XElement("direction", Direction.Index));

		return element;
	}
	
	public override TerrainComponent Clone()
	{
		return new WhirlwindTrap(GetXElement());
	}
}

public class PoisonCloudTrap : SpellTrap
{
	[Browsable(true)]
	[ItemsSource(typeof(DirectionsItemsSource))]
	public Direction Direction { get; set; }
	
	public PoisonCloudTrap(XElement element) : base(element)
	{
		if (element.TryGetElement("direction", out var directionElement))
			Direction = Direction.GetDirection((int)directionElement);
	}
	
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (Direction != null)
			element.Add(new XElement("direction", Direction.Index));

		return element;
	}
	
	public override TerrainComponent Clone()
	{
		return new PoisonCloudTrap(GetXElement());
	}
}

public class BonfireTrap : SpellTrap
{
	public BonfireTrap(XElement element) : base(element)
	{
	}
	
	public override TerrainComponent Clone()
	{
		return new BonfireTrap(GetXElement());
	}
}

public class FirestormTrap : SpellTrap
{
	public FirestormTrap(XElement element) : base(element)
	{
	}
	
	public override TerrainComponent Clone()
	{
		return new FirestormTrap(GetXElement());
	}
}

public class ConcussionTrap : SpellTrap
{
	public ConcussionTrap(XElement element) : base(element)
	{
	}
	
	public override TerrainComponent Clone()
	{
		return new ConcussionTrap(GetXElement());
	}
}

public class LightningBoltTrap : SpellTrap
{
	public LightningBoltTrap(XElement element) : base(element)
	{
	}
	
	public override TerrainComponent Clone()
	{
		return new LightningBoltTrap(GetXElement());
	}
}

public class BlindTrap : SpellTrap
{
	public BlindTrap(XElement element) : base(element)
	{
	}
	
	public override TerrainComponent Clone()
	{
		return new BlindTrap(GetXElement());
	}
}

public class FearTrap : SpellTrap
{
	public FearTrap(XElement element) : base(element)
	{
	}
	
	public override TerrainComponent Clone()
	{
		return new FearTrap(GetXElement());
	}
}

public class StunTrap : SpellTrap
{
	public StunTrap(XElement element) : base(element)
	{
	}
	
	public override TerrainComponent Clone()
	{
		return new StunTrap(GetXElement());
	}
}