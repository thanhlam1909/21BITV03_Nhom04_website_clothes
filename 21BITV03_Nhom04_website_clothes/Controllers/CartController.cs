using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly WebsiteClothesContext _context;

        public CartController(WebsiteClothesContext context)
        {
            _context = context;
        }

        [HttpGet("GetCartCount")]
        public async Task<IActionResult> GetCartCount()
        {
            // Assuming you have some way to get the user ID from the current context
            var username = HttpContext.User.Identity?.Name; // Adjust as needed for your user identification
            var userId = await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId) // Select only the UserId
                .FirstOrDefaultAsync();
            if (userId == null)
            {
                return Unauthorized();
            }

            // Find the cart for the current user
            var cart = await _context.Carts
                .Include(c => c.CartProductLists) // Ensure to include related data
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for the current user.");
            }

            // Count the number of items in the cart
            var cartCount = cart.CartProductLists.Sum(cpl => cpl.Quantity ?? 0);

            return Ok(new { cartCount });
        }
    }
}
