using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Gumps;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Spells;

public class PeekSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(37, "Peek", typeof(PeekSpell), 14);
		
	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			_caster.CloseGumps<PeekSpellGump>();
			_caster.SendGump(new PeekSpellGump(this));
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
			
		if (_caster != target && CheckSequence())
		{
			if (target != null)
			{
				base.OnCast();
					
				if (_caster is PlayerEntity player)
				{
					player.Peek(target);

					Timer.DelayCall(_caster.Facet.TimeSpan.FromRounds(5), () => Dispel(this));

					if (_item == null)
						player.AwardMagicSkill(this);
				}

				_caster.EmitSound(237, 3, 6);
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
		var mobiles = segment.Groups.SelectMany(g => g.Members).ToList();
			
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
			Query(match.Groups[1].Value);
			return true;
		}

		return false;
	}

	public static void Dispel(Spell spell)
	{
		var caster = spell.Caster;

		if (caster is PlayerEntity player && player.Tranced)
			player.Unpeek();
	}

	protected override void OnCancel(bool silent)
	{
		base.OnCancel(silent);
			
		_caster.CloseGumps<PeekSpellGump>();
	}
}
	
public class PeekSpellGump : LocalizedGump
{
	private PeekSpell _spell;
	
	public override string Template => "Server-Peek";
		
	public PeekSpellGump(PeekSpell spell)
	{
		_spell = spell;
			
		Style = "Client-GameContent-Modal";
		Overlay = true;
		OnEnterKey = "peek";

		HorizontalAlignment = HorizontalAlignment.Center;
		VerticalAlignment = VerticalAlignment.Center;
		
		SetResponseAction("peek", Peek);
	}
	
	public override dynamic GetData()
	{
		return new
		{
			Source = (_spell.Item != default) ? "item" : "spell",
			SourceId = (_spell.Item != default) ? _spell.Item.ItemId : _spell.SpellId
		};
	}

	private void Peek(GumpResponseArgs args)
	{
		var client = args.Client;
		
		if (args.Texts.TryGetValue("peekQuery", out var query))
			_spell.Query(query);
			
		client.CloseGump<PeekSpellGump>();
	}
}
