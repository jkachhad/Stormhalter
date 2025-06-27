using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Input;
using DigitalRune.Storages;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Editor.MVP.Models.World.Components;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public partial class ComponentsPanel : UserControl
{
    public static ComponentsPanel Instance { get; private set; }
    private ObservableCollection<ComponentsCategory> _baseCategories;
    private Dictionary<string, ComponentsCategory> _categoryCache;
    private ObservableCollection<ComponentsCategory> _allCategories;




    public ComponentsPanel ( )
    {
        InitializeComponent ( );
        _categories.ContextMenu = null; // prevent base menu from inheriting invalid context

        Instance = this;
        Loaded += ( _, __ ) => Reload ( );
        IsVisibleChanged += OnVisibilityChanged;
    }

    private ComponentsCategory _lastSelectedCategory;
    public async void Reload ( )
    {
        // Store currently selected category (if any)
        var previousCategory = _categories.SelectedItem as ComponentsCategory;
        var previousCategoryName = previousCategory?.Name;

        // Load prefab category fresh (others are cached)
        var prefabCategory = await Task.Run ( ( ) => BuildPrefabCategory ( ) );

        Dispatcher.Invoke ( ( ) =>
        {
            var categories = new ObservableCollection<ComponentsCategory> ( _categoryCache.Values );
            categories.Add ( prefabCategory );
            _categories.ItemsSource = categories;

            // Restore previous selection if still present
            var restored = categories.FirstOrDefault ( c => c.Name == previousCategoryName );
            if ( restored != null )
            {
                _categories.SelectedItem = restored;
            }
            else if ( _categories.Items.Count > 0 )
            {
                _categories.SelectedIndex = 0;
            }
        } );
    }



    private void OnVisibilityChanged ( object sender, DependencyPropertyChangedEventArgs e )
    {
        if ( Visibility == Visibility.Visible )
        {
            // Restore the last selected category
            if ( _lastSelectedCategory != null && _allCategories.Contains ( _lastSelectedCategory ) )
            {
                _categories.SelectedItem = _lastSelectedCategory;
            }
        }
        else
        {
            // Save the currently selected category
            _lastSelectedCategory = _categories.SelectedItem as ComponentsCategory;
        }
    }

    public override void EndInit ( )
    {
        base.EndInit ( );
        _categoryCache = BuildBaseComponentCategories ( )
        .ToDictionary ( cat => cat.Name, cat => cat );
    }

    private void CategoryItem_Loaded ( object sender, RoutedEventArgs e )
    {
        Debug.WriteLine ( $"[ComboBox] Item rendered at {DateTime.Now:hh:mm:ss.fff}" );
    }

    private ObservableCollection<ComponentsCategory> BuildBaseComponentCategories ( )
    {
        var collection = new ObservableCollection<ComponentsCategory> ( );

        // STATIC CATEGORY
        var staticCategory = LoadStaticCategory ( );
        collection.Add ( staticCategory );

        // XML-based component categories
        foreach ( var category in LoadComponentXmlCategories ( ) )
            collection.Add ( category );

        return collection;
    }

    private ComponentsCategory _cachedStaticCategory;

    private ComponentsCategory LoadStaticCategory ( )
    {
        if ( _cachedStaticCategory != null )
            return _cachedStaticCategory; // Return cached category if already loaded

        var sw = Stopwatch.StartNew ( );
        var services = ServiceLocator.Current;
        var storage = services.GetInstance<IStorage> ( );

        var staticCategory = new ComponentsCategory ( )
        {
            Name = "Static",
        };

        var terrainDocument = XDocument.Load ( storage.OpenFile ( @"Data\Terrain.xml" ) );
        var terrainRootElement = terrainDocument.Root;

        if ( terrainRootElement != null )
        {
            foreach ( var terrainElement in terrainRootElement.Elements ( "terrain" ) )
            {
                var idAttribute = terrainElement.Attribute ( "id" );

                if ( idAttribute != null )
                {
                    staticCategory.Components.Add ( new StaticComponent ( (int) idAttribute )
                    {
                        Name = idAttribute.Value
                    } );
                }
            }
        }

        terrainDocument = XDocument.Load ( storage.OpenFile ( @"Data\Terrain-External.xml" ) );
        var customTerrainPath = $@"{Core.CustomArtPath}\Data\Terrain-External.xml";
        if ( File.Exists ( customTerrainPath ) )
        {
            try
            {
                terrainDocument = XDocument.Load ( customTerrainPath );
            }
            catch
            {
                // Handle errors gracefully
            }
        }
        terrainRootElement = terrainDocument.Root;
        if ( terrainRootElement != null )
        {
            foreach ( var terrainElement in terrainRootElement.Elements ( "terrain" ) )
            {
                var idAttribute = terrainElement.Attribute ( "id" );

                if ( idAttribute != null && int.TryParse ( idAttribute.Value, out var id ) )
                {
                    staticCategory.Components.Add ( new StaticComponent ( id )
                    {
                        Name = idAttribute.Value
                    } );
                }
            }
        }

        sw.Stop ( );
        Debug.WriteLine ( $"[LoadStaticCategory] Built in {sw.ElapsedMilliseconds} ms" );

        _cachedStaticCategory = staticCategory; // Cache the result
        return staticCategory;
    }

    private IEnumerable<ComponentsCategory> LoadComponentXmlCategories ( )
    {

        var assembly = Assembly.GetExecutingAssembly ( );
        var document = Core.ComponentsResource;
        var collection = new ObservableCollection<ComponentsCategory> ( );

        // your Core.ComponentsResource + components.xml logic here
        var customComponentsPath = $@"{Core.CustomArtPath}\WorldForge\components.xml";
        if ( File.Exists ( customComponentsPath ) )
        {
            try
            {
                document = XDocument.Load ( customComponentsPath );
                document.Save ( customComponentsPath ); // pretty prints the document, should help keep tabs consistent and avoid ugly git commits
            }
            catch ( Exception e )
            {
                System.Windows.MessageBox.Show ( e.Message, "Custom Components.xml failed to load", System.Windows.MessageBoxButton.OK );
            }
        }

        if ( document != null )
        {
            var componentsRootElement = document.Root;

            if ( componentsRootElement != null )
            {
                foreach ( var categoryElement in componentsRootElement.Elements ( "category" ) )
                {
                    var category = new ComponentsCategory ( )
                    {
                        Name = (string) categoryElement.Attribute ( "name" ),
                    };

                    foreach ( var element in categoryElement.Elements ( ) )
                    {
                        try
                        {
                            var componentTypename = $"Kesmai.WorldForge.Models.{element.Name}";
                            var componentType = assembly.GetType ( componentTypename, true );

                            if ( Activator.CreateInstance ( componentType, element ) is TerrainComponent component )
                                category.Components.Add ( component );
                        }
                        catch ( Exception e )
                        {
                            System.Windows.MessageBox.Show ( $"Component failed to parse:\n${element}\n{e.Message}", "Custom Components.xml failed to load", System.Windows.MessageBoxButton.OK );
                        }
                    }

                    collection.Add ( category );
                }
            }
        }
        return collection;
    }


    private ComponentsCategory BuildPrefabCategory ( )
    {
        var services = ServiceLocator.Current;
        var storage = services.GetInstance<IStorage> ( );
        var assembly = Assembly.GetExecutingAssembly ( );


        /* Other Categories */
        // If a custom components.xml exists, load that instead.

        // Load Prefab Tile Components
        var prefabCategory = new ComponentsCategory
        {
            Name = "Prefabs"
        };

        try
        {
            var prefabPath = $@"{Core.CustomArtPath}\Data\TilePrefabs.xml";
            XDocument prefabDoc;
            if ( File.Exists ( prefabPath ) )
            {
                prefabDoc = XDocument.Load ( prefabPath );
            }
            else
            {
                // Fallback to embedded if custom not available
                using var stream = storage.OpenFile ( @"Data\TilePrefabs.xml" );
                prefabDoc = XDocument.Load ( stream );
            }
            var prefabRoot = prefabDoc.Root;
            if ( prefabRoot != null )
            {
                foreach ( var prefabElement in prefabRoot.Elements ( "prefab" ) )
                {
                    var nameAttr = prefabElement.Attribute ( "name" );
                    if ( nameAttr == null )
                        continue;

                    var prefab = new TilePrefabComponent
                    {
                        Name = nameAttr.Value
                    };
                    var typeCache = assembly.GetTypes ( )
    .Where ( t => typeof ( TerrainComponent ).IsAssignableFrom ( t ) )
    .ToDictionary ( t => t.Name, t => t );

                    foreach ( var child in prefabElement.Elements ( ) )
                    {
                        try
                        {
                            if ( child.Name.LocalName == "component" && child.Attribute ( "type" ) is { } typeAttr )
                            {
                                var typeName = typeAttr.Value;
                                var fullTypeName = $"Kesmai.WorldForge.Models.{typeName}";

                                typeCache.TryGetValue ( typeName, out var componentType );


                                if ( componentType != null && Activator.CreateInstance ( componentType, child ) is TerrainComponent subComponent )
                                {
                                    prefab.Components.Add ( subComponent );
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine ( $"Activator returned null or non-TerrainComponent for {typeName}" );
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine ( $"Skipped element: <{child.Name}> (missing or invalid 'type' attribute)" );
                            }
                        }
                        catch ( Exception e )
                        {
                            System.Diagnostics.Debug.WriteLine ( $"Failed to load prefab component: {e.Message}" );
                        }
                    }

                    // Create a synthetic tile for preview rendering
                    var previewTile = new SegmentTile ( 0, 0 );
                    foreach ( var comp in prefab.Components )
                        previewTile.Components.Add ( comp );

                    // Store the tile in the prefab for later preview rendering
                    prefab.PreviewTile = previewTile;


                    prefabCategory.Components.Add ( prefab );

                }
            }


        }
        catch ( Exception e )
        {
           // System.Windows.MessageBox.Show ( $"Failed to load prefab components:\n{e.Message}", "TilePrefabs.xml error", System.Windows.MessageBoxButton.OK );
        }


        return prefabCategory;
    }

    public System.Windows.Input.ICommand DeletePrefabCommand => new RelayCommand<TerrainComponent> ( OnDeletePrefab );

    private void OnDeletePrefab ( TerrainComponent component )
    {
        System.Diagnostics.Debug.WriteLine ( $"[DeletePrefabCommand] Invoked with: {component?.Name} ({component?.GetType ( ).Name})" );

        if ( component is not TilePrefabComponent prefab )
        {
            System.Diagnostics.Debug.WriteLine ( "[DeletePrefabCommand] Not a prefab. Ignoring." );
            return;
        }

        var result = MessageBox.Show (
            $"Delete prefab \"{prefab.Name}\"?",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning );

        if ( result != MessageBoxResult.Yes )
        {
            System.Diagnostics.Debug.WriteLine ( "[DeletePrefabCommand] Deletion cancelled." );
            return;
        }

        DeletePrefabByName ( prefab.Name );
        Reload ( );
    }


    private void DeletePrefabByName ( string prefabName )
    {
        var prefabPath = System.IO.Path.Combine ( Core.CustomArtPath, "Data", "TilePrefabs.xml" );
        if ( !File.Exists ( prefabPath ) )
            return;

        var doc = XDocument.Load ( prefabPath );
        var root = doc.Root;
        if ( root == null )
            return;

        var prefabElement = root.Elements ( "prefab" )
            .FirstOrDefault ( p => (string) p.Attribute ( "name" ) == prefabName );

        if ( prefabElement != null )
        {
            prefabElement.Remove ( );
            doc.Save ( prefabPath );
            System.Diagnostics.Debug.WriteLine ( $"[ComponentsPanel] Deleted prefab: {prefabName}" );
        }
    }


}