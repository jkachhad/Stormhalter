using System;

namespace Kesmai.Prototype.RoslynWorkspace;

public class InMemorySource
{
    public string Name { get; }

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value)
                return;

            _text = value;
            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? TextChanged;

    public InMemorySource(string name, string text)
    {
        Name = name;
        _text = text;
    }
}
