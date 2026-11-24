using System.Collections.Generic;
using Kesmai.Server.Engines.Library;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

/// <summary>
/// A library publication on a scroll, usually a <see cref="PublishedScroll"/>. It reads like a <see cref="Book"/>:
/// looking at it adds the publication to the reader's library, shows it, and uses the scroll up. Only its name,
/// description and default sprite differ.
/// </summary>
/// <example>
/// A segment's treasure that drops a library scroll:
/// <code>return new Scroll("stormhalter.portal-has-moved");</code>
/// </example>
public class Scroll : Book
{
	/// <inheritdoc />
	public override int LabelNumber => 6000078; /* scroll */

	/// <summary>
	/// Initializes a new instance of the <see cref="Scroll"/> class.
	/// </summary>
	public Scroll(string publicationId) : base(publicationId)
	{
		/* Shown only if the publication isn't found; a publication sets its own sprite. */
		ItemId = LibraryCollection.DefaultScrollItemId;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Scroll"/> class.
	/// </summary>
	public Scroll(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		if (Publication is not null)
			entries.Add(new LocalizationEntry(
				6200399, Publication.Title, Publication.Author)); /* [You are looking at a scroll entitled '{0}' written by {1}.] */
	}

	/// <inheritdoc />
	public override void GetTooltipDescription(List<LocalizationEntry> entries)
	{
		if (Publication is not null)
			entries.Add(new LocalizationEntry(
				6200400, Publication.Title, Publication.Author)); /* a scroll entitled '{0}' written by {1}. */
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}
