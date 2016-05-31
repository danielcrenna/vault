using cms.Models;

namespace cms.example.ViewModels
{
    public class PostViewModel
    {
        public Post Post { get; private set; }

        public PostViewModel(Post post)
        {
            Post = post;
        }
    }
}