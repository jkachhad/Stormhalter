using System.Linq;
using System.Xml.Linq;
using Kesmai.WorldForge.UI.Documents;

namespace Kesmai.WorldForge.Editor.Items;

/// <summary>
/// The "Items" and "Weapons" entries in the project tree. Selecting one opens the Item Maker or Weapon Maker for the
/// segment. Items live in the segment's Source\Items folder as the C# files the maker writes, so nothing is serialized here.
/// </summary>
public class SegmentItems : ISegmentObject
{
	private readonly Segment _segment;
	private readonly ItemKind _kind;

	public SegmentItems(Segment segment, ItemKind kind)
	{
		_segment = segment;
		_kind = kind;
	}

	public string Name
	{
		get => _kind == ItemKind.Weapon ? "Weapons" : "Items";
		set { }
	}

	public void Present(ApplicationPresenter presenter)
	{
		var maker = presenter.Documents.OfType<ItemMakerViewModel>().FirstOrDefault(m => m.Kind == _kind);

		if (maker is null)
			presenter.Documents.Add(maker = new ItemMakerViewModel(_kind));

		maker.Segment = _segment;

		presenter.SetActiveDocument(maker);
	}

	public void Copy(Segment segment)
	{
	}

	public XElement GetSerializingElement() => new XElement("items");
	public XElement GetReferencingElement() => new XElement("items");
}
