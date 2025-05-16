using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Helper;
using _21BITV03_Nhom04_website_clothes.Models;
using _21BITV03_Nhom04_website_clothes.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly WebsiteClothesContext _context;
        private readonly OrderService _orderService;
        public CheckoutController(OrderService orderService, WebsiteClothesContext context)
        {
            _orderService = orderService;
            _context = context;
        }
        public async Task<IActionResult> Index()

        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
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
                        .ThenInclude(mp => mp.MainProduct)
                .Include(c => c.CartProductLists)
                    .ThenInclude(cpl => cpl.SubProduct)
                        .ThenInclude(sp => sp.Size)
                .Include(c => c.CartProductLists)
                    .ThenInclude(cpl => cpl.SubProduct)
                        .ThenInclude(sp => sp.Material)
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
                    ColorName = cpl.SubProduct?.Color?.ColorName ?? "Unknown Color",
                    SizeName = cpl.SubProduct?.Size?.SizeName ?? "Unknown Size",
                    MaterialName = cpl.SubProduct?.Material?.MaterialName ?? "Unknown Material",

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
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var username = HttpContext.User.Identity.Name;
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orderService = new OrderService(_context);
            int userId = await orderService.GetUserIdFromUsernameAsync(username);

            if (userId == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var cart = await orderService.GetUserCartAsync(userId);
            if (cart == null || !cart.CartProductLists.Any())
            {
                return View(new CartViewModel { CartProducts = new List<CartProductViewModel>() });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy chi tiết sản phẩm
                var productDetails = await orderService.GetProductDetailsFromCartAsync(cart);

                // Tạo đơn hàng
                var order = orderService.CreateOrder(userId, orderMessage, paymentMethod, productDetails);
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Xử lý kho và giao dịch
                await orderService.ProcessInventoryAsync(order, productDetails);

                // Xóa giỏ hàng
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ViewBag.ErrorMessage = "Có lỗi xảy ra trong quá trình thanh toán: " + ex.Message;
                return View("Error");
            }

            return RedirectToAction("OrderConfirmation");
        }
        public IActionResult OrderConfirmation()
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            return View();
        }


    }
}
