using Microsoft.AspNetCore.Mvc;
using Poster.Models;
using System.Diagnostics;

namespace Poster.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        Context _context;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = new Context();
        }

         IActionResult Index()
    {

            return RedirectToAction("ShowListPosts");
    }
         public IActionResult ShowListPosts([FromQuery(Name = "page")]int CurrentPage)
        {
            int PageSize = 10;
            var post = _context.Posts.ToList();
            int TotalPost = post.Count(); // tổng số bài
            int CountPages = (int)Math.Ceiling((double)TotalPost / PageSize);
            if (CurrentPage>CountPages)
            {
                CurrentPage = CountPages;
            }
            if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }
            var Paging = new Paging()
            {
                CurrentPage = CurrentPage,
                CountPage = CountPages,
                GeneralUrl = (pageNumber)=>Url.Action("ShowListPosts", new
                {
                    page = pageNumber,
                })
            };
            ViewBag.Paging = Paging;
            ViewBag.TotalPost = TotalPost;
            var PostInPage = post.Skip((CurrentPage-1)*PageSize).Take(10).ToList();
            return View(PostInPage);
        }

        public IActionResult Them()
        {
            for (int i = 0; i < 500; i++)
            {
                Posts p = new Posts()
                {
                    Content = "Content thứ: " + i,
                    Title = "Bài viết số : " +i,
                    Published = true
                };
                _context.Posts.Add(p);
                _context.SaveChanges();
            }
           return RedirectToAction("ShowListPosts");
        }
        public IActionResult Xoa()
        {
            var post = _context.Posts.Take(500).ToList();
            foreach (var item in post)
            {
                _context.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("ShowListPosts");
        }
        public IActionResult CreatePosts()
        {
            return View();
        }
        public IActionResult Details(Guid id)
        {
            var post = _context.Posts.FirstOrDefault(c => c.ID == id);
            return View(post);
        }
        [HttpPost]
        public IActionResult CreatePosts(Posts post)
        {
            var posts = new Posts()
            {
                Title = post.Title,
                Content = post.Content,
                Published = true
            };
            _context.Add(posts);
            _context.SaveChanges();
            return RedirectToAction("ShowListPosts");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}