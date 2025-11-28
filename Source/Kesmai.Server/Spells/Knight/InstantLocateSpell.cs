using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Gumps;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Spells;

/*
Knights use this spell to aid in tracking down other beings. To locate the nearest orc, for example, double left 
click on the spell icon.  

An Information Query Pop-up will appear, enabling you to enter the name of the creature you wish to locate.  
In this case, enter "orc" and then left click on "OK" or press the <Enter> key on your keyboard.

A message will appear on the main viewport indicating the creature's general direction and approximate distance.
The creature name may be entered using standard abbreviations.  Other adventurers can be located in this way also.
 */
public class InstantLocateSpell : InstantSpell
{
	private static SpellInfo _info = new SpellInfo(15, "Locate", typeof(InstantLocateSpell), 3);

	private static Dictionary<int, string> _distanceTable = new Dictionary<int, string>()
	{
		[0] = "very near",
		[6] = "near",
		[15] = "far away",
		[30] = "very far away",
	};
	
	public override string Name => _info.Name;
		
	public override bool AllowInterrupt => false;

	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			_caster.CloseGumps<LocateSpellGump>();
			_caster.SendGump(new LocateSpellGump(_caster, this));
		}
		else
		{
			Fizzle();
			FinishSequence();
		}
	}
		
	public virtual void CastAt(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			_caster.EmitSound(226, 3, 6);

			if (target != null)
			{
				base.OnCast();
					
				var segment = _caster.Segment;

				var casterLocation = _caster.Location;
				var targetLocation = target.Location;

				var casterAltitude = segment.GetElevation(casterLocation);
				var targetAltitude = segment.GetElevation(targetLocation);

				if (casterAltitude > targetAltitude)
				{
					_caster.SendLocalizedMessage(6300312, target.Name, (casterAltitude - targetAltitude).ToString());
				}
				else if (casterAltitude < targetAltitude)
				{
					_caster.SendLocalizedMessage(6300313, target.Name, (targetAltitude - casterAltitude).ToString());
				}
				else
				{
					var distance = _caster.GetDistanceToMax(targetLocation);
					var direction = Direction.GetDirection(casterLocation, targetLocation);

					if (direction != Direction.None)
					{
						_caster.SendLocalizedMessage(6300314, target.Name,
							_distanceTable.LastOrDefault(kvp => kvp.Key < distance).Value,
							direction.GetAlias());
					}
					else
					{
						_caster.SendLocalizedMessage(6300315, target.Name);
					}
				}

				if (_item is ICharged charged)
					charged.ChargesCurrent--;
			}
			else
			{
				Fizzle();
			}
		}
		else
		{
			Fizzle();
		}
			
		FinishSequence();
	}

	public void Query(string name)
	{
		var segment = _caster.Segment;
		var mobiles = segment.Groups.Select(g => g.Leader).ToList();
			
		var players = mobiles.OfType<PlayerEntity>().Where(p => p.RespondsTo(name)).ToList();

		if (players.Any())
		{
			CastAt(players.FirstOrDefault());
			return;
		}
			
		var creatures = mobiles.OfType<CreatureEntity>().Where(c => c.RespondsTo(name)).ToList();
			
		if (creatures.Any())
		{
			CastAt(creatures.FirstOrDefault());
			return;
		}

		CastAt(default(MobileEntity));
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var match = Regex.Match(arguments, @"^at (.*)$", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			Warm(source);
			Query(match.Groups[1].Value);
			return true;
		}

		return base.OnCastCommand(source, arguments);
	}
		
	protected override void OnCancel(bool silent)
	{
		base.OnCancel(silent);
			
		_caster.CloseGumps<LocateSpellGump>();
	}
}

public class LocateSpellGump : LocalizedGump
{
	private MobileEntity _caster;
	private InstantLocateSpell _spell;
		
	public override string Template => "Server-Locate";
	
	public LocateSpellGump(MobileEntity caster, InstantLocateSpell spell)
	{
		_caster = caster;
		_spell = spell;

		Style = "Client-GameContent-Modal";
		Overlay = true;
		OnEnterKey = "locate";

		HorizontalAlignment = HorizontalAlignment.Center;
		VerticalAlignment = VerticalAlignment.Center;
		
		SetResponseAction("locate", Locate);
	}
	
	public override dynamic GetData()
	{
		return new
		{
			Source = (_spell.Item != default) ? "item" : "spell",
			SourceId = (_spell.Item != default) ? _spell.Item.ItemId : _spell.SpellId
		};
	}

	private void Locate(GumpResponseArgs args)
	{
		var client = args.Client;

		if (args.Texts.TryGetValue("locateQuery", out var query))
		{
			_spell.Query(query);

			if (_spell.Item is ICharged charged)
				charged.ChargesCurrent--;
		}

		client.CloseGump<LocateSpellGump>();
	}

	protected override void OnClose(Client source)
	{
		base.OnClose(source);
			
		if (_caster.Spell != _spell)
			return;
				
		_spell.Cancel();
	}
}