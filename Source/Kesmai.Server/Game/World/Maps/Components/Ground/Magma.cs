using System.Xml;
using System.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;
using System.Threading;

namespace Kesmai.Server.Game;

[WorldForgeComponent("MagmaComponent")]
public class Magma : Floor, IHandlePathing
{
    private int _baseDamage;
    private Timer _internalTimer;
    private static Dictionary<MobileEntity, DateTime> _entities = new Dictionary<MobileEntity, DateTime>();    
    private static List<KeyValuePair<MobileEntity, DateTime>> _entitiesToAdd { get; } = new List<KeyValuePair<MobileEntity, DateTime>>();
    private static List<MobileEntity> _entitiesToRemove { get; } = new List<MobileEntity>();

    /// <inheritdoc />
    public int PathingPriority { get; } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Magma"/> class.
    /// </summary>
    private Magma(Color color, int MagmaId, int movementCost, int baseDamage) : base(color, MagmaId, movementCost)
    {
        _baseDamage = baseDamage;
    }

    /// <inheritdoc />
    public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
    {
        return true;
    }

    /// <inheritdoc />
    public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
    {
        return true;
    }

    public override void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
    {    
        if (!entity.IsAlive)
            return;

        if (!_entities.ContainsKey(entity))
            _entitiesToAdd.Add(new KeyValuePair<MobileEntity,DateTime>(entity, DateTime.Now));
        
        var region = parent.Region;
        
        if (!region.IsInactive && !_internalTimer.Running)
            StartTimer(entity.Facet);    

    }

    /// <summary>
    /// Called when a mobile entity steps off this component.
    /// </summary>
    public override void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
    {
        if (_entities.ContainsKey(entity))
            _entitiesToRemove.Add(entity);
        
        if (_entities.Count == 0)
            StopTimer();
    }

    private void StartTimer(Facet facet)
    {
        if (_internalTimer != null)
            _internalTimer.Stop();

        _internalTimer = Timer.DelayCall(TimeSpan.Zero, facet.TimeSpan.FromRounds(1), OnTick);
    }
    private void StopTimer()
    {
        if (_internalTimer != null)
        {
            _internalTimer.Stop();
            _internalTimer = null;
        }
    }

    /// <summary>
    /// Handles pathing requests over this terrain.
    /// </summary>
    public void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
    {
        args.Result = PathingResult.Allowed;

        if (args.Entity is PlayerEntity player)
        {
            var balance = player.Stats[EntityStat.DexterityAdds].Value + 5;

            if (Utility.Random(1, 20) > balance)
            {
                args.Entity.SendMessage(Color.Yellow,"You lose your balance and fall in the Magma"); /* You lose your balance when you step onto the lava */
                args.Entity.Health -= (args.Entity.MaxHealth / 10);
                args.Result = PathingResult.Interrupted;
            }
        }
    }

    private void OnTick()
    {
        var tempKeysToRemove = new List<MobileEntity>(_entitiesToRemove);
        var tempKeysToAdd = new List<KeyValuePair<MobileEntity,DateTime>>(_entitiesToAdd);

        var damage = _baseDamage;
        var damageString = "Your boots and feet begin to burn as you sink slowly into the lava.";

        _entitiesToRemove.Clear();
        _entitiesToAdd.Clear();

        var spell = new FireballSpell();

        foreach (var entity in _entities)
        {
            if (entity.Key is null)
                continue;
            
            if (!entity.Key.IsAlive)
            {
                // Not sure how to implement corpse burning here... other then strip and teleport?
                tempKeysToRemove.Add(entity.Key);
                continue;
            }    

            TimeSpan elapsedTime = DateTime.Now - entity.Value; 

            if (elapsedTime.TotalSeconds > 5 && elapsedTime.TotalSeconds <= 10) 
            {
                damage *= 2;
                damageString = "As you sink deeper into the lava, the heat becomes unbearable.";
            }

            if (elapsedTime.TotalSeconds > 10 && elapsedTime.TotalSeconds <= 20) 
            {
                damage *= 5;
                damageString = "The lava is now up to your waist and you can feel your skin melting.";
            }

            if (entity.Key != null && entity.Key.IsAlive)
            {
                // assuming applyspell damage accounts fore resistances and immunities
                entity.Key.ApplySpellDamage(null,spell,damage,true);
                if (entity.Key is PlayerEntity)
                    entity.Key.SendMessage(Color.Yellow, damageString); /* You are standing in the Magma and take damage. */
            }

            if (elapsedTime.TotalSeconds > 20)
            {
                entity.Key.Kill();
                tempKeysToRemove.Add(entity.Key);
            }


        }

        foreach (var entity in tempKeysToRemove)
        {
            _entities.Remove(entity);
        }

        foreach (var entity in tempKeysToAdd)
        {
            _entities.Add(entity.Key, entity.Value);
        }
    }
}