namespace Hammock.Web
{
    public class WebPair
    {
        public WebPair(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Value { get; set; }
        public string Name { get; private set; }
    }
}