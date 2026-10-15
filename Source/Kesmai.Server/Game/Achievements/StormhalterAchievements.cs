using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Engines.Achievements;
using Kesmai.Server.Engines.Rewards;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

/// <summary>
/// The first set of Stormhalter achievements: levels, skills, kills, exploration, guild hunts and a daily visit.
/// </summary>
/// <remarks>
/// Names and descriptions are in <c>Content/Localization/Enu.xml</c>, in the 6800100 range.
/// Ids are saved with every player's progress, so never rename or reuse one.
/// </remarks>
public sealed class StormhalterAchievements : AchievementSet
{
	/* Rewards given by these achievements. */
	public const string DragonslayerTitle = "achievement-title-dragonslayer";
	public const string WyrmbaneTitle = "achievement-title-wyrmbane";

	private static readonly string[] OldLands = { "Kesmai", "Leng", "Axe Glacier", "Oakvael", "Underkingdom" };
	private static readonly string[] NewLands = { "Annwn", "Mu", "Bloodlands", "Blackburrow", "Keshma" };

	[ServerConfigure]
	public static void Configure()
	{
		/* The test harness configures the server once per test; subscribe only once. */
		EventSink.ServerStarted -= RegisterRewards;
		EventSink.ServerStarted += RegisterRewards;

		EventSink.WorldEnter -= OnWorldEnter;
		EventSink.WorldEnter += OnWorldEnter;
	}

	/// <summary>
	/// Registers the reward titles once; definitions outlive server restarts within one process.
	/// </summary>
	private static void RegisterRewards()
	{
		if (!Rewards.TryGetDefinition(DragonslayerTitle, out _))
			Rewards.Register(new RewardDefinition(DragonslayerTitle, RewardScope.Player, RewardLimit.Unique, new TitleAward(6800401)));

		if (!Rewards.TryGetDefinition(WyrmbaneTitle, out _))
			Rewards.Register(new RewardDefinition(WyrmbaneTitle, RewardScope.Account, RewardLimit.Unique, new TitleAward(6800402)));
	}

	private static void OnWorldEnter(Client client, PlayerEntity player)
	{
		if (Achievements.TryGetAchievement("daily-visit", out var achievement) && achievement is DailyAchievement daily)
			daily.Credit(player);
	}

	private static bool IsDragon(CreatureEntity creature) => creature is Dragon or Drake or Wyvern;
	private static bool IsTroll(CreatureEntity creature) => creature is Troll or TrollGuard or ElderTroll;
	private static bool IsUndead(CreatureEntity creature) => creature is IUndead;

	public override IEnumerable<AchievementCategory> BuildCategories()
	{
		yield return new AchievementCategory("general") { Name = 6800101, Order = 0 };
		yield return new AchievementCategory("combat") { Name = 6800102, Order = 1 };
		yield return new AchievementCategory("skills") { Name = 6800103, Order = 2 };
		yield return new AchievementCategory("exploration") { Name = 6800104, Order = 3 };
		yield return new AchievementCategory("guild") { Name = 6800105, Order = 4 };
		yield return new AchievementCategory("daily") { Name = 6800106, Order = 5 };
	}

	public override IEnumerable<Achievement> Build()
	{
		/* General */
		yield return new LevelAchievement("level-10", AchievementScope.Player, 10)
		{
			Category = "general", Order = 1,
			Name = 6800201, Description = 6800202,
			Points = 10,
		};

		yield return new LevelAchievement("level-20", AchievementScope.Player, 20)
		{
			Category = "general", Order = 2,
			Name = 6800203, Description = 6800204,
			Points = 25,
		};

		yield return new LevelAchievement("account-level-20", AchievementScope.Account, 20)
		{
			Category = "general", Order = 3,
			Name = 6800205, Description = 6800206,
			Points = 25,
		};

		yield return new MetaAchievement("old-hand", AchievementScope.Account)
		{
			Category = "general", Order = 4,
			Name = 6800207, Description = 6800208,
			Points = 50,
			Requirements = new[] { "level-20", "explore-old-lands" },
		};

		/* Combat */
		yield return new KillAchievement("kill-dragons", AchievementScope.Player, IsDragon)
		{
			Category = "combat", Order = 1,
			Name = 6800211, Description = 6800212,
			Stages = new uint[] { 1, 10, 50, 100, 500 },
			Points = 10,
			AnnounceFirst = true,
			Rewards = new[] { DragonslayerTitle },
		};

		yield return new KillAchievement("kill-undead", AchievementScope.Player, IsUndead)
		{
			Category = "combat", Order = 2,
			Name = 6800213, Description = 6800214,
			Stages = new uint[] { 10, 100, 500, 1_000 },
			Points = 10,
		};

		yield return new KillAchievement("kill-trolls", AchievementScope.Player, IsTroll)
		{
			Category = "combat", Order = 3,
			Name = 6800215, Description = 6800216,
			Stages = new uint[] { 10, 50, 250 },
			Points = 10,
		};

		yield return new KillAchievement("kill-wyvern", AchievementScope.Player, creature => creature is Wyvern)
		{
			Category = "combat", Order = 4,
			Name = 6800217, Description = 6800218,
			Points = 15,
			Visibility = AchievementVisibility.Hidden,
		};

		/* Skills: one per skill, named after it. */
		foreach (var skill in Skill.All)
		{
			yield return new SkillAchievement($"skill-{skill.Name}-10", AchievementScope.Player, skill, 10)
			{
				Category = "skills", Order = skill.Id,
				Name = 6800250 + skill.Id, Description = 6800250,
				Points = 10,
			};
		}

		/* Exploration */
		yield return new ExploreAchievement("explore-old-lands", AchievementScope.Player, OldLands)
		{
			Category = "exploration", Order = 1,
			Name = 6800221, Description = 6800222,
			Points = 20,
		};

		yield return new ExploreAchievement("explore-new-lands", AchievementScope.Player, NewLands)
		{
			Category = "exploration", Order = 2,
			Name = 6800223, Description = 6800224,
			Points = 20,
		};

		/* Guild */
		yield return new KillAchievement("guild-kill-dragons", AchievementScope.Guild, IsDragon)
		{
			Category = "guild", Order = 1,
			Name = 6800231, Description = 6800232,
			Stages = new uint[] { 10, 100, 1_000 },
			Points = 25,
			AnnounceFirst = true,
			Rewards = new[] { WyrmbaneTitle },
		};

		yield return new KillAchievement("guild-kill-undead", AchievementScope.Guild, IsUndead)
		{
			Category = "guild", Order = 2,
			Name = 6800233, Description = 6800234,
			Stages = new uint[] { 100, 1_000, 10_000 },
			Points = 25,
		};

		/* Daily */
		yield return new DailyAchievement("daily-visit", AchievementScope.Player)
		{
			Order = 1,
			Name = 6800241, Description = 6800242,
		};
	}
}
