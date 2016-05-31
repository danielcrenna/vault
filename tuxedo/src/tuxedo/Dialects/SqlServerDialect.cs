namespace tuxedo.Dialects
{
    public class SqlServerDialect : Dialect
    {
        public char StartIdentifier { get { return '['; } }
        public char EndIdentifier { get { return ']'; } }
        public char Separator { get { return '.'; } }
        public int ParametersPerQuery { get { return 500; } }
        public string Identity { get { return "SELECT CAST(SCOPE_IDENTITY() AS INT) AS [Id]"; } }
    }
}