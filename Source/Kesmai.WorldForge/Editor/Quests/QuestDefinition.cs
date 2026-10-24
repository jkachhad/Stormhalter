#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor.Quests;

/// <summary>What a player has to do before a stage's choices open up.</summary>
public enum QuestStageKind
{
	/// <summary>Nothing to do; the choices are offered right away.</summary>
	Talk,
	/// <summary>Kill a number of a segment entity.</summary>
	Kill,
	/// <summary>Bring a number of an item type to the quest giver.</summary>
	Collect,
	/// <summary>Lead a follower to a destination.</summary>
	Escort,
	/// <summary>Finishes the quest and hands out the stage's rewards.</summary>
	End,
}

/// <summary>
/// A quest made in the Quest Maker: a giver NPC and a graph of stages. Each stage has an objective and choices,
/// and each choice leads to another stage, which is how a quest branches. The definition is stored as XML inside
/// the C# file the Quest Maker writes, so the quest can be reopened and edited.
/// </summary>
public class QuestDefinition : ObservableObject
{
	public const string Marker = "// worldforge-quest: ";

	private string _name = "NewQuest";
	private string _title = "A new quest";
	private string _giverName = "Questgiver";
	private int _giverBody = 49;
	private string _intro = "I have a task for you, if you are willing.";
	private string _acceptLabel = "Accept quest";
	private string _completedText = "Thank you again for your help.";
	private string _startStage = "Start";
	private bool _repeatable;

	public string Name { get => _name; set => SetProperty(ref _name, value); }
	public string Title { get => _title; set => SetProperty(ref _title, value); }
	public string GiverName { get => _giverName; set => SetProperty(ref _giverName, value); }
	public int GiverBody { get => _giverBody; set => SetProperty(ref _giverBody, value); }
	public string Intro { get => _intro; set => SetProperty(ref _intro, value); }
	public string AcceptLabel { get => _acceptLabel; set => SetProperty(ref _acceptLabel, value); }
	public string CompletedText { get => _completedText; set => SetProperty(ref _completedText, value); }
	public string StartStage { get => _startStage; set => SetProperty(ref _startStage, value); }
	public bool Repeatable { get => _repeatable; set => SetProperty(ref _repeatable, value); }

	public ObservableCollection<QuestStage> Stages { get; } = new();

	/// <summary>A quest with one stage of each kind, wired into two branches, to start from.</summary>
	public static QuestDefinition CreateSample()
	{
		var quest = new QuestDefinition { Name = "SampleQuest", Title = "Trouble on the Road" };

		var start = new QuestStage { Id = "Start", Kind = QuestStageKind.Talk, Text = "Orcs raid the road and a merchant is missing. Will you fight, or go looking for him?" };
		start.Choices.Add(new QuestChoice { Label = "Fight the orcs", Next = "SlayOrcs" });
		start.Choices.Add(new QuestChoice { Label = "Find the merchant", Next = "EscortMerchant" });

		var slay = new QuestStage { Id = "SlayOrcs", Kind = QuestStageKind.Kill, Text = "Slay the raiders on the road.", DoneText = "The road is safer already. Now bring me their tusks.", Count = 5 };
		slay.Choices.Add(new QuestChoice { Label = "Turn in the kills", Next = "CollectTusks" });

		var collect = new QuestStage { Id = "CollectTusks", Kind = QuestStageKind.Collect, Text = "Bring me their tusks as proof.", DoneText = "Proof enough!", Count = 3, Consume = true };
		collect.Choices.Add(new QuestChoice { Label = "Hand over the tusks", Next = "Done" });

		var escort = new QuestStage { Id = "EscortMerchant", Kind = QuestStageKind.Escort, Text = "He is hiding nearby. Bring him safely back to town.", DoneText = "You brought him home!", EscortName = "merchant", EscortBody = 49, DestinationRange = 2 };
		escort.Choices.Add(new QuestChoice { Label = "Report back", Next = "Done" });

		var done = new QuestStage { Id = "Done", Kind = QuestStageKind.End, Text = "Well done. Take this for your trouble.", RewardExperience = 5000, RewardGold = 100 };

		quest.Stages.Add(start);
		quest.Stages.Add(slay);
		quest.Stages.Add(collect);
		quest.Stages.Add(escort);
		quest.Stages.Add(done);

		return quest;
	}

	public XElement ToXml()
	{
		return new XElement("quest",
			new XAttribute("name", Name),
			new XAttribute("title", Title),
			new XAttribute("giverName", GiverName),
			new XAttribute("giverBody", GiverBody),
			new XAttribute("startStage", StartStage),
			new XAttribute("repeatable", Repeatable),
			new XElement("intro", Intro),
			new XElement("acceptLabel", AcceptLabel),
			new XElement("completedText", CompletedText),
			Stages.Select(s => s.ToXml()));
	}

	public static QuestDefinition FromXml(XElement element)
	{
		var quest = new QuestDefinition
		{
			Name = (string?)element.Attribute("name") ?? "NewQuest",
			Title = (string?)element.Attribute("title") ?? String.Empty,
			GiverName = (string?)element.Attribute("giverName") ?? "Questgiver",
			GiverBody = (int?)element.Attribute("giverBody") ?? 49,
			StartStage = (string?)element.Attribute("startStage") ?? String.Empty,
			Repeatable = (bool?)element.Attribute("repeatable") ?? false,
			Intro = (string?)element.Element("intro") ?? String.Empty,
			AcceptLabel = (string?)element.Element("acceptLabel") ?? "Accept quest",
			CompletedText = (string?)element.Element("completedText") ?? String.Empty,
		};

		foreach (var stageElement in element.Elements("stage"))
			quest.Stages.Add(QuestStage.FromXml(stageElement));

		return quest;
	}

	/// <summary>The definition as the one-line comment the Quest Maker puts at the top of the C# file.</summary>
	public string ToMarkerLine()
		=> Marker + Convert.ToBase64String(Encoding.UTF8.GetBytes(ToXml().ToString(SaveOptions.DisableFormatting)));

	/// <summary>Reads the definition back from a C# file the Quest Maker wrote, or returns null for any other file.</summary>
	public static QuestDefinition? FromSource(string source)
	{
		var line = source.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith(Marker.Trim()));

		if (line is null)
			return null;

		try
		{
			var xml = Encoding.UTF8.GetString(Convert.FromBase64String(line.Substring(Marker.Trim().Length).Trim()));
			return FromXml(XElement.Parse(xml));
		}
		catch
		{
			return null;
		}
	}

	private static readonly Regex _identifier = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

	public static bool IsIdentifier(string? value) => value is not null && _identifier.IsMatch(value);
}

public class QuestStage : ObservableObject
{
	private string _id = "Stage";
	private QuestStageKind _kind;
	private string _text = String.Empty;
	private string _doneText = String.Empty;
	private string _target = String.Empty;
	private int _count = 1;
	private bool _consume = true;
	private string _escortName = "traveler";
	private int _escortBody = 49;
	private int _escortHealth = 200;
	private int _destinationX;
	private int _destinationY;
	private int _destinationRegion;
	private int _destinationRange = 2;
	private long _rewardExperience;
	private int _rewardGold;
	private string _rewardItems = String.Empty;

	/// <summary>The stage's name, used by choices to point at it. Letters, digits and underscores.</summary>
	public string Id { get => _id; set => SetProperty(ref _id, value); }
	public QuestStageKind Kind { get => _kind; set => SetProperty(ref _kind, value); }

	/// <summary>What the giver says when the stage starts and when the player talks to them during it.</summary>
	public string Text { get => _text; set => SetProperty(ref _text, value); }

	/// <summary>What the giver says once the objective is done.</summary>
	public string DoneText { get => _doneText; set => SetProperty(ref _doneText, value); }

	/// <summary>Kill: the segment entity name. Collect: the item class name.</summary>
	public string Target { get => _target; set => SetProperty(ref _target, value); }
	public int Count { get => _count; set => SetProperty(ref _count, value); }

	/// <summary>Collect: whether the items are taken when the player turns them in.</summary>
	public bool Consume { get => _consume; set => SetProperty(ref _consume, value); }

	public string EscortName { get => _escortName; set => SetProperty(ref _escortName, value); }
	public int EscortBody { get => _escortBody; set => SetProperty(ref _escortBody, value); }
	public int EscortHealth { get => _escortHealth; set => SetProperty(ref _escortHealth, value); }
	public int DestinationX { get => _destinationX; set => SetProperty(ref _destinationX, value); }
	public int DestinationY { get => _destinationY; set => SetProperty(ref _destinationY, value); }
	public int DestinationRegion { get => _destinationRegion; set => SetProperty(ref _destinationRegion, value); }
	public int DestinationRange { get => _destinationRange; set => SetProperty(ref _destinationRange, value); }

	/// <summary>Given when the player turns the stage in, or on reaching it for an End stage.</summary>
	public long RewardExperience { get => _rewardExperience; set => SetProperty(ref _rewardExperience, value); }
	public int RewardGold { get => _rewardGold; set => SetProperty(ref _rewardGold, value); }

	/// <summary>Item class names separated by commas, or full constructor calls like <c>new Gem(500)</c>.</summary>
	public string RewardItems { get => _rewardItems; set => SetProperty(ref _rewardItems, value); }

	public ObservableCollection<QuestChoice> Choices { get; } = new();

	public override string ToString() => $"{Id} ({Kind})";

	public XElement ToXml()
	{
		return new XElement("stage",
			new XAttribute("id", Id),
			new XAttribute("kind", Kind),
			new XElement("text", Text),
			new XElement("doneText", DoneText),
			new XElement("objective",
				new XAttribute("target", Target),
				new XAttribute("count", Count),
				new XAttribute("consume", Consume)),
			new XElement("escort",
				new XAttribute("name", EscortName),
				new XAttribute("body", EscortBody),
				new XAttribute("health", EscortHealth),
				new XAttribute("x", DestinationX),
				new XAttribute("y", DestinationY),
				new XAttribute("region", DestinationRegion),
				new XAttribute("range", DestinationRange)),
			new XElement("rewards",
				new XAttribute("experience", RewardExperience),
				new XAttribute("gold", RewardGold),
				new XAttribute("items", RewardItems)),
			Choices.Select(c => new XElement("choice", new XAttribute("label", c.Label), new XAttribute("next", c.Next))));
	}

	public static QuestStage FromXml(XElement element)
	{
		var objective = element.Element("objective");
		var escort = element.Element("escort");
		var rewards = element.Element("rewards");

		var stage = new QuestStage
		{
			Id = (string?)element.Attribute("id") ?? "Stage",
			Kind = Enum.TryParse<QuestStageKind>((string?)element.Attribute("kind"), out var kind) ? kind : QuestStageKind.Talk,
			Text = (string?)element.Element("text") ?? String.Empty,
			DoneText = (string?)element.Element("doneText") ?? String.Empty,
			Target = (string?)objective?.Attribute("target") ?? String.Empty,
			Count = (int?)objective?.Attribute("count") ?? 1,
			Consume = (bool?)objective?.Attribute("consume") ?? true,
			EscortName = (string?)escort?.Attribute("name") ?? "traveler",
			EscortBody = (int?)escort?.Attribute("body") ?? 49,
			EscortHealth = (int?)escort?.Attribute("health") ?? 200,
			DestinationX = (int?)escort?.Attribute("x") ?? 0,
			DestinationY = (int?)escort?.Attribute("y") ?? 0,
			DestinationRegion = (int?)escort?.Attribute("region") ?? 0,
			DestinationRange = (int?)escort?.Attribute("range") ?? 2,
			RewardExperience = (long?)rewards?.Attribute("experience") ?? 0,
			RewardGold = (int?)rewards?.Attribute("gold") ?? 0,
			RewardItems = (string?)rewards?.Attribute("items") ?? String.Empty,
		};

		foreach (var choice in element.Elements("choice"))
			stage.Choices.Add(new QuestChoice { Label = (string?)choice.Attribute("label") ?? String.Empty, Next = (string?)choice.Attribute("next") ?? String.Empty });

		return stage;
	}
}

/// <summary>An option the giver offers once a stage's objective is done. Picking it moves the player to <see cref="Next"/>.</summary>
public class QuestChoice : ObservableObject
{
	private string _label = "Continue";
	private string _next = String.Empty;

	public string Label { get => _label; set => SetProperty(ref _label, value); }
	public string Next { get => _next; set => SetProperty(ref _next, value); }
}
