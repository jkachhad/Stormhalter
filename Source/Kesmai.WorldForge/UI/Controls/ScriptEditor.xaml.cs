using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using Kesmai.WorldForge.Editor.Scripting;

namespace Kesmai.WorldForge.UI.Controls;

public partial class ScriptEditor : UserControl
{
    public ScriptEditor()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => PART_Editor.Text;
        set => PART_Editor.Text = value;
    }

    public void Bind(ScriptDocument document)
    {
        Text = document.Text;
    }
}
