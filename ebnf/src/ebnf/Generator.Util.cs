using System.Text.RegularExpressions;

namespace ebnf
{
    partial class Generator
    {
        private const string UnderscoresPattern = "(?:^|_)(.)";
        private const string CamelCasePattern = "(((?<=[a-z])[A-Z])|([A-Z](?![A-Z]|$)))";

        public static string Capitalize(string input)
        {
            var lower = input.ToLowerInvariant();
            return char.ToUpperInvariant(lower[0]) + lower.Substring(1);
        }

        public static string TransformGrammarName(string input)
        {
            input = input.Replace("#", "Sharp").Replace("+", "Plus");
            input = Underscore(input);
            var pascalized = Regex.Replace(input, UnderscoresPattern, match => match.Groups[1].Value.ToUpper(), RegexOptions.Compiled);
            return pascalized;
        }

        public static string Underscore(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            var underscored = Regex.Replace(input, CamelCasePattern, m => string.Concat("_", m.Value.ToLowerInvariant()), RegexOptions.Compiled);
            return underscored.StartsWith("_") ? underscored.Substring(1) : underscored;
        }	
    }
}
