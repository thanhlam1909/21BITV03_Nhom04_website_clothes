using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebsiteClothesContext _context;


        public AccountController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> LogOut()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            // If the user is authenticated, redirect to the Index action
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel modelLogin)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ValidateMessage"] = "Please enter valid data.";
                return View(modelLogin);
            }

            var user = _context.AspNetUsers
                .FirstOrDefault(u => u.UserName == modelLogin.UserName);

            if (user != null && modelLogin.UserName == user.UserName && modelLogin.PassWord == user.PasswordHash)
            {
                List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, modelLogin.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("OtherProperties", "Example Role") // Add more claims as needed
            };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                return RedirectToAction("Index", "Account");
            }

            ViewData["ValidateMessage"] = "User not found or password incorrect.";
            return View(modelLogin);
        }
    }
}
