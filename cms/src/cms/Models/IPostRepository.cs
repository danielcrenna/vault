using System.Collections.Generic;

namespace cms.Models
{
    public interface IPostRepository
    {
        IEnumerable<Post> GetPosts();
        Post GetPostBySlug(string slug);
    }
}