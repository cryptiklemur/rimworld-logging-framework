namespace Cryptiklemur.RimLogging.Filtering;

public sealed class AndNode : AstNode
{
    public AstNode Left = null!;
    public AstNode Right = null!;
}
