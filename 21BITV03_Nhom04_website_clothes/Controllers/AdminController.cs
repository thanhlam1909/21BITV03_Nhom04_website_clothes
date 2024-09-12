using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly WebsiteClothesContext _context;


        public AdminController(WebsiteClothesContext context)
        {
            _context = context;
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.AspNetUsers
                .Include(u => u.UserInfo)
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Phone = int.Parse(user.PhoneNumber), // Assuming phone number is stored as an integer
                FullName = user.UserInfo.FullName,
                Address = user.UserInfo.Address,
            };

            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View(model);
        }

        // POST: Edit User
        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username is unique (excluding the current user)
                var existingUser = await _context.AspNetUsers
                    .FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Id != model.UserId);

                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Username is already taken. Please choose a different one.");
                    ViewBag.Roles = _context.AspNetRoles.ToList();
                    return View(model);
                }

                // Fetch user with UserInfo and existing roles
                var user = await _context.AspNetUsers
                    .Include(u => u.UserInfo)
                    .Include(u => u.Roles) // Include user roles to manage role changes
                    .FirstOrDefaultAsync(u => u.Id == model.UserId);

                if (user == null)
                {
                    return NotFound();
                }

                // Update user details
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PasswordHash = model.Password; // Consider hashing the password before storing it
                user.PhoneNumber = model.Phone.ToString();

                // Update UserInfo details
                user.UserInfo.FullName = model.FullName;
                user.UserInfo.Address = model.Address;
                user.UserInfo.Phone = model.Phone;
                user.UserInfo.Email = model.Email;
                user.UserInfo.UserName = model.UserName;
                user.UserInfo.Password = model.Password; // Consider hashing



                _context.AspNetUsers.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // Reload roles if validation fails
            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View(model);
        }


    }
}
