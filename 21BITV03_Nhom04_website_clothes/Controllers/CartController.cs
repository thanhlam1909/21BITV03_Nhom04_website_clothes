using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{

    public class CartController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public CartController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Cart()
        {
            var username = HttpContext.User.Identity.Name;
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            if (userId == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var cart = await _context.Carts
                .Include(c => c.CartProductLists)
                    .ThenInclude(cpl => cpl.SubProduct)
                        .ThenInclude(sp => sp.Color)
                .Include(c => c.CartProductLists)
                    .ThenInclude(cpl => cpl.SubProduct)
                        .ThenInclude(sp => sp.Size)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartProductLists == null || !cart.CartProductLists.Any())
            {
                // Return a view that indicates the cart is empty
                return View(new CartViewModel { CartProducts = new List<CartProductViewModel>() });
            }

            var cartViewModel = new CartViewModel
            {
                CartId = cart.CartId,
                UserId = userId,
                CartProducts = cart.CartProductLists.Select(cpl => new CartProductViewModel
                {
                    ProductId = cpl.ProductId ?? 0,
                    SubProductId = cpl.SubProductId ?? 0,
                    ProductName = cpl.SubProduct?.MainProduct?.ProductName ?? "Unknown Product",
                    ColorName = cpl.SubProduct?.Color?.ColorName ?? "Unknown Color",
                    SizeName = cpl.SubProduct?.Size?.SizeName ?? "Unknown Size",
                    Quantity = cpl.Quantity ?? 1,
                    OriginalPrice = cpl.SubProduct?.OriginalPrice ?? 0,
                    DiscountedPrice = cpl.SubProduct?.DiscountedPrice ?? cpl.SubProduct?.OriginalPrice ?? 0,
                    ImageUrl = cpl.SubProduct?.Linkimage ?? "default-image.png",
                    CartProductListId = cpl.CartProductList1 // Adjust to use CartProductList1
                }).ToList()
            };

            return View(cartViewModel);
        }


        [HttpPost]
        public async Task<JsonResult> UpdateQuantity(int cartProductListId, int quantity)
        {
            var username = HttpContext.User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User is not authenticated" });
            }

            var userId = await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            var cartProduct = await _context.Carts
                .Where(c => c.UserId == userId)
                .SelectMany(c => c.CartProductLists)
                .FirstOrDefaultAsync(cpl => cpl.CartProductList1 == cartProductListId);

            if (cartProduct != null && quantity > 0)
            {
                cartProduct.Quantity = quantity;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Update to quantity item" });
            }

            return Json(new { success = false, message = "Failed to update quantity" });
        }
        [HttpPost]
        public async Task<JsonResult> RemoveFromCart(int cartProductListId)
        {
            var username = HttpContext.User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User is not authenticated" });
            }

            var userId = await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            var cartProduct = await _context.Carts
                .Where(c => c.UserId == userId)
                .SelectMany(c => c.CartProductLists)
                .FirstOrDefaultAsync(cpl => cpl.CartProductList1 == cartProductListId);

            if (cartProduct != null)
            {
                _context.CartProductLists.Remove(cartProduct);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Success to remove item" });
            }

            return Json(new { success = false, message = "Failed to remove item" });
        }
        [HttpGet("GetCartCount")]
        public async Task<IActionResult> GetCartCount()
        {

            var username = HttpContext.User.Identity?.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            var userId = await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts
                .Include(c => c.CartProductLists)
                .ThenInclude(cpl => cpl.SubProduct) // Ensure to include SubProduct
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for the current user.");
            }

            // Count the number of unique SubProduct items in the cart
            var distinctSubProductCount = cart.CartProductLists
                .Select(cpl => cpl.SubProductId) // Select SubProductId
                .Distinct() // Ensure uniqueness
                .Count();

            return Ok(new { cartCount = distinctSubProductCount });
        }
    }
}
