using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Accounting;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game;

public partial class PlayerEntity
{
[GameCommand("belt", AccessLevel.Player, true)]
private static bool BeltCommand(GameCommandEventArgs args)
{
var client = args.Client;

if (client.State != State.World)
return false;

var character = client.Character;

if (character == null || !character.IsAlive)
return false;

// Parse the hand argument
if (args.Length < 1)
{
return false;
}

var handArg = args.Arguments[0]?.ToLowerInvariant() ?? "";
ItemEntity itemToBelt = null;

// Simple string comparison for left/right
if (handArg == "left" || handArg == "l")
{
itemToBelt = character.LeftHand;
}
else if (handArg == "right" || handArg == "r")
{
itemToBelt = character.RightHand;
}
else
{
// Invalid argument
return false;
}

if (itemToBelt == null)
{
// No item in that hand
return false;
}

var belt = character.Belt;

if (belt == null)
{
// No belt
return false;
}

// Find a slot in the belt
var slot = belt.CheckHold(itemToBelt);

if (!slot.HasValue)
{
// Belt full
return false;
}

// Move item to belt
itemToBelt.DropToContainer(belt, slot.Value);

character.QueueRoundTimer();

return true;
}

[GameCommand("throw", AccessLevel.Player, true)]
private static bool ThrowCommand(GameCommandEventArgs args)
{
var client = args.Client;

if (client.State != State.World)
return false;

var character = client.Character;

if (character == null || !character.IsAlive)
return false;

// Parse the hand argument
if (args.Length < 1)
{
return false;
}

var handArg = args.Arguments[0]?.ToLowerInvariant() ?? "";
ItemEntity itemToThrow = null;

if (handArg == "left" || handArg == "l")
{
itemToThrow = character.LeftHand;
}
else if (handArg == "right" || handArg == "r")
{
itemToThrow = character.RightHand;
}
else
{
return false;
}

if (itemToThrow == null)
{
return false;
}

// Set up targeting for throw
character.Target = new ThrowItemTarget(itemToThrow);

return true;
}
}
