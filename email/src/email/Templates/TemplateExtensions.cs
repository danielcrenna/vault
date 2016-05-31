using DotLiquid;

namespace email.Templates
{
    internal static class TemplateExtensions
    {
        public static string Render(this Template template, dynamic source)
        {
            var hash = HashExtensions.FromDynamic(source);
            return template.Render(hash);
        }
    }
}