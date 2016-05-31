using System.Collections.Generic;
using System.IO;
using System.Linq;
using ServiceStack.Text;

namespace cms.Models
{
    public class FileBasedPostRepository : IPostRepository 
    {
        private readonly string _directory;

        static FileBasedPostRepository()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = JsonPropertyConvention.Lenient;
        }

        public FileBasedPostRepository(string directory)
        {
            _directory = directory;
        }

        public IEnumerable<Post> GetPosts()
        {
            var files = Directory.GetFiles(_directory, "*.json");
            var posts = new List<Post>(files.Length);
            foreach (var fileName in files)
            {
                var text = File.ReadAllText(fileName);
                var post = JsonSerializer.DeserializeFromString<Post>(text);
                posts.Add(post);
            }
            return posts;
        }

        public Post GetPostBySlug(string slug)
        {
            var json = Path.Combine(_directory, slug + ".json");
            var markdown = Path.Combine(_directory, slug + ".md");

            if (!File.Exists(json)) return null;
            var text = File.ReadAllText(json);
            var post = JsonSerializer.DeserializeFromString<Post>(text);
            post.Body = File.ReadAllText(markdown);
            return post;
        }
    }
}