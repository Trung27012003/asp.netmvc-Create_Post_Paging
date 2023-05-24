using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Poster.Models;
using System.Diagnostics;
using OfficeOpenXml;
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

        public IActionResult Index()
        {

            return View();
        }
        public async Task<IActionResult> ShowListUser()
        {
            return View(await _context.Users.ToListAsync());
        }
        public IActionResult ShowListPosts([FromQuery(Name = "page")] int CurrentPage, [FromQuery] string? searchKeyword, [FromQuery] string? title, [FromQuery] string? content, [FromQuery] string? orderby, [FromQuery] int? minprice, [FromQuery] int? maxprice)
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
            if (title != null) // check title
            {
                post = post.Where(p => p.Title.Contains(title)).ToList();
            }
            if (content != null) //check content
            {
                post = post.Where(p => p.Content.Contains(content)).ToList();
            }
            if (minprice != null && maxprice != null) // check minprice và maxprice
            {
                post.Where(p => Int32.Parse(p.Title) <= maxprice && Int32.Parse(p.Title) >= minprice);

            }
            if (orderby == "ASC")
            {
                post = post.OrderBy(c => c.Title).ToList();
            }
            if (orderby == "DESC")
            {
                post = post.OrderByDescending(c => c.Title).ToList();
            }
            int TotalPost = post.Count(); // tổng số bài
            int CountPages = (int)Math.Ceiling((double)TotalPost / PageSize);
            if (CurrentPage > CountPages)
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
            var PostInPage = post.Skip((CurrentPage - 1) * PageSize).Take(10).ToList();
            return View(PostInPage);
        }

        public IActionResult Them()
        {
            for (int i = 0; i < 1000; i++)
            {
                Posts p = new Posts()
                {
                    Content = "Content thứ: " + i,
                    Title = i.ToString(),
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
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] string Username, [FromForm] string Password)
        {
            var acc = await _context.Users.ToListAsync();
            var user = acc.FirstOrDefault(c => c.Email == Username && c.Password == Password);
            if (user != null)
            {
                HttpContext.Session.SetString("userId", user.ID.ToString());
                return RedirectToAction("ShowListPosts");
            }
            return View();
        }
        public async Task LoginGG() // chuyển hướng đến trang đăng nhập google
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()// trang đăng nhập google
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims; // lấy thông tin người dùng

            string email = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value; // lấy email
            var acc = await _context.Users.ToListAsync();
            var user = acc.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (user.IsConnectGooogle) // nếu tài khoản đã kết nối thì lấy id
                {
                    HttpContext.Session.SetString("userId", user.ID.ToString());
                    return RedirectToAction("ShowListPosts");
                }
                else // nếu chưa kết nối thì kết nối, cần phải hỏi lại người dùng
                {
                    user.IsConnectGooogle = true;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("userId", user.ID.ToString());
                    return RedirectToAction("ShowListPosts");
                }
            }
            else // nếu tài khoản chưa có thì chuyển hướng đến trang đăng kí
            {
                string img = claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;
                string name = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
                HttpContext.Session.SetString("email", email);
                HttpContext.Session.SetString("img", img);
                HttpContext.Session.SetString("name", name);
                return RedirectToAction("SignUp");
            }
        }
        public async Task<IActionResult> LogOut() // đăng xuất
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("ShowListPosts");
        }
        public async Task<IActionResult> SignUp()
        {
            ViewBag.email = HttpContext.Session.GetString("email");
            ViewBag.name = HttpContext.Session.GetString("name");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp([FromForm] string name, [FromForm] string email, [FromForm] string password)
        {
            if (name != null && email != null && name != password)
            {
                var user = new User()
                {
                    Name = name,
                    Email = email,
                    Password = password,
                    img = "",
                    IsConnectGooogle = true

                };
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("userId", user.ID.ToString());
            }
            return RedirectToAction("ShowListPosts");
        }
        public IActionResult ExportExcel()// cài nuget EPPlus   
        {
            
            var data = _context.Posts.ToList();
            var stream  = new MemoryStream();
            using(var package = new ExcelPackage(stream))
            {
				var sheet = package.Workbook.Worksheets.Add("Danh sách post");
                sheet.Cells[1,1].Value = "STT";
                sheet.Cells[1,2].Value = "Tiêu đề";
                sheet.Cells[1,3].Value = "Nội dung";
                sheet.Cells[1,4].Value = "Trạng thái";
                int stt = 1;
                int rowIndex = 2;
                foreach (var item in data)
                {
                    sheet.Cells[rowIndex, 1].Value = stt;
                    sheet.Cells[rowIndex, 2].Value = item.Title;
                    sheet.Cells[rowIndex, 3].Value = item.Content;
                    sheet.Cells[rowIndex,4].Value = item.Published==true?"Đã xuất bản" : "Chưa xuất bản";
					rowIndex++;
					stt++;
				}
				package.Save();
			}
            stream.Position = 0;
            var fileName = $"ListPost.xlsx";
            return File(stream,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}