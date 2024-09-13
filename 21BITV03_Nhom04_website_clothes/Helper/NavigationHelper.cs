using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _21BITV03_Nhom04_website_clothes.Helper
{
    public static class NavigationHelper
    {
        public static IActionResult RedirectToRoleBasedPage(Controller controller)
        {
            // Kiểm tra nếu người dùng đã đăng nhập
            if (controller.User.Identity.IsAuthenticated)
            {
                // Lấy vai trò của người dùng đã đăng nhập
                var roleClaim = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                var roleName = roleClaim?.Value;

                // Chuyển hướng dựa trên vai trò
                if (roleName == "Admin")
                {
                    return controller.RedirectToAction("Index", "Admin");
                }
                else
                {
                    return controller.RedirectToAction("Index", "Home"); // Trang mặc định cho người dùng không phải admin
                }
            }

            // Nếu chưa đăng nhập, không chuyển hướng
            return null;
        }
    }
}
