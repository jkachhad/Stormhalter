#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>
/// Lists the choices offered by the OnSpawn asset panel: server types read from the Kesmai.Server.Reference
/// package, and the bodies and sounds defined in the custom art folder (Body-External.xml, Audio-External.xml).
/// </summary>
public class AssetCatalog
{
	private static Task<AssetCatalog>? _loading;

	/// <summary>Loads the catalog once; later calls return the same instance.</summary>
	public static Task<AssetCatalog> LoadAsync() => _loading ??= Task.Run(CreateAsync);

	public IReadOnlyList<string> Creatures { get; private set; } = [];
	public IReadOnlyList<string> Brains { get; private set; } = [];
	public IReadOnlyList<string> Weapons { get; private set; } = [];
	public IReadOnlyList<string> Shields { get; private set; } = [];
	public IReadOnlyList<string> Armor { get; private set; } = [];
	public IReadOnlyList<string> Helmets { get; private set; } = [];
	public IReadOnlyList<string> Gauntlets { get; private set; } = [];
	public IReadOnlyList<string> Boots { get; private set; } = [];
	public IReadOnlyList<string> Jewelry { get; private set; } = [];
	public IReadOnlyList<string> Spells { get; private set; } = [];
	public IReadOnlyList<string> SearchStrategies { get; private set; } = [];
	public IReadOnlyList<string> ShieldPenetration { get; private set; } = [];
	public IReadOnlyList<string> Immunities { get; private set; } = [];
	public IReadOnlyList<string> Weaknesses { get; private set; } = [];

	public IReadOnlyList<string> Alignments { get; } = ["Lawful", "Neutral", "Chaotic", "Evil"];

	public IReadOnlyList<string> NamedColors { get; } = typeof(System.Drawing.Color)
		.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
		.Where(p => p.PropertyType == typeof(System.Drawing.Color))
		.Select(p => p.Name)
		.ToList();

	public IReadOnlyList<BodyAsset> Bodies { get; private set; } = [];
	public IReadOnlyList<SoundAsset> Sounds { get; private set; } = [];

	/// <summary>Problems found while loading, shown to the user instead of failing silently.</summary>
	public IReadOnlyList<string> Warnings { get; private set; } = [];

	private static async Task<AssetCatalog> CreateAsync()
	{
		var catalog = new AssetCatalog();
		var warnings = new List<string>();

		try
		{
			await catalog.LoadTypesAsync();
		}
		catch (Exception e)
		{
			warnings.Add($"Server types could not be loaded: {e.Message}");
		}

		var contentPath = Core.CustomArtPath ?? String.Empty;

		catalog.Bodies = LoadBodies(contentPath, warnings);
		catalog.Sounds = LoadSounds(contentPath, warnings);
		catalog.Warnings = warnings;

		return catalog;
	}

	private async Task LoadTypesAsync()
	{
		var packageReader = await NuGetResolver.Resolve("Kesmai.Server.Reference");
		var references = await NuGetResolver.ResolveMetadataReferences(packageReader);

		var compilation = CSharpCompilation.Create("AssetCatalog", references: references);
		var types = new List<INamedTypeSymbol>();

		foreach (var assembly in compilation.References
			         .Select(compilation.GetAssemblyOrModuleSymbol)
			         .OfType<IAssemblySymbol>()
			         .Where(a => a.Name.StartsWith("Kesmai.Server")))
			CollectTypes(assembly.GlobalNamespace, types);

		bool DerivesFrom(INamedTypeSymbol type, string baseName)
		{
			for (var current = type.BaseType; current != null; current = current.BaseType)
			{
				if (current.Name == baseName)
					return true;
			}

			return false;
		}

		List<string> Concrete(params string[] baseNames) => types
			.Where(t => !t.IsAbstract && !t.IsGenericType && t.DeclaredAccessibility == Accessibility.Public)
			.Where(t => baseNames.Any(b => DerivesFrom(t, b)))
			.Select(t => t.Name)
			.Distinct()
			.OrderBy(n => n)
			.ToList();

		List<string> EnumMembers(string name) => types
			.FirstOrDefault(t => t.TypeKind == TypeKind.Enum && t.Name == name)?
			.GetMembers().OfType<IFieldSymbol>()
			.Where(f => f.HasConstantValue && f.Name != "None")
			.Select(f => f.Name)
			.ToList() ?? [];

		Creatures = Concrete("CreatureEntity");
		Brains = Concrete("AIBrain");
		Weapons = Concrete("MeleeWeapon", "ProjectileWeapon");
		Shields = Concrete("Shield");
		Armor = Concrete("Armor", "Jacket", "Robe");
		Helmets = Concrete("Helmet");
		Gauntlets = Concrete("Gauntlets");
		Boots = Concrete("Boots");
		Jewelry = Concrete("Ring", "Amulet", "Bracelet");
		Spells = Concrete("Spell");
		SearchStrategies = Concrete("CombatantSearchStrategy");

		var penetration = EnumMembers("ShieldPenetration");
		penetration.Insert(0, "None");

		ShieldPenetration = penetration;
		Immunities = EnumMembers("CreatureImmunity");
		Weaknesses = EnumMembers("CreatureWeakness");
	}

	private static void CollectTypes(INamespaceSymbol ns, List<INamedTypeSymbol> types)
	{
		foreach (var type in ns.GetTypeMembers())
			types.Add(type);

		foreach (var child in ns.GetNamespaceMembers())
			CollectTypes(child, types);
	}

	private static List<BodyAsset> LoadBodies(string contentPath, List<string> warnings)
	{
		var path = Path.Combine(contentPath, "Data", "Body-External.xml");

		if (!File.Exists(path))
		{
			warnings.Add($"No body list at {path}. Point WorldForge's custom art folder at the repository's Content folder to see bodies.");
			return [];
		}

		try
		{
			return XDocument.Load(path).Root!
				.Elements("body")
				.Select(element => BodyAsset.FromElement(element, contentPath))
				.Where(body => body != null)
				.Select(body => body!)
				.OrderBy(body => body.Id)
				.ToList();
		}
		catch (Exception e)
		{
			warnings.Add($"Body-External.xml could not be read: {e.Message}");
			return [];
		}
	}

	private static List<SoundAsset> LoadSounds(string contentPath, List<string> warnings)
	{
		var path = Path.Combine(contentPath, "Data", "Audio-External.xml");

		if (!File.Exists(path))
		{
			warnings.Add($"No sound list at {path}.");
			return [];
		}

		try
		{
			return XDocument.Load(path).Root!
				.Elements("definition")
				.Select(element => SoundAsset.FromElement(element, contentPath))
				.OrderBy(sound => sound.Id)
				.ToList();
		}
		catch (Exception e)
		{
			warnings.Add($"Audio-External.xml could not be read: {e.Message}");
			return [];
		}
	}
}

public class BodyAsset
{
	public int Id { get; init; }
	public string Name { get; init; } = String.Empty;
	public string Texture { get; init; } = String.Empty;
	public Int32Rect? Source { get; init; }

	private readonly string _texturePath = String.Empty;
	private BitmapSource? _image;
	private bool _imageLoaded;

	public string Label => String.IsNullOrEmpty(Name) ? Id.ToString() : $"{Id}  {Name}";

	/// <summary>The body's first frame, cropped from its texture. Loaded the first time it is shown.</summary>
	public BitmapSource? Image
	{
		get
		{
			if (_imageLoaded)
				return _image;

			_imageLoaded = true;

			try
			{
				if (!File.Exists(_texturePath))
					return null;

				var bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.UriSource = new Uri(_texturePath);
				bitmap.EndInit();

				BitmapSource image = bitmap;

				if (Source is Int32Rect source && source.X + source.Width <= bitmap.PixelWidth && source.Y + source.Height <= bitmap.PixelHeight)
					image = new CroppedBitmap(bitmap, source);

				image.Freeze();
				_image = image;
			}
			catch
			{
				_image = null;
			}

			return _image;
		}
	}

	private BodyAsset(string texturePath)
	{
		_texturePath = texturePath;
	}

	public static BodyAsset? FromElement(XElement element, string contentPath)
	{
		if (!Int32.TryParse((string?)element.Attribute("id"), out var id))
			return null;

		var texture = ((string?)element.Element("texture") ?? String.Empty).Trim();
		var texturePath = Path.Combine(contentPath, texture.Replace('\\', Path.DirectorySeparatorChar) + ".png");

		Int32Rect? source = null;
		var sourceText = ((string?)element.Element("source") ?? String.Empty).Trim('(', ')', ' ');
		var parts = sourceText.Split(',').Select(p => Int32.TryParse(p.Trim(), out var v) ? v : -1).ToArray();

		if (parts.Length == 4 && parts.All(p => p >= 0))
			source = new Int32Rect(parts[0], parts[1], parts[2], parts[3]);

		// the body's name is kept in the comment just above it
		var comment = element.NodesBeforeSelf().LastOrDefault(n => n is not XText) as XComment;

		return new BodyAsset(texturePath)
		{
			Id = id,
			Name = comment?.Value.Trim() ?? String.Empty,
			Texture = texture,
			Source = source,
		};
	}

	/// <summary>Draws the body multiplied by a hue, the way a tinted sprite is drawn in game.</summary>
	public BitmapSource? Tint(Color hue)
	{
		var image = Image;

		if (image is null)
			return null;

		var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
		var stride = converted.PixelWidth * 4;
		var pixels = new byte[stride * converted.PixelHeight];

		converted.CopyPixels(pixels, stride, 0);

		for (var i = 0; i < pixels.Length; i += 4)
		{
			pixels[i] = (byte)(pixels[i] * hue.B / 255);
			pixels[i + 1] = (byte)(pixels[i + 1] * hue.G / 255);
			pixels[i + 2] = (byte)(pixels[i + 2] * hue.R / 255);
			pixels[i + 3] = (byte)(pixels[i + 3] * hue.A / 255);
		}

		var tinted = BitmapSource.Create(converted.PixelWidth, converted.PixelHeight, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
		tinted.Freeze();

		return tinted;
	}
}

public class SoundAsset
{
	public int Id { get; init; }
	public string Name { get; init; } = String.Empty;
	public string Category { get; init; } = String.Empty;
	public string? FilePath { get; init; }

	public string Label => $"{Id}  {Name}";

	private static readonly string[] _categories = ["Unknown", "General", "Combat", "Spell", "Entity", "UI"];
	private static readonly string[] _extensions = [".wav", ".ogg", ".mp3"];

	public static SoundAsset FromElement(XElement element, string contentPath)
	{
		var content = ((string?)element.Attribute("content") ?? String.Empty).Replace('\\', Path.DirectorySeparatorChar);
		var basePath = Path.Combine(contentPath, content);

		Int32.TryParse((string?)element.Attribute("id"), out var id);
		Int32.TryParse((string?)element.Attribute("category"), out var category);

		return new SoundAsset
		{
			Id = id,
			Name = (string?)element.Attribute("name") ?? String.Empty,
			Category = category >= 0 && category < _categories.Length ? _categories[category] : "Unknown",
			FilePath = _extensions.Select(e => basePath + e).FirstOrDefault(File.Exists),
		};
	}
}
