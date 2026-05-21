using System;

namespace Cryptiklemur.RimLogging.Filtering;

public sealed class FilterExpression
{
    private readonly AstNode _ast;
    private readonly Func<LogEntry, bool> _predicate;

    public string Source { get; }

    private FilterExpression(string source, AstNode ast)
    {
        Source = source;
        _ast = ast;
        _predicate = Compiler.Compile(ast);
    }

    public static FilterExpression Parse(string input)
        => new FilterExpression(input, Parser.Parse(input));

    public bool Match(LogEntry entry) => _predicate(entry);

    public override string ToString() => Stringify(_ast);

    private static string Stringify(AstNode n) => n switch
    {
        AndNode a  => "(" + Stringify(a.Left) + " AND " + Stringify(a.Right) + ")",
        OrNode o   => "(" + Stringify(o.Left) + " OR " + Stringify(o.Right) + ")",
        NotNode nt => "NOT " + Stringify(nt.Operand),
        LevelCompareNode lc => "level " + OpToString(lc.Op) + " " + lc.RightValue,
        ChannelMatchNode cm => "channel " + (cm.Negated ? "!= " : "= ") + "\"" + cm.Pattern + "\"",
        _ => "<unknown>",
    };

    private static string OpToString(TokenKind op) => op switch
    {
        TokenKind.OpEq  => "=",
        TokenKind.OpNeq => "!=",
        TokenKind.OpLt  => "<",
        TokenKind.OpLte => "<=",
        TokenKind.OpGt  => ">",
        TokenKind.OpGte => ">=",
        _ => "?",
    };
}
