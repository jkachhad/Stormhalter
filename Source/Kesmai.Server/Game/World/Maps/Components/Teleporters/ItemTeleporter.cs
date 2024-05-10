using System;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("ItemTeleporter")]
public class ItemTeleporter : HiddenTeleporter
{
	public Type Type { get; set; }
		
	public PathingResult OnFail { get; set; }
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ItemTeleporter"/> class.
	/// </summary>
	public ItemTeleporter(XElement element) : base(element)
	{
		if (element.TryGetElement("type", out var typeElement))
			Type = TypeCache.FindTypeByName(typeElement.Value);
			
		if (element.TryGetElement("fail", out var failElement))
			OnFail = Enum.Parse<PathingResult>(failElement.Value);
		else
			OnFail = PathingResult.Interrupted;
	}

	protected override bool CanTeleport(MobileEntity entity)
	{
		if (!base.CanTeleport(entity))
			return false;

		if (Type != null)
		{
			var rightHand = entity.RightHand;
			var leftHand = entity.LeftHand;

			if (rightHand != null && Type.IsInstanceOfType(rightHand))
				return true;

			if (leftHand != null && Type.IsInstanceOfType(leftHand))
				return true;
		}
			
		return false;
	}
		
	public override void HandleMovementPath(PathingRequestEventArgs args)
	{
		if (CanTeleport(args.Entity))
			args.Result = PathingResult.Teleport;
		else
			args.Result = OnFail;
	}
}