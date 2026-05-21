namespace Cryptiklemur.RimLogging.Filtering;

public readonly struct Token
{
    public readonly TokenKind Kind;
    public readonly string Text;
    public readonly int Pos;

    public Token(TokenKind k, string t, int p)
    {
        Kind = k;
        Text = t;
        Pos = p;
    }
}
