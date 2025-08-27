using System.Collections.Generic;

namespace Kesmai.WorldForge.Editor.Scripting;

public class ScriptHost
{
    private readonly Dictionary<string, ScriptDocument> _scripts = new();

    public ScriptDocument? this[string name] => _scripts.TryGetValue(name, out var doc) ? doc : null;

    public ScriptDocument SetScript(string name, string text, string? filePath = null)
    {
        var doc = ScriptWorkspace.Instance.AddOrUpdate(name, text, filePath);
        _scripts[name] = doc;
        return doc;
    }

    public IEnumerable<KeyValuePair<string, ScriptDocument>> Scripts => _scripts;
}
