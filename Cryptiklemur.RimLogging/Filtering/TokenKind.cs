namespace Cryptiklemur.RimLogging.Filtering;

public enum TokenKind
{
    LevelIdent,
    ChannelIdent,
    And,
    Or,
    Not,
    LParen,
    RParen,
    OpEq,
    OpNeq,
    OpLt,
    OpLte,
    OpGt,
    OpGte,
    LevelLiteral,
    StringLiteral,
    End,
}
