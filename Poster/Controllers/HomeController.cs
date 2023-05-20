using Microsoft.AspNetCore.Mvc;
using Poster.Models;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
         public IActionResult ShowListPosts([FromQuery(Name = "page")]int CurrentPage, [FromQuery] string? searchKeyword, [FromQuery] string? title, [FromQuery] string? content, [FromQuery] string? orderby, [FromQuery] int? minprice, [FromQuery] int? maxprice)
         {
            // khi tìm mới thì reset lọc

            int PageSize = 10; // 10 items
            var post = _context.Posts.ToList(); // lấy list post
            title = title ?? "";
            content = content ?? "";
            if (!string.IsNullOrEmpty(searchKeyword)) //check từ khóa tìm kiếm (title)
            {
                post = post.Where(p => p.Title.Contains(searchKeyword)).ToList();
            }
            if (title!=null) // check title
            {
                post = post.Where(p => p.Title.Contains(title)).ToList();
			}
            if (content!=null) //check content
            {
                post = post.Where(p => p.Content.Contains(content)).ToList();
			}
            if (minprice != null && maxprice!=null) // check minprice và maxprice
            {
                post.Where(p => Int32.Parse(p.Title) <= maxprice && Int32.Parse(p.Title) >= minprice);

			}
			if (orderby == "ASC") 
			{
				post = post.OrderBy(c=>c.Title).ToList();
			}
			if (orderby == "DESC")
			{
				post = post.OrderByDescending(c => c.Title).ToList();
			}
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
                GeneralUrl = (pageNumber) => Url.Action("ShowListPosts", new
                {
                    page = pageNumber,
                    searchKeyword = searchKeyword,
                    title = title,
                    content = content,
                    orderby = orderby,
                    minprice = minprice,
                    maxprice = maxprice
                })
            };
            ViewBag.Paging = Paging;
            ViewBag.TotalPost = TotalPost;
            ViewBag.orderby = orderby; // của select
			ViewData["minprice"] = minprice;
			ViewData["maxprice"] = maxprice;
			ViewData["searchKeyword"] = searchKeyword;
			var PostInPage = post.Skip((CurrentPage-1)*PageSize).Take(10).ToList();
			return View(PostInPage);
        }

        public IActionResult Them()
        {
            for (int i = 0; i < 1000; i++)
            {
                Posts p = new Posts()
                {
                    Content = "Content thứ: " + i,
                    Title =i.ToString(),
                    Published = true
                };
                _context.Posts.Add(p);
                _context.SaveChanges();
            }
           return RedirectToAction("ShowListPosts");
        }
        public IActionResult Xoa()
        {
            var post = _context.Posts.Take(1000).ToList();
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