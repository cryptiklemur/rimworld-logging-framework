using System;
using System.Text.RegularExpressions;

namespace Cryptiklemur.RimLogging.Filtering;

public static class WildcardMatcher
{
    public static bool Match(string pattern, string input)
    {
        if (pattern.EndsWith(".*", StringComparison.Ordinal))
        {
            string prefix = pattern.Substring(0, pattern.Length - 2);
            return input.Equals(prefix, StringComparison.Ordinal)
                || input.StartsWith(prefix + ".", StringComparison.Ordinal);
        }
        if (pattern.IndexOf('*') < 0)
            return input.Equals(pattern, StringComparison.Ordinal);
        string esc = Regex.Escape(pattern).Replace("\\*", ".*");
        return Regex.IsMatch(input, "^" + esc + "$");
    }
}
