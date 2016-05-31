using System.Web;
using System.Web.Mvc;
using MarkdownSharp;

namespace cms
{
    public static class HtmlExtensions
    {
        private static readonly Markdown _markdown;

        static HtmlExtensions()
        {
            _markdown = new Markdown();
        }

        public static IHtmlString Markdown(this HtmlHelper helper, string text)
        {
            var html = _markdown.Transform(text);
            return MvcHtmlString.Create(html);
        }
    }
}