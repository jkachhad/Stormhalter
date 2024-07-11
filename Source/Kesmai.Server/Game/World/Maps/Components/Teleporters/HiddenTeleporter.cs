using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("HiddenTeleporterComponent")]
public class HiddenTeleporter : Teleporter
{
	private List<Lightphase> _lightphases;
	private List<Moonphase> _moonphases;
	private List<Profession> _professions;
	private List<Alignment> _alignments;
		
	private int _message;
	private int _level;
	private bool _allowNPC;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="HiddenTeleporter"/> class.
	/// </summary>
	public HiddenTeleporter(XElement element) : base(element)
	{
		_lightphases = new List<Lightphase>();
		_moonphases = new List<Moonphase>();
		_professions = new List<Profession>();
		_alignments = new List<Alignment>();
			
		/* Load the lightphases under which this teleporter is active. */
		if (element.TryGetElement("lightphases", out var lightphasesElement))
		{
			if (lightphasesElement.TryGetAttribute("select", out var selectAttribute))
			{
				switch (selectAttribute.Value)
				{
					case "all":
						_lightphases.AddRange(Lightphase.All);
						break;
				}
			}
			else
			{
				foreach (var phase in lightphasesElement.Value.Split(','))
					_lightphases.Add(Lightphase.All.First(p => p.Name.Matches(phase)));
			}
		}
		else
		{
			/* The element doesn't exist, we'll populate with all the phases as default behavior. */
			_lightphases.AddRange(Lightphase.All);
		}

		/* Load the Moonphase under which this teleporter is active. */
		if (element.TryGetElement("moonphases", out var moonphasesElement))
		{
			if (moonphasesElement.TryGetAttribute("select", out var selectAttribute))
			{
				switch (selectAttribute.Value)
				{
					case "all":
						_moonphases.AddRange(Moonphase.All);
						break;
				}
			}
			else
			{
				foreach (var phase in moonphasesElement.Value.Split(','))
					_moonphases.Add(Moonphase.All.First(p => p.Name.Matches(phase)));
			}
		}
		else
		{
			/* The element doesn't exist, we'll populate with all the phases as default behavior. */
			_moonphases.AddRange(Moonphase.All);
		}

		/* Load the professions. */
		var allProfessions = Profession.GetAll();

		if (element.TryGetElement("professions", out var professionsElement))
		{
			if (professionsElement.TryGetAttribute("select", out var selectAttribute))
			{
				switch (selectAttribute.Value)
				{
					case "all":
						_professions.AddRange(allProfessions);
						break;
				}
			}
			else
			{
				foreach (var name in professionsElement.Value.Split(','))
					_professions.Add(allProfessions.First(p => p.Info.Name.Matches(name)));
			}
		}
		else
		{
			/* The element doesn't exist, we'll populate with all the phases as default behavior. */
			_professions.AddRange(allProfessions);
		}

		/* Load the Alignments under which this teleporter is active. */
		if (element.TryGetElement("alignments", out var alignmentsElement))
		{
			if (alignmentsElement.TryGetAttribute("select", out var selectAttribute))
			{
				switch (selectAttribute.Value)
				{
					case "all":
						_alignments.AddRange(Alignment.All);
						break;
				}
			}
			else
			{
				foreach (var alignment in alignmentsElement.Value.Split(','))
					_alignments.Add(Alignment.All.First(p => p.Description.Matches(alignment)));
			}
		}
		else
		{
			/* The element doesn't exist, we'll populate with all the phases as default behavior. */
			_alignments.AddRange(Alignment.All);
		}

		/* message when teleport is successful */
		if (element.TryGetElement("message", out var messageElement))
			_message = (int)messageElement;
			
		/* Level requirement */
		if (element.TryGetElement("level", out var levelElement))
			_level = (int)levelElement;
			
		/* NPCs */
		if (element.TryGetElement("allowNPC", out var allowNPCElement))
			_allowNPC = (bool)allowNPCElement;
	}

	protected override bool CanTeleport(SegmentTile parent, MobileEntity entity)
	{
		var facet = parent.Facet;

		if (_lightphases == null || !_lightphases.Contains(facet.Lightphase))
			return false;

		if (_moonphases == null || !_moonphases.Contains(facet.Moonphase))
			return false;

		if (_alignments == null || !_alignments.Contains(entity.Alignment))
			return false;
			
		if (entity is PlayerEntity player)
		{
			var teleport = _professions.Contains(player.Profession);

			if (!teleport && player.IsCarryingCorpse(out var corpse))
				teleport = CanTeleport(parent, corpse.Owner);

			if (!teleport)
				return false;

			if (_level > 0 && player.Level < _level)
				return false;
		}

		if (entity is CreatureEntity creature && !_allowNPC)
			return false;

		return true;
	}

	protected override bool CanTeleport(SegmentTile parent, ItemEntity entity)
	{
		if (entity is Corpse corpse)
			return CanTeleport(parent, corpse.Owner);

		return base.CanTeleport(parent, entity);
	}

	protected override void OnAfterTeleport(SegmentTile parent, WorldEntity entity)
	{
		base.OnAfterTeleport(parent, entity);

		if (entity is PlayerEntity player && _message > 0)
			player.SendLocalizedMessage(_message);
	}
}