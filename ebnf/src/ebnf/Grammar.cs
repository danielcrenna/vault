namespace ebnf
{
    public class Grammar
    {
        public string Name { get; set; }
        public Tree Tree { get; set; }
        public Grammar(string name, string path)
        {
            Name = name;
            Tree = Parser.Parse(path);
        }
    }
}
