namespace tuxedo.Dialects
{
    public class MySqlDialect : Dialect
    {
        public char StartIdentifier { get { return '`'; } }
        public char EndIdentifier { get { return '`'; } }
        public char Separator { get { return '.'; } }
        public int ParametersPerQuery { get { return 1000; } }
        public string Identity { get { return "SELECT LAST_INSERT_ID() AS `Id`"; } }
    }
}