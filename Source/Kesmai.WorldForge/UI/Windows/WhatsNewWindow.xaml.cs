using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kesmai.WorldForge.UI.Windows
{
    /// <summary>
    /// Interaction logic for WhatsNew.xaml
    /// </summary>
    public partial class WhatsNew : Window
    {
        public WhatsNew()
        {
            InitializeComponent();
            String Changelog = 
$@" 
<html>
<body>
<h3>=== {Core.Version} ===</h3>
<strong>Enhancements:</strong>
<ul>
<li>Added the ability to add comments to components. Tiles with comments can be highlighted from the Visibility Options toolbar.</li>
</ul>
<br/>
<h3>=== 0.91.0.0 ===</h3>
<strong>Enhancements:</strong>
<ul>
<li>Added a visibility option to show walls and doors as their destroyed images.</li>
<li>Added a Flags field to Entities. Visible on the Entity tab as well as the grid view on a Spawner. Shows statuses and attributes like canFly, canLoot, NightVisionStatus, Poison and Prone. Can only show values if they are explicitly specified in the entity OnSpawn() script.</li>
</ul>
<br/>
<h3>=== 0.89.0.0 ===</h3>
<strong>Enhancements:</strong>
<ul>
<li>When saving a segment, all script sections are checked for correct syntax. Mismatched brackets or missing semicolons should be called out here. If prompted that there are syntax errors, choosing not to continue will abort the save action and jump to the script with an issue.</li>
</ul>
<br/>
<strong>Fixes:</strong>
<ul>
<li>Removed a crash when selecting a treeComponent if the data sent from the server does not define a live\dead sprite pair</li>
<li>Removed a crash when saving a segment while a previous save was in-progress.</li>
</ul>
</br>
<h3>=== 0.87.0.0 ===</h3>
<strong>Enhancements:</strong>
<ul>
<li>Added a Help Menu with a ""What's New"" window and a link to the WoldForge article on the wiki.</li>
<li>Added a calculated 'Threat' metric to Entities. This (m,r) notation shows skill levels in melee and ranged damage respectively. Threat does not take into account the weapon used or any custom damage ranges, only skill. Magic skill levels are doubled. This threat metric, as well as HP and XP is now presented in the datagrid for Spawners.</li>
<li>Component Editor has two new buttons for reordering components. This generally has no effect on gameplay, but certain components can layer.</li>
<li>Component Editor supports Ctrl-C and Ctrl-V for copying and pasting individual components. This and other Copy\Paste functions below use the clipboard to store XML.</li>
<li>Spawners, Entities, Subregions, Locations and Treasures all have export, clone, and import buttons where the 'add' and 'delete' buttons are.</li>
<li>Upgraded Right-Click context menu and streamlined selection of tiles. You can now right click a selection and get options to turn it into a new spawner or subregion, or add the selection as an inclusion or exclusion to the currently selection Region Spawner.</li>
<li>Added function to specify the destination of a teleporter component. From the Component Editor, click the 'Select Destination' button, then right-click on a tile and select 'Set as Teleporter Destination..'</li>
</ul>
<br/>
<strong>Fixes:</strong>
<ul>
<li>Removed a crash when using the Jump-to-Spawn or Jump-to-Entity functions after closing and opening segments.</li>
</ul>
</br>
<h3>=== 0.86.0.0 ===</h3>
<strong>Enhancements:</strong>
<ul>
<li>Paint tool now behaves like Draw tool in that a painted floor-type component will overwrite other floor - type components.Append instead with Shift. Paint ONLY the selected component and clobber all existing components with ALT.</li>
<li>When using the delete hotkey, Only currently visible components are deleted from the selected tile.Filter to floors to remove floor - type components, etc.</li>
<li>When pasting from the clipboard, the behavior is changed if the target selection is more than 1x1.Old behavior, or if the target selection is 1x1: whole clipboard is pasted in starting with the top left tile as the current selection. New behavior if the target selection is larger than 1x1: the clipboard is tiled horizontally and vertically to fill the selected space. Clipboard tiles that extend beyond the selection are not applied.</li>
<li>Tile Component Editor(double click a tile) now has a delete action button to remove a specific component without removing all other components.</li>
<li>Counter property editor now reads and writes accessDirection. This is a bit of a workaround.Only the LAST entry selected will be saved to the mapproj.The editor does not show that previous values are cleared unless you close the component editor or navigate to another component on the tile and back.</li>
<li>When editing components, changes to color, static IDs and other properties are periodically rendered on the world screen.Previously, the editor had to be closed and the world moved to trigger a new render.</li>
<li>Using the Ctrl-T hotkey to jump to the Treasures tab will now jump directly to a Treasure definition if coming from the Entities tab if the currently selected text matches a Treasure name.</li>
<li>When jumping to Entities, Spawns or Treasures, the list will scroll to bring the destination entry into view.</li>
<li>Treasures Document has a listing showing all Entities using that Treasure name.This is not code interpretation, but simple string matching. An Entity with a comment containing the Treasure name will still appear in the list.</li>
<li>On the Entities, Spawns and Treasures documents, the lists of related objects have a right - click context menu that will jump directly to that record in the related tab.</li>
</ul>
<br/>
<strong>Fixes:</strong>
<ul>
<li>Lists for Entities, Spawns, Locations, Subregions, Treasures all now correctly receive mouse scroll input</li>
<li>A few sprites(ones that were not 100x100 and had offsets, will now render correctly when zooming in and out.Thanks Midgit.</li>
</ul>
</body>
";
            Content.NavigateToString(Changelog);
        }
    }
}
