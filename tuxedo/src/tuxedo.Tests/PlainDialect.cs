namespace tuxedo.Tests
{
    public class PlainDialect : Dialect
    {
        public char StartIdentifier { get { return ' '; } }
        public char EndIdentifier { get { return ' '; } }
        public char Separator { get { return ' '; } }
        public int ParametersPerQuery { get { return 10; } }
        public string Identity { get { return "IDENTITY"; } }
    }
}