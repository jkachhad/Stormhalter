namespace Kesmai.WorldForge.Roslyn;

public abstract class EntityScript
{
    protected EntityScript(string body)
    {
        Body = body;
    }

    protected abstract string MethodSignature { get; }

    public string Body { get; set; }

    public string ToDocumentText()
    {
        return $"{MethodSignature}\n{{\n\t{Body}\n}}";
    }
}

public class OnDeathScript : EntityScript
{
    public OnDeathScript()
        : base("killer.SendMessage(\"You have killed {0}.\", this.Name);")
    {
    }

    protected override string MethodSignature =>
        "public void OnDeath(MobileEntity source, MobileEntity killer)";
}

public class OnIncomingPlayerScript : EntityScript
{
    public OnIncomingPlayerScript()
        : base("player.SendMessage(\"Welcome to the game, {0}!\", player.Name);")
    {
    }

    protected override string MethodSignature =>
        "public void OnIncomingPlayer(MobileEntity source, MobileEntity player)";
}

