using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public abstract partial class Questkeeper : VendorEntity
{
	protected Questkeeper()
	{
	}

	private static readonly ConcurrentDictionary<Type, (bool Teach, bool Accept, bool Decline)> _overrides = new();

	private static readonly QuestInteraction _teachEntry = new QuestInteraction(QuestResponse.Teach, new LocalizationEntry("Teach"));
	private static readonly QuestInteraction _acceptEntry = new QuestInteraction(QuestResponse.Accept, new LocalizationEntry("Yes"));
	private static readonly QuestInteraction _declineEntry = new QuestInteraction(QuestResponse.Decline, new LocalizationEntry("No"));

	/// <summary>
	/// Adds Teach, Yes and No for the responses this questkeeper implements (overrides of <see cref="OnTeach"/>,
	/// <see cref="OnAccept"/> and <see cref="OnDecline"/>), unless the subclass already listed an entry for
	/// that response or with that title. These replace the "teach", "yes" and "no" speech.
	/// </summary>
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		if (!ShowStockQuestEntries)
		{
			base.GetInteractions(source, entries);
			return;
		}

		var (teach, accept, decline) = _overrides.GetOrAdd(GetType(), type => (
			Overrides(type, nameof(OnTeach)), Overrides(type, nameof(OnAccept)), Overrides(type, nameof(OnDecline))));

		var added = false;

		if (teach) added |= AddStockEntry(source, entries, _teachEntry);
		if (accept) added |= AddStockEntry(source, entries, _acceptEntry);
		if (decline) added |= AddStockEntry(source, entries, _declineEntry);

		if (added)
			entries.Add(InteractionSeparator.Instance);

		base.GetInteractions(source, entries);
	}

	/// <summary>
	/// Gets whether the stock Teach, Yes and No entries are added. Questkeepers that list their own quest
	/// entries for each stage return false.
	/// </summary>
	protected virtual bool ShowStockQuestEntries => true;

	private static bool Overrides(Type type, string method)
	{
		var info = type.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(PlayerEntity) }, null);

		return info != null && info.DeclaringType != typeof(Questkeeper);
	}

	private static bool AddStockEntry(PlayerEntity source, List<InteractionEntry> entries, QuestInteraction entry)
	{
		var title = entry.GetTitle(source).Arguments.FirstOrDefault();

		var listed = entries.Any(existing => (existing is QuestInteraction quest && quest.Response == entry.Response)
			|| (existing != null && !existing.IsSeparator && String.Equals(existing.GetTitle(source).Arguments.FirstOrDefault(), title, StringComparison.OrdinalIgnoreCase)));

		if (listed)
			return false;

		entries.Add(entry);
		return true;
	}

	/// <summary>
	/// The questkeeper response an entry triggers.
	/// </summary>
	public enum QuestResponse
	{
		Teach,
		Accept,
		Decline,
	}

	/// <summary>
	/// A stock entry that calls <see cref="OnTeach"/>, <see cref="OnAccept"/> or <see cref="OnDecline"/>
	/// with the quest's own title, after the standard vendor and counter checks.
	/// </summary>
	public sealed class QuestInteraction : VendorInteraction
	{
		private readonly QuestResponse _response;

		public QuestResponse Response => _response;

		public QuestInteraction(QuestResponse response, LocalizationEntry title, int range = -1) : base(title, range)
		{
			_response = response;
		}

		protected override bool CanUse(PlayerEntity player, VendorEntity vendor)
		{
			return player.IsAlive && vendor.CanSee(player);
		}

		protected override void OnCannotUse(PlayerEntity player, VendorEntity vendor)
		{
			vendor.SayTo(player, 6300235); /* I cannot serve you if I cannot see you. */
		}

		protected override void OnClick(PlayerEntity player, VendorEntity vendor, Point2D counter)
		{
			if (vendor is not Questkeeper keeper)
				return;

			switch (_response)
			{
				case QuestResponse.Teach: keeper.OnTeach(player); break;
				case QuestResponse.Accept: keeper.OnAccept(player); break;
				case QuestResponse.Decline: keeper.OnDecline(player); break;
			}
		}
	}

	protected virtual void OnTeach(PlayerEntity player)
	{
	}

	protected virtual void OnAccept(PlayerEntity player)
	{
	}

	protected virtual void OnDecline(PlayerEntity player)
	{
	}
}