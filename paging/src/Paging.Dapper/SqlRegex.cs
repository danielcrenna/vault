using System.Text.RegularExpressions;

namespace Paging
{
    /// <summary>
    /// Cribbed from: https://github.com/toptensoftware/PetaPoco
    /// </summary>
    internal static class SqlRegex
    {
        public static RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled;
        public static Regex SelectColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", Options);
        public static Regex OrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", Options);
        public static Regex Distinct = new Regex(@"\ADISTINCT\s", Options);
    }
}