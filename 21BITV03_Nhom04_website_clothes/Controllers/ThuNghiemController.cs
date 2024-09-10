using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class ThuNghiemController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public ThuNghiemController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Fetch all products from the database
            var products = await _context.Products
                .Include(p => p.SubProducts)
                .ToListAsync();

            // Map the products to ProductViewModel
            var productViewModels = products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                OriginalPrice = p.SubProducts.FirstOrDefault()?.OriginalPrice ?? 0,
                DiscountedPrice = p.SubProducts.FirstOrDefault()?.DiscountedPrice,
                ImageUrl = p.SubProducts.FirstOrDefault()?.Linkimage ?? "default-image.png",
                IsAvailable = p.Status ?? false,
                SubProducts = p.SubProducts.Select(sp => new SubProductViewModel
                {
                    SubProductId = sp.SubProductId,
                    MainProductId = sp.MainProductId ?? 0,
                    OriginalPrice = sp.OriginalPrice ?? 0,
                    DiscountedPrice = sp.DiscountedPrice,
                    ColorId = sp.ColorId ?? 0,
                    SizeId = sp.SizeId ?? 0,
                    LinkImage = sp.Linkimage,
                    CreationDate = sp.CreationDate ?? DateTime.Now
                }).ToList()
            }).ToList();

            return View(productViewModels);
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            // Fetch the specific product by ID from the database
            var product = await _context.Products
                .Include(p => p.SubProducts)
                    .ThenInclude(sp => sp.Color)  // Include ProductColor
                .Include(p => p.SubProducts)
                    .ThenInclude(sp => sp.Size)
                .Include(p => p.ReviewProducts)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            // Check if the product exists
            if (product == null)
            {
                return NotFound(); // Return a 404 error if the product is not found
            }

            // Map the product to ProductViewModel
            var productViewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                OriginalPrice = product.SubProducts.FirstOrDefault()?.OriginalPrice ?? 0,
                DiscountedPrice = product.SubProducts.FirstOrDefault()?.DiscountedPrice,
                ImageUrl = product.SubProducts.FirstOrDefault()?.Linkimage ?? "default-image.png",
                IsAvailable = product.Status ?? false,
                SubProducts = product.SubProducts.Select(sp => new SubProductViewModel
                {
                    SubProductId = sp.SubProductId,
                    MainProductId = sp.MainProductId ?? 0,
                    OriginalPrice = sp.OriginalPrice ?? 0,
                    DiscountedPrice = sp.DiscountedPrice,
                    ColorId = sp.ColorId ?? 0,
                    ColorName = sp.Color?.ColorName,  // Include color name
                    SizeId = sp.SizeId ?? 0,
                    SizeName = sp.Size?.SizeName,    // Include size name
                    LinkImage = sp.Linkimage,
                    CreationDate = sp.CreationDate ?? DateTime.Now
                }).ToList(),
                Reviews = product.ReviewProducts.Select(rp => new ReviewProductView
                {
                    IdRv = rp.IdRv,
                    ProductId = rp.ProductId,
                    Username = rp.Username,
                    Email = rp.Email,
                    Comment = rp.Comment
                }).ToList()  // Map ReviewProduct to ProductViewModel
            };

            return View(productViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> AddReview(ReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new ReviewProduct object from the submitted data
                var newReview = new ReviewProduct
                {
                    ProductId = model.ProductId,
                    Username = model.Username,
                    Email = model.Email,
                    Comment = model.Comment
                };

                // Add the new review to the database
                _context.ReviewProducts.Add(newReview);
                await _context.SaveChangesAsync();

                // Redirect back to the product detail page
                return RedirectToAction("ProductDetail", new { id = model.ProductId });
            }

            // If the model is not valid, return to the view with the current model
            return View(model);
        }
        public IActionResult GetAvailableSizes(int colorId, int productId)
        {
            // Fetch sizes available for the selected color and product
            var availableSizes = _context.SubProducts
                .Where(sp => sp.ColorId == colorId && sp.MainProductId == productId)
                .Select(sp => sp.SizeId)
                .Distinct()
                .ToList();

            return Json(new { availableSizes });
        }
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> AddToCart(int productId, int quantity, int colorId, int sizeId)
        {
            var username = HttpContext.User.Identity.Name; // Get the username from the context
            if (username == null)
            {
                return Json(new { success = false, message = "User is not logged in." });
            }

            // Retrieve the userId based on the username
            var userId = await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId) // Select only the UserId
                .FirstOrDefaultAsync();

            if (userId == 0)
            {
                return Json(new { success = false, message = "User not found." });
            }
            var subProductId = await _context.Products
                            .Where(p => p.ProductId == productId)
                            .SelectMany(p => p.SubProducts) // Navigate to SubProducts of this Product
                            .Where(sp => sp.ColorId == colorId && sp.SizeId == sizeId) // Filter by ColorId and SizeId
                            .Select(sp => sp.SubProductId) // Select only SubProductId
                            .FirstOrDefaultAsync();

            if (subProductId == 0) // If no matching SubProduct is found
            {
                return Json(new { success = false, message = "Product variant not found." });
            }
            // Check if the cart exists for the user, otherwise create a new one
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            var existingCartProduct = await _context.CartProductLists
                   .FirstOrDefaultAsync(cp => cp.CartId == cart.CartId && cp.SubProductId == subProductId);

            if (existingCartProduct != null)
            {
                // If it exists, increase the quantity
                existingCartProduct.Quantity += quantity;
                _context.CartProductLists.Update(existingCartProduct);
            }
            else
            {
                // If it doesn't exist, add a new product to the cart
                var cartProduct = new CartProductList
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    SubProductId = subProductId, // Make sure SubProductId corresponds to the selected product variant
                    Quantity = quantity
                };
                _context.CartProductLists.Add(cartProduct);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product added to cart successfully!" });
        }

        public async Task<IActionResult> Cart()
        {
            var username = HttpContext.User.Identity.Name;
            if (username == null)
            {
                return RedirectToAction("Index", "Login");
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
                return View("CartEmpty");
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
                return Json(new { success = true , message = "Success to remove item" });
            }

            return Json(new { success = false, message = "Failed to remove item" });
        }



    }
}
