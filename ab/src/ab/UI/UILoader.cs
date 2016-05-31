using System.IO;

namespace ab.UI
{
    internal class UILoader
    {
        public string Render(string template)
        {
            var file = ReadFromManifestResourceStream("ab.UI." + template);
            return file;
        }

        private static string ReadFromManifestResourceStream(string name)
        {
            var assembly = typeof(UILoader).Assembly;
            using (var resourceStream = assembly.GetManifestResourceStream(name))
            {
                if (resourceStream != null)
                {
                    using (var sr = new StreamReader(resourceStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            return "";
        }
    }
}
