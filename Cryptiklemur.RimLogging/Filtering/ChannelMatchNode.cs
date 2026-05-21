namespace Cryptiklemur.RimLogging.Filtering;

public sealed class ChannelMatchNode : AstNode
{
    public bool Negated;
    public string Pattern = "";
}
