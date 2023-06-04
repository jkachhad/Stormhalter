using System.Collections.Generic;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge;

public abstract class ProfessionInfo
{
	private static readonly Dictionary<int, ProfessionInfo> _all = new Dictionary<int, ProfessionInfo>();

	/// <summary>
	///     Initializes the <see cref="ProfessionInfo" /> class.
	/// </summary>
	static ProfessionInfo()
	{
		Register(Fighter = new FighterProfessionInfo());
		Register(MartialArtist = new MartialArtistProfessionInfo());
		Register(Thaumaturge = new ThaumaturgeProfessionInfo());
		Register(Thief = new ThiefProfessionInfo());
		Register(Wizard = new WizardProfessionInfo());

		Register(Knight = new KnightProfessionInfo());
	}

	/// <summary>
	///     Finds a <see cref="ProfessionInfo" /> instance from the specified id;
	/// </summary>
	public static ProfessionInfo Find(byte id)
	{
		if (_all.TryGetValue(id, out var info))
			return info;

		return default(ProfessionInfo);
	}

	/// <summary>
	///     Gets all registered instances of profession information.
	/// </summary>
	public static List<ProfessionInfo> GetAll()
	{
		return new List<ProfessionInfo>(_all.Values);
	}

	private static void Register(ProfessionInfo info)
	{
		_all[info.Id] = info;
	}

	public static ProfessionInfo Fighter { get; }
	public static ProfessionInfo MartialArtist { get; }
	public static ProfessionInfo Thaumaturge { get; }
	public static ProfessionInfo Thief { get; }
	public static ProfessionInfo Wizard { get; }

	public static ProfessionInfo Knight { get; }

	/// <summary>
	///     Gets the profession id.
	/// </summary>
	public abstract byte Id { get; }

	/// <summary>
	///     Gets the profession name.
	/// </summary>
	public abstract string Name { get; }
}

public class FighterProfessionInfo : ProfessionInfo
{
	public override byte Id => 1;

	public override string Name => "Fighter";
}

public class MartialArtistProfessionInfo : ProfessionInfo
{
	public override byte Id => 2;

	public override string Name => "Martial Artist";
}

public class ThaumaturgeProfessionInfo : ProfessionInfo
{
	public override byte Id => 3;

	public override string Name => "Thaumaturge";
}

public class ThiefProfessionInfo : ProfessionInfo
{
	public override byte Id => 4;

	public override string Name => "Thief";
}

public class WizardProfessionInfo : ProfessionInfo
{
	public override byte Id => 5;

	public override string Name => "Wizard";
}

public class KnightProfessionInfo : ProfessionInfo
{
	public override byte Id => 6;

	public override string Name => "Knight";
}
	
public class ProfessionsItemsSource : IItemsSource
{
	public ItemCollection GetValues()
	{
		var sizes = new ItemCollection();

		foreach (var profession in ProfessionInfo.GetAll())
			sizes.Add(profession, profession.Name);

		return sizes;
	}
}