using System;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("ItemActionComponent")]
public class ItemActionComponent : TerrainComponent, IHandleItems
{
	private readonly string _tag;

	private readonly string _itemAddedAction;
	private readonly string _itemRemovedAction;
	
	private readonly bool _ignoreTeleports;

	private Action<SegmentTile, ItemEntity, string> _itemAdded;
	private Action<SegmentTile, ItemEntity, string> _itemRemoved;
	
	public ItemActionComponent(XElement element) : base(element)
	{
		if (element.TryGetElement("tag", out var tagElement))
			_tag = (string)tagElement;

		if (element.TryGetElement("itemAdded", out var itemAddedElement))
			_itemAddedAction = (string)itemAddedElement;
		
		if (element.TryGetElement("itemRemoved", out var itemRemovedElement))
			_itemRemovedAction = (string)itemRemovedElement;
		
		if (element.TryGetElement("ignoreTeleports", out var ignoreTeleportsElement))
			_ignoreTeleports = (bool)ignoreTeleportsElement;
	}
	
	public override void Initialize(SegmentTile parent)
	{
		base.Initialize(parent);

		if (parent is null)
			return;

		if (!String.IsNullOrEmpty(_itemAddedAction))
		{
			var internalCache = parent.Segment.Cache.Internal;
			var actionMethod = internalCache.GetMethod(_itemAddedAction);

			if (actionMethod != null)
				_itemAdded = actionMethod.CreateDelegate<Action<SegmentTile, ItemEntity, string>>(null);
		}
		
		if (!String.IsNullOrEmpty(_itemRemovedAction))
		{
			var internalCache = parent.Segment.Cache.Internal;
			var actionMethod = internalCache.GetMethod(_itemRemovedAction);

			if (actionMethod != null)
				_itemRemoved = actionMethod.CreateDelegate<Action<SegmentTile, ItemEntity, string>>(null);
		}
	}

	public void OnItemAdded(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
		if (parent is null || item is null || item.Deleted)
			return;

		if (_ignoreTeleports && isTeleport)
			return;

		if (_itemAdded != null)
			_itemAdded(parent, item, _tag);
	}

	public void OnItemRemoved(SegmentTile parent, ItemEntity item, bool isTeleport)
	{
		if (parent is null || item is null || item.Deleted)
			return;
		
		if (_ignoreTeleports && isTeleport)
			return;
		
		if (_itemRemoved != null)
			_itemRemoved(parent, item, _tag);
	}
}