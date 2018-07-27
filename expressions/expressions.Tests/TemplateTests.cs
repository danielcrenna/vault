using DotLiquid;
using Xunit;

namespace expressions.Tests
{
    public class TemplateTests
    {
        [Fact]
        public void Can_render_template()
        {
            string source = "Hello {{ world }}";
            Template template = Template.Parse(source);
            string target = template.Render(Hash.FromAnonymousObject(new
            {
                world = "World!"
            }));
            Assert.Equal("Hello World!", target);
        }
    }
}
