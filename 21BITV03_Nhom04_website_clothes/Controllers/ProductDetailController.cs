using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Helper;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class ProductDetailController : Controller
    {
        private readonly WebsiteClothesContext _context;
        public ProductDetailController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int id, int? color, int? size, int? material)
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            // Fetch the specific product by ID from the database
            var product = await _context.Products
                 .Include(p => p.SubProducts)
                     .ThenInclude(sp => sp.Color)
                 .Include(p => p.SubProducts)
                     .ThenInclude(sp => sp.Size)
                 .Include(p => p.SubProducts)
                     .ThenInclude(sp => sp.Material)
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
                SubProducts = product.SubProducts.Select(sp => new SubProductViewModel
                {
                    SubProductId = sp.SubProductId,
                    MainProductId = sp.MainProductId ?? 0,
                    OriginalPrice = sp.OriginalPrice ?? 0,
                    DiscountedPrice = sp.DiscountedPrice,
                    ColorId = sp.ColorId ?? 0,
                    ColorName = sp.Color?.ColorName,  // Include color name
                    SizeId = sp.SizeId ?? 0,
                    SizeName = sp.Size?.SizeName,
                    MaterialId = sp.MaterialId ?? 0,
                    MaterialName = sp.Material?.MaterialName,
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
        public IActionResult GetAvailableColorsAndSizesByMaterial(int materialId, int productId)
        {
            var availableColors = _context.SubProducts
                .Where(sp => sp.MaterialId == materialId && sp.MainProductId == productId)
                .Select(sp => sp.ColorId)
                .Distinct()
                .ToList();

            var availableSizes = _context.SubProducts
                .Where(sp => sp.MaterialId == materialId && sp.MainProductId == productId)
                .Select(sp => sp.SizeId)
                .Distinct()
                .ToList();

            return Json(new { availableColors, availableSizes });
        }

        public IActionResult GetAvailableSizesAndMaterials(int colorId, int productId)
        {
            var availableSizes = _context.SubProducts
                .Where(sp => sp.ColorId == colorId && sp.MainProductId == productId)
                .Select(sp => sp.SizeId)
                .Distinct()
                .ToList();

            var availableMaterials = _context.SubProducts
                .Where(sp => sp.ColorId == colorId && sp.MainProductId == productId)
                .Select(sp => sp.MaterialId)
                .Distinct()
                .ToList();

            return Json(new { availableSizes, availableMaterials });
        }

        public IActionResult GetAvailableColorsAndMaterials(int sizeId, int productId)
        {
            var availableColors = _context.SubProducts
                .Where(sp => sp.SizeId == sizeId && sp.MainProductId == productId)
                .Select(sp => sp.ColorId)
                .Distinct()
                .ToList();

            var availableMaterials = _context.SubProducts
                .Where(sp => sp.SizeId == sizeId && sp.MainProductId == productId)
                .Select(sp => sp.MaterialId)
                .Distinct()
                .ToList();

            return Json(new { availableColors, availableMaterials });
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

    }
}
