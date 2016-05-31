namespace tuxedo.Dialects
{
    public class SqliteDialect : Dialect
    {
        public char StartIdentifier { get { return '\"'; } }
        public char EndIdentifier { get { return '\"'; } }
        public char Separator { get { return '.'; } }
        public int ParametersPerQuery { get { return 999; } }
        public string Identity { get { return "SELECT LAST_INSERT_ROWID() AS \"Id\""; } }
    }
}