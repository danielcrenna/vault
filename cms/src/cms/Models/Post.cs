using System;
using TableDescriptor;

namespace cms.Models
{
    public class Post
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Synopsis { get; set; }
        public string Body { get; set; }
        [Computed] public DateTime CreatedAt { get; set; }
    }
}
