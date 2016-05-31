using System.Collections.Generic;
using cms.Models;

namespace cms.example.ViewModels
{
    public class PostListViewModel
    {
        public IEnumerable<Post> Posts { get; private set; } 

        public PostListViewModel(IEnumerable<Post> posts)
        {
            Posts = posts;
        }
    }
}