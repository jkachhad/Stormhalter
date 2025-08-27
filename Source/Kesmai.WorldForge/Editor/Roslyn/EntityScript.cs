using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Roslyn;

public abstract class EntityScript : ObservableObject
{
    private string _body;
    private bool _isEnabled = true;

    protected EntityScript(string name, string body)
    {
        Name = name;
        _body = body;
    }

    public abstract string MethodSignature { get; }

    public string Name { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public string Body
    {
        get => _body;
        set => SetProperty(ref _body, value);
    }

    public string ToDocumentText()
    {
        return $"{MethodSignature}\n{{\n\t{Body}\n}}";
    }
}

public class OnDeathScript : EntityScript
{
    public OnDeathScript()
        : base("OnDeath", "killer.SendMessage(\"You have killed {0}.\", this.Name);")
    {
    }

    public override string MethodSignature =>
        "public void OnDeath(MobileEntity source, MobileEntity killer)";
}

public class OnIncomingPlayerScript : EntityScript
{
    public OnIncomingPlayerScript()
        : base("OnIncomingPlayer", "player.SendMessage(\"Welcome to the game, {0}!\", player.Name);")
    {
    }

    public override string MethodSignature =>
        "public void OnIncomingPlayer(MobileEntity source, MobileEntity player)";
}

