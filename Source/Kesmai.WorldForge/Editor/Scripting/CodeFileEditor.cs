using System.IO;
using System.Windows;
using System.Windows.Media;
using CommonServiceLocator;
using DigitalRune.ServiceLocation;
using RoslynPad.Editor;

namespace Kesmai.WorldForge.Scripting;

public class CodeFileEditor : RoslynCodeEditor
{
    private bool _initialized;

    public string FilePath { get; private set; }

    public CodeFileEditor()
    {
        FontFamily = new FontFamily("Consolas");
        HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
        Loaded += OnLoaded;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("VSTHRD", "VSTHRD100:Avoid async void methods")]
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
            return;

        var services = (ServiceContainer)ServiceLocator.Current;
        var presenter = services.GetInstance<ApplicationPresenter>();

        await InitializeAsync(presenter.RoslynHost, new ClassificationHighlightColors(),
            Directory.GetCurrentDirectory(), string.Empty, Microsoft.CodeAnalysis.SourceCodeKind.Regular);

        _initialized = true;
    }

    public void LoadFile(string path)
    {
        FilePath = path;
        Text = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        Document.UndoStack.ClearAll();
    }

    public void SaveFile()
    {
        if (!string.IsNullOrEmpty(FilePath))
        {
            File.WriteAllText(FilePath, Text);
        }
    }
}
