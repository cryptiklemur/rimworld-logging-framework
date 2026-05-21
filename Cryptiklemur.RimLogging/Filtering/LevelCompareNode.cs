namespace Cryptiklemur.RimLogging.Filtering;

public sealed class LevelCompareNode : AstNode
{
    public TokenKind Op;
    public LogLevel RightValue;
}
