using System;
using System.Collections.Generic;

namespace CryptikLemur.RimLogging.Filtering;

/// <summary>
/// Converts filter expression source text into a list of <see cref="Token"/> values.
/// </summary>
internal static class Lexer
{
    /// <summary>
    /// Tokenizes the input, recognizing parentheses, operators, double-quoted strings, and
    /// keyword/level identifiers, and appends a trailing <see cref="TokenKind.End"/> token.
    /// </summary>
    /// <param name="input">The filter expression source text.</param>
    /// <returns>The list of tokens, ending with an <see cref="TokenKind.End"/> token.</returns>
    /// <exception cref="FormatException">
    /// Thrown for an unterminated string, an unexpected character, or an unknown identifier.
    /// </exception>
    public static List<Token> Tokenize(string input)
    {
        List<Token> tokens = new();
        int i = 0;
        while (i < input.Length)
        {
            char c = input[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (c == '(') { tokens.Add(new Token(TokenKind.LParen, "(", i)); i++; continue; }
            if (c == ')') { tokens.Add(new Token(TokenKind.RParen, ")", i)); i++; continue; }
            if (c == '"')
            {
                int end = input.IndexOf('"', i + 1);
                if (end < 0) throw new FormatException($"Unterminated string at {i}");
                tokens.Add(new Token(TokenKind.StringLiteral, input.Substring(i + 1, end - i - 1), i));
                i = end + 1; continue;
            }
            if (c == '=') { tokens.Add(new Token(TokenKind.OpEq, "=", i)); i++; continue; }
            if (c == '!' && i + 1 < input.Length && input[i + 1] == '=')
                { tokens.Add(new Token(TokenKind.OpNeq, "!=", i)); i += 2; continue; }
            if (c == '<' && i + 1 < input.Length && input[i + 1] == '=')
                { tokens.Add(new Token(TokenKind.OpLte, "<=", i)); i += 2; continue; }
            if (c == '>' && i + 1 < input.Length && input[i + 1] == '=')
                { tokens.Add(new Token(TokenKind.OpGte, ">=", i)); i += 2; continue; }
            if (c == '<') { tokens.Add(new Token(TokenKind.OpLt, "<", i)); i++; continue; }
            if (c == '>') { tokens.Add(new Token(TokenKind.OpGt, ">", i)); i++; continue; }
            if (char.IsLetter(c))
            {
                int start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;
                string word = input.Substring(start, i - start);
                tokens.Add(ClassifyIdent(word, start));
                continue;
            }
            throw new FormatException($"Unexpected character '{c}' at {i}");
        }
        tokens.Add(new Token(TokenKind.End, "", input.Length));
        return tokens;
    }

    private static Token ClassifyIdent(string word, int pos)
    {
        string lower = word.ToLowerInvariant();
        return lower switch
        {
            "and" => new Token(TokenKind.And, word, pos),
            "or"  => new Token(TokenKind.Or, word, pos),
            "not" => new Token(TokenKind.Not, word, pos),
            "level"   => new Token(TokenKind.LevelIdent, word, pos),
            "channel" => new Token(TokenKind.ChannelIdent, word, pos),
            "trace" or "debug" or "info" or "warn" or "error" or "fatal"
                  => new Token(TokenKind.LevelLiteral, word, pos),
            _ => throw new FormatException($"Unknown identifier '{word}' at {pos}"),
        };
    }
}
