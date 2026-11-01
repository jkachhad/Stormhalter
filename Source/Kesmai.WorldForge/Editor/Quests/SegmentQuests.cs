using System.Linq;
using System.Xml.Linq;
using Kesmai.WorldForge.UI.Documents;

namespace Kesmai.WorldForge.Editor.Quests;

/// <summary>
/// The "Quests" entry in the project tree. Selecting it opens the Quest Maker for the segment. Quests live in the
/// segment's Source\Quests folder as the C# files the Quest Maker writes, so there is nothing to serialize here.
/// </summary>
public class SegmentQuests : ISegmentObject
{
	private readonly Segment _segment;

	public SegmentQuests(Segment segment)
	{
		_segment = segment;
	}

	public string Name
	{
		get => "Quests";
		set { }
	}

	public void Present(ApplicationPresenter presenter)
	{
		var questMaker = presenter.Documents.OfType<QuestMakerViewModel>().FirstOrDefault();

		if (questMaker is null)
			presenter.Documents.Add(questMaker = new QuestMakerViewModel());

		questMaker.Segment = _segment;

		presenter.SetActiveDocument(questMaker);
	}

	public void Copy(Segment segment)
	{
	}

	public XElement GetSerializingElement() => new XElement("quests");
	public XElement GetReferencingElement() => new XElement("quests");
}
