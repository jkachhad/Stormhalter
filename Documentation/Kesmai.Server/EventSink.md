# Stormhalter EventSink

This is a human-friendly guide to the `EventSink` system, using the shared `Kesmai.Server` implementation as the reference model for Stormhalter.

## What EventSink is

`EventSink` is the server's hook system.

It gives you two ways to plug behavior into the server:

- Attribute-based hooks for startup and per-tick processing
- Event-based hooks for runtime game, world, account, network, and application activity

In simple terms:

- Use attributes when the server should automatically discover and run your code
- Use events when one part of the server needs to react to something another part just did

## The two parts of EventSink

### 1. Attribute-driven hooks

These are methods marked with attributes such as:

- `[ServerConfigure]`
- `[ServerProcess]`

The server finds these methods automatically and invokes them at the right time.

This is useful when you want:

- Startup registration
- Automatic boot-time wiring
- Ordered per-tick processing

### 2. Runtime events

These are named events on `EventSink`, such as:

- `ServerStarted`
- `FacetChanged`
- `ClientConnect`
- `AccountCreated`

Other systems subscribe to them and react when those events fire.

This is useful when:

- Multiple systems need to know that something happened
- You want to add behavior without tightly coupling two classes together
- A feature should run only when a specific runtime action occurs

## How startup hooks work

`[ServerConfigure]` is for one-time setup.

Typical uses:

- Subscribe to one or more `EventSink` events
- Register background services
- Wire up gameplay systems that need to listen for future activity

Stormhalter already uses this pattern in `RecallRing`:

```csharp
[ServerConfigure]
public new static void Configure()
{
    EventSink.FacetChanged += (player, oldFacet, newFacet) => Reset(player, false);
    EventSink.SegmentChanged += (player, oldSegment, newSegment) => Reset(player);
}
```

Reference: [RecallRing.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Game/Items/Equipment/Rings/RecallRing.cs#L15)

That means recall rings automatically reset when a player changes facet or segment.

## How process hooks work

`[ServerProcess]` is for work that should run repeatedly during the server loop.

Typical uses:

- Time-based systems
- Expiration checks
- Periodic cleanup
- Polling logic that belongs in the main server loop

These methods can be ordered with priorities, so important work can run earlier or later in the tick.

## Priorities

Both `[ServerConfigure]` and `[ServerProcess]` inherit from `CallPriorityAttribute`.

Priority rules:

- Range is `-1000` to `1000`
- Higher numbers run earlier
- `0` is the default

Use priorities only when execution order matters.

Good reasons to set a priority:

- One subsystem depends on another being initialized first
- A process hook must run before save or cleanup logic
- You need deterministic ordering between multiple hooks

## When to use which approach

Use `[ServerConfigure]` when:

- You are wiring up event subscriptions
- The work should happen once at startup
- You want your feature to self-register

Use `[ServerProcess]` when:

- The work should run every server tick or at regular intervals
- The feature is time-based
- The logic belongs inside the main server processing loop

Use a runtime event subscription when:

- You only care about a specific event such as login, level up, world entry, or disconnect
- You want your feature to react rather than poll
- You want cleaner separation between systems

## Example: startup registration

```csharp
[ServerConfigure]
public static void Configure()
{
    EventSink.ServerStarted += () =>
    {
        // Start or initialize your system.
    };

    EventSink.ServerStopped += () =>
    {
        // Clean up your system.
    };
}
```

## Example: process hook

```csharp
[ServerProcess(250)]
public static void Process(long tickCount)
{
    if ((tickCount % 1000) == 0)
    {
        // Do periodic work.
    }
}
```

## Example: persistence hook

```csharp
[ServerConfigure]
public static void Configure()
{
    EventSink.Serialize += SaveState;
    EventSink.Deserialize += LoadState;
    EventSink.Deserialized += AfterLoad;
}

private static void SaveState(bool isClosing)
{
    // Save custom state here.
}

private static void LoadState()
{
    // Load custom state here.
}

private static void AfterLoad()
{
    // Finish setup once all persisted data is available.
}
```

## A useful mental model

Think about `EventSink` like this:

- `[ServerConfigure]` is where you plug your feature into the server
- `[ServerProcess]` is where you keep something running over time
- Runtime events are where your feature reacts to gameplay and infrastructure activity

## All EventSink events in Kesmai.Server

The list below is the shared `Kesmai.Server` `EventSink` event catalog, which Stormhalter is assumed to follow.

## Application events

- `ApplicationStarted`: Fired when the application itself has started.
- `ApplicationStopped`: Fired when the application is shutting down.

## Server lifecycle events

- `ServerStarted`: Fired when server startup begins.
- `ServerStopped`: Fired when the server stops cleanly.
- `ServerCrashed`: Fired when the server encounters an exception.
- `Serialize`: Fired when the server is about to save data.
- `Deserialize`: Fired when the server is about to load persisted data.
- `Deserialized`: Fired after persisted data has been loaded.
- `FacetsLoaded`: Fired after facets have been loaded into memory.
- `FacetsUnloaded`: Fired after facets have been unloaded.
- `SegmentCompiled`: Fired when a segment cache finishes compiling.
- `SegmentInitialized`: Fired when a segment has been initialized.
- `FacetSpawned`: Fired when a facet has completed its spawn/setup work.

## World and gameplay events

- `WorldEnter`: Fired when a client and player enter the world.
- `WorldExit`: Fired when a client and player leave the world.
- `PathingRequest`: Fired when movement/pathing is being checked and can still be allowed or blocked.
- `FacetChanged`: Fired when a player moves from one facet to another.
- `SegmentChanged`: Fired when a player moves from one segment to another.
- `Speech`: Fired when a player says something and listeners may handle it.
- `Order`: Fired when a player issues an order/command that listeners may handle.
- `PlayerLevel`: Fired when a player's level changes.
- `PlayerDeath`: Fired when a player dies.
- `PlayerSkillLevel`: Fired when a player's skill level changes.
- `EnterState`: Fired when a client enters a server state.
- `LeaveState`: Fired when a client leaves a server state.
- `CreatureDeath`: Fired when a creature dies.
- `EntityDeleted`: Fired when a world entity is deleted.

## Network events

- `PeerConnect`: Fired when a network peer requests a connection and can still be accepted or rejected.
- `ClientConnect`: Fired when a client has connected.
- `ClientDisconnect`: Fired when a client disconnects or is disconnected.
- `ClientPacketSend`: Fired when the server is about to send a packet to a client.
- `ClientPacketReceive`: Fired when the server has received a packet from a client.
- `ClientPacketProcessed`: Fired after the server finishes processing a received packet.
- `NetworkCrashed`: Fired when network packet processing throws an exception.

## Account and character events

- `AccountCreated`: Fired when a new account is created.
- `AccountLoaded`: Fired when an account is loaded.
- `AccountInvalidated`: Fired when an account is marked invalid or stale.
- `AccountUnloaded`: Fired when an account is unloaded.
- `AccountDeleted`: Fired when an account is deleted.
- `PlayerCreated`: Fired when a new player entity is created.
- `CharacterCreate`: Fired when a character creation request is being processed.
- `CharacterSelect`: Fired when a client switches from one selected character to another.

## Internal-only events

These exist in the shared implementation but are intended for internal server flow rather than general gameplay extension:

- `AuthenticationUpdated`: Fired when account authentication data changes.
- `LoginRequest`: Fired when a login request is being evaluated.
- `LoginSuccess`: Fired when a login request succeeds.
- `LoginFailure`: Fired when a login request fails.

## Event argument types you will see

Some sinks are simple and carry no arguments. Others provide an event args object.

Common examples:

- `PathingRequestEventArgs`: Lets listeners inspect or change a movement result.
- `SpeechEventArgs`: Gives access to who spoke, what they said, and whether it was handled.
- `OrderEventArgs`: Gives access to who issued an order, the text of the order, and whether it was handled.
- `PeerConnectEventArgs`: Lets listeners accept or reject an incoming peer connection with a reason.
- `CharacterCreateEventArgs`: Provides all requested character-creation details and lets code supply the created character.

## Practical advice

- Prefer event subscriptions over polling when you only need to react to a specific change.
- Use `[ServerConfigure]` to gather all subscriptions in one obvious place.
- Keep `[ServerProcess]` methods lightweight because they run often.
- Use `Deserialized` instead of `ServerStarted` when you need loaded persistence data.
- Be explicit about priorities only when ordering really matters.
- If an event can happen often, keep its handler fast and predictable.

## Common mistakes

- Doing heavy work in `[ServerProcess]` every tick.
- Subscribing in multiple places and forgetting that the same handler now runs more than once.
- Reading persistent data in `ServerStarted` before deserialization is complete.
- Forgetting shutdown cleanup in handlers tied to long-lived services.
- Using an event when a one-time startup hook would be simpler.

## Stormhalter note

Stormhalter currently shows direct use of this system in [RecallRing.cs](/root/stormsmith/Stormhalter/Source/Kesmai.Server/Game/Items/Equipment/Rings/RecallRing.cs), where recall rings subscribe to `FacetChanged` and `SegmentChanged`.

That is a good example of the intended pattern:

- Register once with `[ServerConfigure]`
- Subscribe to the specific runtime sinks you care about
- Keep the runtime handler focused on the actual gameplay consequence
