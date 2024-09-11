using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly WebsiteClothesContext _context;


        public AdminController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.User.Identity.Name; // Get the username from the context
            if (username == null)
            {
                return Json(new { success = false, message = "User is not logged in." });
            }

            // Retrieve the userInfo based on the username
            var userInfo = await _context.UserInfos
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (userInfo == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Pass the userInfo to the view
            return View(userInfo);
        }

    }
}
