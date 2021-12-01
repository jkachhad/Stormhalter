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
            Content.Text = 
$@" 
=== {Core.Version} ===
Enhancements:

*Added a Help Menu with a ""What's New"" window and a link to the WoldForge article on the wiki.
*Added a calculated 'Threat' metric to Entities. This (m,r) notation shows skill levels in melee and ranged damage respectively. Magic skill levels are doubled. This threat metric, as well as HP and XP is now presented in the datagrid for Spawners.
*Component Editor has two new buttons for reordering components. This generally has no effect on gameplay, but certain components can layer.
*Component Editor supports Ctrl-C and Ctrl-V for copying and pasting individual components. This and other Copy\Paste functions below use the clipboard to store XML.
*Spawners, Entities, Subregions, Locations and Treasures all have export, clone, and import buttons where the 'add' and 'delete' buttons are.

=== 0.86.0.0 ===
Enhancements:

*Paint tool now behaves like Draw tool in that a painted floor-type component will overwrite other floor - type components.Append instead with Shift. Paint ONLY the selected component and clobber all existing components with ALT.
*When using the delete hotkey, Only currently visible components are deleted from the selected tile.Filter to floors to remove floor - type components, etc.
*When pasting from the clipboard, the behavior is changed if the target selection is more than 1x1.Old behavior, or if the target selection is 1x1: whole clipboard is pasted in starting with the top left tile as the current selection. New behavior if the target selection is larger than 1x1: the clipboard is tiled horizontally and vertically to fill the selected space. Clipboard tiles that extend beyond the selection are not applied.
*Tile Component Editor(double click a tile) now has a delete action button to remove a specific component without removing all other components.
*Counter property editor now reads and writes accessDirection. This is a bit of a workaround.Only the LAST entry selected will be saved to the mapproj.The editor does not show that previous values are cleared unless you close the component editor or navigate to another component on the tile and back.
*When editing components, changes to color, static IDs and other properties are periodically rendered on the world screen.Previously, the editor had to be closed and the world moved to trigger a new render.
*Using the Ctrl-T hotkey to jump to the Treasures tab will now jump directly to a Treasure definition if coming from the Entities tab if the currently selected text matches a Treasure name.
*When jumping to Entities, Spawns or Treasures, the list will scroll to bring the destination entry into view.
*Treasures Document has a listing showing all Entities using that Treasure name.This is not code interpretation, but simple string matching. An Entity with a comment containing the Treasure name will still appear in the list.
*On the Entities, Spawns and Treasures documents, the lists of related objects have a right - click context menu that will jump directly to that record in the related tab.
  
Fixes:

*Lists for Entities, Spawns, Locations, Subregions, Treasures all now correctly receive mouse scroll input
*A few sprites(ones that were not 100x100 and had offsets, will now render correctly when zooming in and out.Thanks Midgit.
";

        }
    }
}
