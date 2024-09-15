using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace _21BITV03_Nhom04_website_clothes.Helper
{
    public static class NavigationHelper
    {
        public static IActionResult RedirectToRoleBasedPage(Controller controller)
        {
            if (!controller.User.Identity.IsAuthenticated)
            {
                return null; // No redirection for unauthenticated users
            }

            var roleClaim = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var roleName = roleClaim?.Value;

            if (string.IsNullOrEmpty(roleName))
            {
                return null; // No redirection if user has no role
            }

            var currentController = controller.ControllerContext.ActionDescriptor.ControllerName;

            if (roleName == "Admin" && currentController != "Admin")
            {
                return controller.RedirectToAction("Index", "Admin"); // Redirect Admins to Admin area
            }
            else
            {
                // Non-Admins don't get redirected - stay on current page
                return null;
            }
        }
    }
}