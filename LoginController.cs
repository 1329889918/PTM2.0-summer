using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PTM2._0.Data;
using PTM2._0.Extensions;
using PTM2._0.Models;
using PTM2._0.ViewModels;

namespace PTM2._0.Controllers
{
    public class LoginController : Controller
    {
        private readonly PTM2_0Context _context;

        public LoginController(PTM2_0Context context)
        {
            _context = context;
        }

        // 登录页面（GET）
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // 处理登录请求（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 查询用户（注意：实际项目中建议使用密码哈希，而不是明文比较）
            var user = await _context.User
                .FirstOrDefaultAsync(u =>
                    u.Name == model.Username &&
                    u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "用户名或密码不正确");
                return View(model);
            }

            // 登录成功，设置Session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetBool("IsAdmin", user.IsAdmin);
            HttpContext.Session.SetEnum<Gender>("UserGender", user.Gender);

            // 登录成功提示
            TempData["SuccessMessage"] = "登录成功";

            // 重定向到首页或之前的URL
            if (Url.IsLocalUrl(Request.Query["ReturnUrl"]))
            {
                return Redirect(Request.Query["ReturnUrl"]);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // 注册页面（GET）
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 处理注册请求（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 检查用户名是否已存在
            if (await _context.User.AnyAsync(u => u.Name == model.Username))
            {
                ModelState.AddModelError("Username", "用户名已被使用");
                return View(model);
            }

            // 创建新用户
            var user = new User
            {
                Name = model.Username,
                Birthdate = model.Birthdate,
                Address = model.Address,
                Email = model.Email,
                Phone = model.Phone,
                Gender = model.Gender,
                Password = model.Password, // 注意：实际项目中应使用密码哈希
                IsAdmin = false,           // 注册用户默认为普通用户
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            // 注册成功，自动登录
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetBool("IsAdmin", user.IsAdmin);
            HttpContext.Session.SetEnum<Gender>("UserGender", user.Gender);

            TempData["SuccessMessage"] = "注册成功，欢迎加入！";
            return RedirectToAction("Index", "Home");
        }

        // 退出登录
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // 清除Session
            HttpContext.Session.Clear();

            // 退出成功提示
            TempData["SuccessMessage"] = "已成功退出登录";

            return RedirectToAction("Index", "Home");
        }
    }
}