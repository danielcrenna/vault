namespace ebnf.Tokens
{
    public abstract class Token
    {
        public int Line { get; set; }
        public int Position { get; set; }
        public string Value { get; set; }
    }
}