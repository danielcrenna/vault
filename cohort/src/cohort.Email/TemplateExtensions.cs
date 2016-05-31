using DotLiquid;

namespace cohort.Email
{
    internal static class TemplateExtensions
    {
        public static string Render(this Template template, dynamic source)
        {
            var hash = DotLiquidHashExtensions.FromDynamic(source);
            return template.Render(hash);
        }
    }
}