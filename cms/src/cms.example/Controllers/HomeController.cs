using System.Web.Mvc;
using cms.Models;
using cms.example.ViewModels;

namespace cms.example.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostRepository _postRepository;

        public HomeController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public ActionResult Index()
        {
            var posts = _postRepository.GetPosts();
            return View(new PostListViewModel(posts));
        }

        public ActionResult Post(string id)
        {
            var post = _postRepository.GetPostBySlug(id);
            if (post == null)
            {
                return RedirectToRoute("Error");
            }
            return View(new PostViewModel(post));
        }
    }
}
