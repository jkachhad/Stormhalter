#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>
/// The creature's Shift+Right Click options: <c>creature.Gossips = new List&lt;GossipInteraction&gt;() { new ("Title",
/// (humanoid, player) => { ... }) }</c>. Each option can require a quest state and runs a list of steps, such as saying
/// a line or starting a quest. Quest states are kept per player in Storage under <c>Quest:Name</c>.
/// </summary>
public partial class OnSpawnBuilder
{
	/// <summary>The options, in menu order.</summary>
	public ObservableCollection<DialogueOption> Dialogue { get; } = new();

	// written back as "[ ... ]" when the script used a collection expression
	private bool _dialogueAsCollectionExpression;

	public const string QuestKeyPrefix = "Quest:";

	/// <summary>Quest states as shown in the form; "Not started" is stored as an empty string.</summary>
	public static readonly string[] QuestStates = ["Not started", "Enabled", "Started", "Completed"];

	public static readonly string[] RequirementComparisons = [DialogueOption.Is, DialogueOption.IsNot];

	private static string StoredState(string state) => state.Trim() == "Not started" ? String.Empty : state.Trim();

	private static string ShownState(string stored) => stored.Length == 0 ? "Not started" : stored;

	private static string QuestState(string player, string quest)
		=> $"Storage.Get({player}).Get<string>({Quote(QuestKeyPrefix + quest.Trim())}, \"\")";

	private List<string> GenerateDialogue(string v)
	{
		var lines = new List<string>();
		var options = Dialogue.Where(o => Has(o.Title)).ToList();

		if (options.Count == 0)
			return lines;

		lines.Add(_dialogueAsCollectionExpression ? $"{v}.Gossips =" : $"{v}.Gossips = new List<GossipInteraction>()");
		lines.Add(_dialogueAsCollectionExpression ? "[" : "{");

		foreach (var option in options)
		{
			var h = Has(option.HumanoidName) ? option.HumanoidName.Trim() : "humanoid";
			var p = Has(option.PlayerName) ? option.PlayerName.Trim() : "player";
			var body = new List<string>();

			if (Has(option.RequiredQuest))
			{
				var comparison = option.RequiredComparison == DialogueOption.IsNot ? "==" : "!=";

				body.Add($"if ({QuestState(p, option.RequiredQuest)} {comparison} {Quote(StoredState(option.RequiredState))})");
				body.Add("{");

				if (Has(option.OtherwiseSay))
					body.Add($"\t{h}.SayTo({p}, {Message(option.OtherwiseSay)});");

				body.Add("\treturn;");
				body.Add("}");
			}

			foreach (var step in option.Steps)
				body.AddRange(GenerateStep(step, h, p));

			// an option read as "(h, p) => one call" is written the same way while it is still one call
			if (option.ExpressionBody && body is [var single] && !single.Contains('\n') && single.EndsWith(";")
			    && SyntaxFactory.ParseStatement(single) is ExpressionStatementSyntax)
			{
				lines.Add($"\t{option.Creation}({Quote(option.Title.Trim())}, ({h}, {p}) => {single.TrimEnd(';')}),");
				continue;
			}

			lines.Add($"\t{option.Creation}({Quote(option.Title.Trim())}, ({h}, {p}) =>");
			lines.Add("\t{");
			lines.AddRange(body.SelectMany(l => l.Split('\n')).Select(l => "\t\t" + l.TrimEnd('\r')));
			lines.Add("\t}),");
		}

		lines.Add(_dialogueAsCollectionExpression ? "];" : "};");
		lines.Add(String.Empty);

		return lines;
	}

	private static IEnumerable<string> GenerateStep(DialogueStep step, string h, string p)
	{
		var value = step.Value?.Trim() ?? String.Empty;

		switch (step.Kind)
		{
			case DialogueStep.Say when Has(value):
				yield return $"{h}.SayTo({p}, {Message(value)});";
				break;
			case DialogueStep.MessagePlayer when Has(value):
				yield return $"{p}.SendMessage(Color.Gold, {Message(value)});";
				break;
			case DialogueStep.SetQuest when Has(step.Quest):
				yield return $"Storage.Get({p}).Set<string>({Quote(QuestKeyPrefix + step.Quest.Trim())}, {Quote(StoredState(Has(value) ? value : "Started"))});";
				break;
			case DialogueStep.GiveGold when Has(value):
				yield return $"new Gold({value}).Move({p}.Location, true, {p}.Segment);";
				break;
			case DialogueStep.GiveExperience when Has(value):
				yield return $"{p}.AwardExperience({value}, false);";
				break;
			case DialogueStep.GiveItem when Has(value):
				yield return $"new {value}().Move({p}.Location, true, {p}.Segment);";
				break;
			case DialogueStep.Code when Has(value):
				yield return value.Replace("\r\n", "\n");
				break;
		}
	}

	/// <summary>Reads <c>creature.Gossips = ...</c> when every option is one the tab can show; otherwise it stays kept code.</summary>
	private bool ReadDialogue(ExpressionSyntax value)
	{
		IEnumerable<ExpressionSyntax>? entries = value switch
		{
			ObjectCreationExpressionSyntax { Type: GenericNameSyntax { Identifier.Text: "List", TypeArgumentList.Arguments: [IdentifierNameSyntax { Identifier.Text: "GossipInteraction" }] }, ArgumentList.Arguments.Count: 0, Initializer: { } list }
				when list.IsKind(SyntaxKind.CollectionInitializerExpression) => list.Expressions,
			ObjectCreationExpressionSyntax { Type: GenericNameSyntax { Identifier.Text: "List", TypeArgumentList.Arguments: [IdentifierNameSyntax { Identifier.Text: "GossipInteraction" }] }, ArgumentList: null, Initializer: { } bare }
				when bare.IsKind(SyntaxKind.CollectionInitializerExpression) => bare.Expressions,
			CollectionExpressionSyntax collection when collection.Elements.All(e => e is ExpressionElementSyntax)
				=> collection.Elements.Cast<ExpressionElementSyntax>().Select(e => e.Expression),
			_ => null,
		};

		if (entries is null || Dialogue.Count > 0)
			return false;

		var options = new List<DialogueOption>();

		foreach (var entry in entries)
		{
			var (creation, arguments) = entry switch
			{
				ImplicitObjectCreationExpressionSyntax { Initializer: null } implicitNew => ("new ", implicitNew.ArgumentList.Arguments),
				ObjectCreationExpressionSyntax { Type: IdentifierNameSyntax { Identifier.Text: "GossipInteraction" }, Initializer: null, ArgumentList: { } explicitArguments }
					=> ("new GossipInteraction", explicitArguments.Arguments),
				_ => (null, default),
			};

			if (creation is null
			    || arguments is not [{ NameColon: null, Expression: LiteralExpressionSyntax title }, { NameColon: null, Expression: ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters: [var humanoid, var player], AsyncKeyword.RawKind: 0 } lambda }]
			    || !title.IsKind(SyntaxKind.StringLiteralExpression) || title.Token.Text.StartsWith("@")
			    || humanoid.Type != null || player.Type != null)
				return false;

			var option = new DialogueOption
			{
				Creation = creation,
				Title = title.Token.ValueText,
				HumanoidName = humanoid.Identifier.Text,
				PlayerName = player.Identifier.Text,
				ExpressionBody = lambda.ExpressionBody != null,
			};

			var statements = lambda.Block?.Statements.ToList()
			                 ?? (lambda.ExpressionBody is { } expression ? [SyntaxFactory.ExpressionStatement(expression)] : []);

			if (statements.Count > 0 && ReadRequirement(statements[0], option))
				statements.RemoveAt(0);

			foreach (var statement in statements)
				option.Steps.Add(ReadStep(statement, option.HumanoidName, option.PlayerName));

			options.Add(option);
		}

		_dialogueAsCollectionExpression = value is CollectionExpressionSyntax;

		foreach (var option in options)
			Dialogue.Add(option);

		return true;
	}

	/// <summary>The quest name in <c>Storage.Get(player).Get&lt;string&gt;("Quest:Name", "")</c>.</summary>
	private static string? ReadQuestState(ExpressionSyntax expression, string player)
		=> expression is InvocationExpressionSyntax
		   {
			   Expression: MemberAccessExpressionSyntax
			   {
				   Name: GenericNameSyntax { Identifier.Text: "Get", TypeArgumentList.Arguments: [PredefinedTypeSyntax { Keyword.Text: "string" }] },
				   Expression: InvocationExpressionSyntax
				   {
					   Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "Storage" }, Name.Identifier.Text: "Get" },
					   ArgumentList.Arguments: [{ NameColon: null, Expression: IdentifierNameSyntax storageOwner }],
				   },
			   },
			   ArgumentList.Arguments: [{ NameColon: null, Expression: LiteralExpressionSyntax key }, { NameColon: null, Expression: LiteralExpressionSyntax { Token.ValueText: "" } fallback }],
		   } && storageOwner.Identifier.Text == player && key.IsKind(SyntaxKind.StringLiteralExpression)
		     && fallback.IsKind(SyntaxKind.StringLiteralExpression) && key.Token.ValueText.StartsWith(QuestKeyPrefix)
			? key.Token.ValueText.Substring(QuestKeyPrefix.Length)
			: null;

	/// <summary>Reads <c>if (quest state != "State") { [say;] return; }</c>, the requirement the tab writes.</summary>
	private static bool ReadRequirement(StatementSyntax statement, DialogueOption option)
	{
		if (statement is not IfStatementSyntax { Else: null, Condition: BinaryExpressionSyntax { Right: LiteralExpressionSyntax state } condition, Statement: BlockSyntax block }
		    || !state.IsKind(SyntaxKind.StringLiteralExpression)
		    || !(condition.IsKind(SyntaxKind.NotEqualsExpression) || condition.IsKind(SyntaxKind.EqualsExpression))
		    || ReadQuestState(condition.Left, option.PlayerName) is not string quest)
			return false;

		var inside = block.Statements.ToList();

		if (inside.Count is not (1 or 2) || inside[^1] is not ReturnStatementSyntax { Expression: null })
			return false;

		if (inside.Count == 2)
		{
			if (ReadStep(inside[0], option.HumanoidName, option.PlayerName) is not { Kind: DialogueStep.Say } say)
				return false;

			option.OtherwiseSay = say.Value;
		}

		option.RequiredQuest = quest;
		option.RequiredComparison = condition.IsKind(SyntaxKind.NotEqualsExpression) ? DialogueOption.Is : DialogueOption.IsNot;
		option.RequiredState = ShownState(state.Token.ValueText);

		return true;
	}

	private static bool IsGiveMove(InvocationExpressionSyntax call, string player, out ObjectCreationExpressionSyntax? created)
	{
		created = null;

		if (call is not
		    {
			    Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Move", Expression: ObjectCreationExpressionSyntax { Initializer: null } item },
			    ArgumentList.Arguments: [{ NameColon: null, Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax at, Name.Identifier.Text: "Location" } }, { NameColon: null, Expression: var stack }, { NameColon: null, Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax segmentOwner, Name.Identifier.Text: "Segment" } }],
		    }
		    || at.Identifier.Text != player || segmentOwner.Identifier.Text != player || !stack.IsKind(SyntaxKind.TrueLiteralExpression))
			return false;

		created = item;
		return true;
	}

	/// <summary>One statement as a step; anything the tab has no step for is kept as a Code step, as typed.</summary>
	private static DialogueStep ReadStep(StatementSyntax statement, string h, string p)
	{
		if (statement is ExpressionStatementSyntax { Expression: InvocationExpressionSyntax call })
		{
			switch (call)
			{
				// humanoid.SayTo(player, "text");
				case { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax speaker, Name.Identifier.Text: "SayTo" }, ArgumentList.Arguments: [{ NameColon: null, Expression: IdentifierNameSyntax listener }, { NameColon: null, Expression: var said }] }
					when speaker.Identifier.Text == h && listener.Identifier.Text == p && IsString(said):
					return new DialogueStep { Kind = DialogueStep.Say, Value = MessageText(said) };

				// player.SendMessage(Color.Gold, "text");
				case { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax target, Name.Identifier.Text: "SendMessage" }, ArgumentList.Arguments: [{ NameColon: null, Expression: var color }, { NameColon: null, Expression: var sent }] }
					when target.Identifier.Text == p && Member(color, "Color") == "Gold" && IsString(sent):
					return new DialogueStep { Kind = DialogueStep.MessagePlayer, Value = MessageText(sent) };

				// player.AwardExperience(amount, false);
				case { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax awarded, Name.Identifier.Text: "AwardExperience" }, ArgumentList.Arguments: [{ NameColon: null, Expression: var amount }, { NameColon: null, Expression: var shared }] }
					when awarded.Identifier.Text == p && shared.IsKind(SyntaxKind.FalseLiteralExpression):
					return new DialogueStep { Kind = DialogueStep.GiveExperience, Value = amount.ToString() };

				// Storage.Get(player).Set<string>("Quest:Name", "State");
				case
				{
					Expression: MemberAccessExpressionSyntax
					{
						Name: GenericNameSyntax { Identifier.Text: "Set", TypeArgumentList.Arguments: [PredefinedTypeSyntax { Keyword.Text: "string" }] },
						Expression: InvocationExpressionSyntax
						{
							Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "Storage" }, Name.Identifier.Text: "Get" },
							ArgumentList.Arguments: [{ NameColon: null, Expression: IdentifierNameSyntax storageOwner }],
						},
					},
					ArgumentList.Arguments: [{ NameColon: null, Expression: LiteralExpressionSyntax key }, { NameColon: null, Expression: LiteralExpressionSyntax state }],
				} when storageOwner.Identifier.Text == p && key.IsKind(SyntaxKind.StringLiteralExpression) && state.IsKind(SyntaxKind.StringLiteralExpression)
				       && key.Token.ValueText.StartsWith(QuestKeyPrefix) && QuestStates.Contains(ShownState(state.Token.ValueText)):
					return new DialogueStep { Kind = DialogueStep.SetQuest, Quest = key.Token.ValueText.Substring(QuestKeyPrefix.Length), Value = ShownState(state.Token.ValueText) };
			}

			// new Gold(amount) / new Item() .Move(player.Location, true, player.Segment);
			if (IsGiveMove(call, p, out var created))
			{
				if (created!.Type is IdentifierNameSyntax { Identifier.Text: "Gold" } && created.ArgumentList?.Arguments is [{ NameColon: null } gold])
					return new DialogueStep { Kind = DialogueStep.GiveGold, Value = gold.Expression.ToString() };

				if (created.ArgumentList is null or { Arguments.Count: 0 })
					return new DialogueStep { Kind = DialogueStep.GiveItem, Value = created.Type.ToString() };
			}
		}

		return new DialogueStep { Kind = DialogueStep.Code, Value = Verbatim([statement]) };
	}
}

/// <summary>One Shift+Right Click option.</summary>
public class DialogueOption : ObservableObject
{
	public const string Is = "is";
	public const string IsNot = "is not";

	private string _title = String.Empty;
	private string _requiredQuest = String.Empty;
	private string _requiredComparison = Is;
	private string _requiredState = "Started";
	private string _otherwiseSay = String.Empty;

	/// <summary>The menu text.</summary>
	public string Title { get => _title; set => SetProperty(ref _title, value); }

	/// <summary>When set, the option only does its steps while this quest is (or is not) in <see cref="RequiredState"/>.</summary>
	public string RequiredQuest { get => _requiredQuest; set => SetProperty(ref _requiredQuest, value); }
	public string RequiredComparison { get => _requiredComparison; set => SetProperty(ref _requiredComparison, value); }
	public string RequiredState { get => _requiredState; set => SetProperty(ref _requiredState, value); }

	/// <summary>What the creature says when the requirement isn't met.</summary>
	public string OtherwiseSay { get => _otherwiseSay; set => SetProperty(ref _otherwiseSay, value); }

	public ObservableCollection<DialogueStep> Steps { get; } = new();

	// the script's own spelling, kept so a read and write changes nothing
	public string Creation { get; set; } = "new ";
	public string HumanoidName { get; set; } = "humanoid";
	public string PlayerName { get; set; } = "player";
	public bool ExpressionBody { get; set; }

	public DialogueOption()
	{
		// a change to a step counts as a change to the option, so the code preview follows it
		Steps.CollectionChanged += (_, args) =>
		{
			foreach (DialogueStep step in args.NewItems ?? Array.Empty<DialogueStep>())
				step.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Steps));

			OnPropertyChanged(nameof(Steps));
		};
	}

	public override string ToString() => String.IsNullOrWhiteSpace(Title) ? "(no title)" : Title;
}

/// <summary>Something an option does, in order.</summary>
public class DialogueStep : ObservableObject
{
	public const string Say = "Say";
	public const string MessagePlayer = "Message player";
	public const string SetQuest = "Set quest";
	public const string GiveGold = "Give gold";
	public const string GiveExperience = "Give experience";
	public const string GiveItem = "Give item";
	public const string Code = "Code";

	public static readonly string[] Kinds = [Say, SetQuest, GiveGold, GiveExperience, GiveItem, MessagePlayer, Code];

	private string _kind = Say;
	private string _quest = String.Empty;
	private string _value = String.Empty;

	public string Kind { get => _kind; set => SetProperty(ref _kind, value); }

	/// <summary>The quest a Set quest step changes.</summary>
	public string Quest { get => _quest; set => SetProperty(ref _quest, value); }

	/// <summary>The text, amount, item class, quest state or code, depending on the kind.</summary>
	public string Value { get => _value; set => SetProperty(ref _value, value); }
}
