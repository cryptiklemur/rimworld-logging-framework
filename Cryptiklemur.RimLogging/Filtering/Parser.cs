using System;
using System.Collections.Generic;

namespace Cryptiklemur.RimLogging.Filtering;

public static class Parser
{
    public static AstNode Parse(string input)
    {
        List<Token> tokens = Lexer.Tokenize(input);
        int pos = 0;
        AstNode node = ParseOr(tokens, ref pos);
        if (tokens[pos].Kind != TokenKind.End)
            throw new FormatException($"Unexpected token '{tokens[pos].Text}' at {tokens[pos].Pos}");
        return node;
    }

    private static AstNode ParseOr(List<Token> ts, ref int p)
    {
        AstNode left = ParseAnd(ts, ref p);
        while (ts[p].Kind == TokenKind.Or)
        {
            p++;
            AstNode right = ParseAnd(ts, ref p);
            left = new OrNode { Left = left, Right = right };
        }
        return left;
    }

    private static AstNode ParseAnd(List<Token> ts, ref int p)
    {
        AstNode left = ParseNot(ts, ref p);
        while (ts[p].Kind == TokenKind.And)
        {
            p++;
            AstNode right = ParseNot(ts, ref p);
            left = new AndNode { Left = left, Right = right };
        }
        return left;
    }

    private static AstNode ParseNot(List<Token> ts, ref int p)
    {
        if (ts[p].Kind == TokenKind.Not)
        {
            p++;
            return new NotNode { Operand = ParseNot(ts, ref p) };
        }
        return ParseAtom(ts, ref p);
    }

    private static AstNode ParseAtom(List<Token> ts, ref int p)
    {
        if (ts[p].Kind == TokenKind.LParen)
        {
            p++;
            AstNode n = ParseOr(ts, ref p);
            if (ts[p].Kind != TokenKind.RParen) throw new FormatException($"Expected ')' at {ts[p].Pos}");
            p++;
            return n;
        }
        if (ts[p].Kind == TokenKind.LevelIdent)
        {
            p++;
            TokenKind op = ts[p].Kind;
            if (!IsLevelOp(op)) throw new FormatException($"Expected comparison operator at {ts[p].Pos}");
            p++;
            if (ts[p].Kind != TokenKind.LevelLiteral) throw new FormatException($"Expected level literal at {ts[p].Pos}");
            LogLevel lv = ParseLevel(ts[p].Text);
            p++;
            return new LevelCompareNode { Op = op, RightValue = lv };
        }
        if (ts[p].Kind == TokenKind.ChannelIdent)
        {
            p++;
            bool neg;
            if (ts[p].Kind == TokenKind.OpEq) neg = false;
            else if (ts[p].Kind == TokenKind.OpNeq) neg = true;
            else throw new FormatException($"Expected '=' or '!=' at {ts[p].Pos}");
            p++;
            if (ts[p].Kind != TokenKind.StringLiteral) throw new FormatException($"Expected string literal at {ts[p].Pos}");
            string pat = ts[p].Text;
            p++;
            return new ChannelMatchNode { Negated = neg, Pattern = pat };
        }
        throw new FormatException($"Unexpected token '{ts[p].Text}' at {ts[p].Pos}");
    }

    private static bool IsLevelOp(TokenKind k) => k is
        TokenKind.OpEq or TokenKind.OpNeq or TokenKind.OpLt or TokenKind.OpLte
        or TokenKind.OpGt or TokenKind.OpGte;

    private static LogLevel ParseLevel(string s) => s.ToLowerInvariant() switch
    {
        "trace" => LogLevel.Trace,
        "debug" => LogLevel.Debug,
        "info"  => LogLevel.Info,
        "warn"  => LogLevel.Warn,
        "error" => LogLevel.Error,
        "fatal" => LogLevel.Fatal,
        _ => throw new FormatException($"Unknown level '{s}'"),
    };
}
