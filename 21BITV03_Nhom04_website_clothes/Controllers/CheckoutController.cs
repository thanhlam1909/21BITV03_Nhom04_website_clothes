using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public CheckoutController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()

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

            if (cart == null || !cart.CartProductLists.Any())
            {
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
                    ProductName = cpl.SubProduct?.MainProduct?.ProductName,
                    ColorName = cpl.SubProduct?.Color?.ColorName,
                    SizeName = cpl.SubProduct?.Size?.SizeName,
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
        public async Task<IActionResult> Checkout(CartViewModel model, string orderMessage, string paymentMethod)
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
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartProductLists.Any())
            {
                return View(new CartViewModel { CartProducts = new List<CartProductViewModel>() });
            }

            // Fetch product details based on ProductId and SubProductId
            var productDetails = await _context.CartProductLists
                .Include(cpl => cpl.SubProduct)
                    .ThenInclude(sp => sp.MainProduct)
                .Include(cpl => cpl.SubProduct)
                    .ThenInclude(sp => sp.Color)
                .Include(cpl => cpl.SubProduct)
                    .ThenInclude(sp => sp.Size)
                .Where(cpl => cart.CartProductLists.Select(list => list.CartProductList1).Contains(cpl.CartProductList1))
                .Select(cpl => new
                {   ProductId= cpl.ProductId,
                    ProductName = cpl.SubProduct.MainProduct.ProductName,
                    ColorName = cpl.SubProduct.Color.ColorName,
                    SizeName = cpl.SubProduct.Size.SizeName,
                    Quantity = cpl.Quantity,
                    DiscountedPrice = cpl.SubProduct.DiscountedPrice
                })
                .ToListAsync();

            var order = new Order
            {
                UserId = userId,
                Message = orderMessage,
                PaymentMethod = paymentMethod,
                OrderStatus = "Pending", // Set the initial order status
                OrderProductLists = productDetails.Select(pd => new OrderProductList
                {
                    ProductId =pd.ProductId,
                    ProductName = pd.ProductName,
                    Quanity = pd.Quantity,
                    ColorName = pd.ColorName,
                    SizeName = pd.SizeName
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Optionally, clear the cart
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation"); // Redirect to a confirmation page
        }
        public IActionResult OrderConfirmation()
        {
            return View();
        }


    }
}
