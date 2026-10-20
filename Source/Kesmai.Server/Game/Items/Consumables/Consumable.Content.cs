using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public interface IConsumableContent
{
	/// <summary>
	/// Gets the description for the content.
	/// </summary>
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries);

	/// <summary>
	/// Adds the effect of consuming this content to the tooltip of the item that holds it, such as "Use: Restores 20
	/// health." with <see cref="Tooltip.AddEffect"/>. The item calls this only while it holds the content.
	/// </summary>
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
	}

	/// <summary>
	/// Called when consumed by an entity.
	/// </summary>
	public void OnConsume(MobileEntity entity, Consumable item);

	/// <summary>
	/// Called when this instance is thrown at the specified entity.
	/// </summary>
	/// <remarks>The default behavior is to execute <see cref="ThrowAt(Kesmai.Server.Game.MobileEntity,Kesmai.Server.Game.Point2D)"/></remarks>
	public void ThrowAt(MobileEntity source, Consumable item, MobileEntity target)
	{
		ThrowAt(source, item, target.Location);
	}

	/// <summary>
	/// Called when this instance is thrown to the specified location.
	/// </summary>
	public void ThrowAt(MobileEntity source, Consumable item, Point2D location)
	{
	}

	public void Serialize(SpanWriter writer);
	public void Deserialize(ref SpanReader reader);
}

public class ConsumableHeal : IConsumableContent
{
	private int _amount; 
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ConsumableHeal"/> class.
	/// </summary>
	/// <param name="amount">The amount an entity will be healed.</param>
	public ConsumableHeal(int amount)
	{
		_amount = amount;
	}

	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610020, _amount.ToString()); /* Restores {0} health. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		entity.Health += _amount;
	}

	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
			
		writer.Write(_amount);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				_amount = reader.ReadInt32();
				break;
			}
		}
	}
}

public class ConsumableDamage : IConsumableContent
{
	private int _amount; 
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ConsumableDamage"/> class.
	/// </summary>
	/// <param name="amount">The amount an entity will be hurt.</param>
	public ConsumableDamage(int amount)
	{
		_amount = amount;
	}

	public virtual void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610021, _amount.ToString()); /* Hurts you for {0} damage, though not below 1 health. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		var health = entity.Health;

		health -= _amount;

		if (health < 1)
			health = 1;

		entity.Health = health;
	}
		
	public virtual void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
			
		writer.Write(_amount);
	}

	public virtual void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				_amount = reader.ReadInt32();
				break;
			}
		}
	}
}

public class ConsumablePoison : IConsumableContent
{
	private int _potency;

	public ConsumablePoison(int potency)
	{
		_potency = potency;
	}
		
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6300393)); /* The bottle contains a poison. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610022, _potency.ToString()); /* Poisons you with potency {0}. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		entity.Poison(item.Owner, new Poison(TimeSpan.Zero, _potency));
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
			
		writer.Write(_potency);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				_potency = reader.ReadInt32();
				break;
			}
		}
	}
}

public class ConsumablePoisonAntidote : IConsumableContent
{
	private bool _relative;
	private int _potency; 
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ConsumablePoisonAntidote"/> class.
	/// </summary>
	/// <param name="potency">The amount of potency reduced on poison status.</param>
	/// <param name="relative">if the antidote cures by a relative, or absolute amount.</param>
	public ConsumablePoisonAntidote(int potency, bool relative = true)
	{
		_relative = relative;
		_potency = potency;
	}

	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6300394)); /* The bottle contains a Neutralize Poison potion. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		if (_relative)
			tooltip.AddEffect(TooltipEffectTrigger.Use, 6610023, _potency.ToString()); /* Weakens your poison by {0}. */
		else
			tooltip.AddEffect(TooltipEffectTrigger.Use, 6610024); /* Weakens any poison stronger than potency 5 to potency 5. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity.GetStatus<PoisonStatus>() is PoisonStatus status)
		{
			var poisons = status.Poisons;

			if (_relative)
			{
				for (var i = 0; i < _potency; i++)
					poisons.Where(p => p.Potency > 0).Random().Potency--;
			}
			else
			{
				foreach (var p in poisons.Where(p => p.Potency > 5))
					p.Potency = 5;
			}
				
			if (status.Potency <= 0)
				entity.NeutralizePoison();
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)2);
			
		writer.Write(_relative);
		writer.Write(_potency);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 2:
			{
				_relative = reader.ReadBoolean();
				goto case 1;
			}
			case 1:
			{
				_potency = reader.ReadInt32();
				break;
			}
		}
	}
}

public class ConsumableBlindnessAntidote : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250066)); /* The bottle contains a Cure Blindness potion. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610025); /* Cures blindness. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity.GetStatus(typeof(BlindStatus), out var status))
			entity.RemoveStatus(status);
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableRestoreMana : IConsumableContent
{
	private static Dictionary<int, ConsumableRestoreMana> _collection 
		= new Dictionary<int, ConsumableRestoreMana>();

	public static ConsumableRestoreMana Full = new ConsumableRestoreMana(default(int?));

	public static ConsumableRestoreMana Instantiate(int amount)
	{
		if (!_collection.TryGetValue(amount, out var content))
			_collection.Add(amount, (content = new ConsumableRestoreMana(amount)));

		return content;
	}
		
	private int? _amount;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsumableRestoreMana"/> class.
	/// </summary>
	/// <param name="amount">The amount of mana restored if specified. Otherwise, the maximum amount is restored.</param>
	public ConsumableRestoreMana(int? amount)
	{
		_amount = amount;
	}

	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250060)); /* The bottle contains a potion to restore mana. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		if (_amount.HasValue)
			tooltip.AddEffect(TooltipEffectTrigger.Use, 6610026, _amount.Value.ToString()); /* Restores {0} mana. */
		else
			tooltip.AddEffect(TooltipEffectTrigger.Use, 6610027); /* Fully restores your mana. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (_amount.HasValue)
			entity.Mana += _amount.Value;
		else
			entity.Mana = entity.MaxMana;
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
			
		writer.Write(_amount.HasValue);

		if (_amount.HasValue)
			writer.Write(_amount.Value);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				if (reader.ReadBoolean())
					_amount = reader.ReadInt32();
					
				break;
			}
		}
	}
}

public class ConsumableIncreaseMana : IConsumableContent
{
	private int _amount;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsumableIncreaseMana"/> class.
	/// </summary>
	/// <param name="amount">The amount of max mana increased.</param>
	public ConsumableIncreaseMana(int amount)
	{
		_amount = amount;
	}

	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250078)); /* The liquid increases mana. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610028, _amount.ToString()); /* Removes up to {0} of your maximum mana penalty and fully restores your mana. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is not PlayerEntity player) 
			return;

		var currentMaxManaPenalty = player.Stats[EntityStat.MaxMana].Penalty;

		if (currentMaxManaPenalty is 0) 
			return;
		
		player.Stats[EntityStat.MaxMana].Penalty = Math.Max(currentMaxManaPenalty - _amount, 0);
		
		player.Mana = player.MaxMana;
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);

		writer.Write(_amount);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				_amount = reader.ReadInt32();
				break;
			}
		}
	}
}

public class ConsumableRestoreStamina : IConsumableContent
{
	private static Dictionary<int, ConsumableRestoreStamina> _collection 
		= new Dictionary<int, ConsumableRestoreStamina>();

	public static ConsumableRestoreStamina Full = new ConsumableRestoreStamina(default(int?));

	public static ConsumableRestoreStamina Instantiate(int amount)
	{
		if (!_collection.TryGetValue(amount, out var content))
			_collection.Add(amount, (content = new ConsumableRestoreStamina(amount)));

		return content;
	}
		
	private int? _amount;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsumableRestoreStamina"/> class.
	/// </summary>
	/// <param name="amount">The amount of stamina restored if specified. Otherwise, the maximum amount is restored.</param>
	public ConsumableRestoreStamina(int? amount)
	{
		_amount = amount;
	}

	public virtual void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250067)); /* The bottle contains a stamina potion. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		if (_amount.HasValue)
			tooltip.AddEffect(TooltipEffectTrigger.Use, 6610029, _amount.Value.ToString()); /* Restores {0} stamina. */
		else
			tooltip.AddEffect(TooltipEffectTrigger.Use, 6610030); /* Fully restores your stamina. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (_amount.HasValue)
			entity.Stamina += _amount.Value;
		else
			entity.Stamina = entity.MaxStamina;
	}
		
	public virtual void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
			
		writer.Write(_amount.HasValue);

		if (_amount.HasValue)
			writer.Write(_amount.Value);
	}

	public virtual void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				if (reader.ReadBoolean())
					_amount = reader.ReadInt32();
					
				break;
			}
		}
	}
}

public class ConsumableWater : ConsumableRestoreStamina
{
	public ConsumableWater() : base(10)
	{
	}
		
	public override void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6250132)); /* Inside you see a clear liquid. */

		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250062)); /* The bottle contains water. */
	}
		
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);
			
		writer.Write((byte)1);
	}

	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);
			
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableUrine : ConsumableDamage
{
	private string _owner;

	public ConsumableUrine(string owner) : base(10)
	{
		_owner = owner;
	}
		
	public override void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6250133)); /* Inside you see a yellow liquid. */

		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250064, _owner)); /* The bottle contains {_owner} urine. */
	}
		
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);
			
		writer.Write((byte)1);
		writer.Write(_owner);
	}

	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);
			
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				_owner = reader.ReadString();
				break;
			}
		}
	}
}

public class ConsumableAmbrosia : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250074)); /* The liquid inside restores youth. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610031); /* Makes you younger. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is PlayerEntity player)
		{
			var age = player.Age;

			if (age > Aging.Young)
				age -= Aging.Young * 2;

			if (age < Aging.Young)
				age = Aging.Young;

			player.Age = age;
			player.SendLocalizedMessage(6300346);
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableNaphtha : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250061)); /* The bottle contains an ounce of Naphtha. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610032); /* Burns you for 10 damage. Thrown, it bursts into flames. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		entity.SendLocalizedMessage(6300345);
		entity.ApplyDamage(entity, 10);
	}

	public void ThrowAt(MobileEntity source, Consumable item, Point2D location)
	{
		var segment = source.Segment;
		var tile = segment.FindTile(location);

		if (tile is null || !tile.AllowsSpellPath()) 
			return;
			
		// TODO: Check for hole/sky?
		var spell = new BonfireSpell
		{
			Item = item,
					
			SkillLevel = 6,
			Cost = 0,
		};

		spell.Warm(source);
		spell.CastAt(location);
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableNitro : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6250132)); /* Inside you see a clear liquid. */
			
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250063)); /* The bottle contains Nitro. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610033); /* Burns you for 10 damage. Thrown, it explodes in a concussive blast. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		entity.SendLocalizedMessage(6300345);
		entity.ApplyDamage(entity, 10);
	}

	public void ThrowAt(MobileEntity source, Consumable item, Point2D location)
	{
		var segment = source.Segment;
		var tile = segment.FindTile(location);

		if (tile is null || !tile.AllowsSpellPath()) 
			return;
			
		// TODO: Check for hole/sky?
		var spell = new ConcussionSpell
		{
			Item = item,
			Localized = true,
					
			SkillLevel = 8,
			Cost = 0,
		};

		spell.Warm(source);
		spell.CastAt(location);
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableStrengthSpell : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6250131)); /* Inside you see a red liquid. */
		
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250065)); /* The bottle contains a potion of temporary strength. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610002, 602); /* Casts Strength. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is PlayerEntity player)
		{
			var spell = new InstantStrengthSpell
			{
				Item = item, 
				Cost = 0,
			};
				
			spell.Warm(entity);
			spell.CastAt(entity);
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableStrengthStat : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250072)); /* The liquid permanently increases strength. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610034); /* Permanently raises your Strength by 1, but may blind and daze you. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is PlayerEntity player)
		{
			var strength = player.Stats[EntityStat.BaseStrength];

			if (strength.Base < strength.MaximumValue)
				strength.Base++;

			player.SendLocalizedMessage(6100100); /* You feel a little bit more like Hercules. */
				
			if (Utility.RandomBetween(1, 2) >= 2)
				player.Blind(4);
				
			player.Daze(12);
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableDexterityStat : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250069)); /* The liquid permanently increases dexterity. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610035); /* Permanently raises your Dexterity by 1, but may blind and daze you. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is PlayerEntity player)
		{
			var dexterity = player.Stats[EntityStat.BaseDexterity];

			if (dexterity.Base < dexterity.MaximumValue)
				dexterity.Base++;

			player.SendLocalizedMessage(6100102); /* You feel more agile. */

			if (Utility.RandomBetween(1, 2) >= 2)
				player.Blind(4);

			player.Daze(12);
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableIntelligenceStat : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250068)); /* The liquid permanently increases intelligence. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610036); /* Permanently raises your Intelligence by 1. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is PlayerEntity player)
		{
			var intelligence = player.Stats[EntityStat.BaseIntelligence];

			if (intelligence.Base < intelligence.MaximumValue)
				intelligence.Base++;

			player.SendLocalizedMessage(6100103); /* You feel more ingenious. */
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableWillpowerStat : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250077)); /* The liquid permanently increases willpower. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610037); /* Permanently raises your Willpower by 1. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is PlayerEntity player)
		{
			var willpower = player.Stats[EntityStat.BaseWillpower];

			if (willpower.Base < willpower.MaximumValue)
				willpower.Base++;

			player.SendLocalizedMessage(6100105); /* You feel more resolute. */
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableWisdomStat : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250076)); /* The liquid permanently increases wisdom. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610038); /* Permanently raises your Wisdom by 1. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is PlayerEntity player)
		{
			var wisdom = player.Stats[EntityStat.BaseWisdom];

			if (wisdom.Base < wisdom.MaximumValue)
				wisdom.Base++;

			player.SendLocalizedMessage(6100104); /* You feel more enlightened. */
		}
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableConstitutionStat : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250073)); /* The liquid permanently increases constitution. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610039); /* Permanently raises your Constitution by 2, reduces your maximum health and stamina penalties by 4, and fully restores you. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		if (entity is not PlayerEntity player) 
			return;
		
		var baseConstitution = player.Stats[EntityStat.BaseConstitution];

		/* Compare base value with the maximum value.*/
		if (baseConstitution.Base < baseConstitution.MaximumValue)
			baseConstitution.Base += 2;

		player.SendLocalizedMessage(6100106); /* You feel more hale. */
		
		/* Reduce the penalty applied to maximum health. */
		var currentMaxHealthPenalty = player.Stats[EntityStat.MaxHealth].Penalty;

		if (currentMaxHealthPenalty > 0)
			player.Stats[EntityStat.MaxHealth].Penalty = Math.Max(currentMaxHealthPenalty - 4, 0);
		
		/* Reduce the penalty applied to maximum stamina. */
		var currentMaxStaminaPenalty = player.Stats[EntityStat.MaxStamina].Penalty;

		if (currentMaxStaminaPenalty > 0)
			player.Stats[EntityStat.MaxStamina].Penalty = Math.Max(currentMaxStaminaPenalty - 4, 0);
		
		/* Increase the player to full health, stamina, and mana. */
		player.Health = player.MaxHealth;
		player.Stamina = player.MaxStamina;
		player.Mana = player.MaxMana;
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}

public class ConsumableBalm : IConsumableContent
{
	public void GetDescription(ItemEntity consumable, List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6250130)); /* Inside you see a cloudy white liquid. */
			
		if (consumable.Identified)
			entries.Add(new LocalizationEntry(6250004)); /* The bottle contains balm. */
	}

	/// <inheritdoc />
	public void GetTooltipEffects(Consumable item, Tooltip tooltip)
	{
		tooltip.AddEffect(TooltipEffectTrigger.Use, 6610040); /* Heals you over time. */
	}

	public void OnConsume(MobileEntity entity, Consumable item)
	{
		entity.BalmTimer = new BalmTimer(entity);
	}
		
	public void Serialize(SpanWriter writer)
	{
		writer.Write((byte)1);
	}

	public void Deserialize(ref SpanReader reader)
	{
		var version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}