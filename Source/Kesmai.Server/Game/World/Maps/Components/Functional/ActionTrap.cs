using System;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("ActionTrap")]
public class ActionTrap : TrapComponent
{
	private string _action;
	private Action<MobileEntity> Action;
	
	public ActionTrap(XElement element) : base(element)
	{
		if (element.TryGetElement("action", out var actionElement))
			_action = (string)actionElement;
	}

	public override void Initialize()
	{
		base.Initialize();

		var parent = Parent;

		if (parent is null || String.IsNullOrEmpty(_action))
			return;
		
		var internalCache = parent.Segment.Cache.Internal;
		var actionMethod = internalCache.GetMethod(_action);

		if (actionMethod != null)
			Action = actionMethod.CreateDelegate<Action<MobileEntity>>(null);
	}

	protected override void OnSpring(MobileEntity entity)
	{
		if (Action != null)
			Action(entity);
	}
}