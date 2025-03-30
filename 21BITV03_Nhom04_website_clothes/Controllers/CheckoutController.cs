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
        //public async Task<IActionResult> Checkout(CartViewModel model, string orderMessage, string paymentMethod)
        //{
        //    var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
        //    if (redirectResult != null)
        //    {
        //        return redirectResult;
        //    }
        //    var username = HttpContext.User.Identity.Name;
        //    if (username == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var userId = await _context.UserInfos
        //        .Where(u => u.UserName == username)
        //        .Select(u => u.UserId)
        //        .FirstOrDefaultAsync();

        //    if (userId == 0)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var cart = await _context.Carts
        //        .Include(c => c.CartProductLists)
        //        .FirstOrDefaultAsync(c => c.UserId == userId);

        //    if (cart == null || !cart.CartProductLists.Any())
        //    {
        //        return View(new CartViewModel { CartProducts = new List<CartProductViewModel>() });
        //    }

        //    // Fetch product details based on ProductId and SubProductId
        //    var productDetails = await _context.CartProductLists
        //        .Include(cpl => cpl.SubProduct)
        //            .ThenInclude(sp => sp.MainProduct)
        //        .Include(cpl => cpl.SubProduct)
        //            .ThenInclude(sp => sp.Color)
        //        .Include(cpl => cpl.SubProduct)
        //            .ThenInclude(sp => sp.Size)
        //        .Where(cpl => cart.CartProductLists.Select(list => list.CartProductList1).Contains(cpl.CartProductList1))
        //        .Select(cpl => new
        //        {   ProductId= cpl.ProductId,
        //            ProductName = cpl.SubProduct.MainProduct.ProductName,
        //            ColorName = cpl.SubProduct.Color.ColorName,
        //            SizeName = cpl.SubProduct.Size.SizeName,
        //            MaterialName = cpl.SubProduct.Material.MaterialName,
        //            Quantity = cpl.Quantity,
        //            SubProductId = cpl.SubProductId,
        //            DiscountedPrice = cpl.SubProduct.DiscountedPrice
        //        })
        //        .ToListAsync();

        //    var order = new Order
        //    {
        //        UserId = userId,
        //        Message = orderMessage,
        //        PaymentMethod = paymentMethod,
        //        OrderStatus = "Pending", // Set the initial order status
        //        OrderProductLists = productDetails.Select(pd => new OrderProductList
        //        {
        //            ProductId =pd.ProductId,
        //            ProductName = pd.ProductName,
        //            Quanity = pd.Quantity,
        //            ColorName = pd.ColorName,
        //            SizeName = pd.SizeName,
        //            Price = pd.DiscountedPrice,
        //            SubproductId = pd.SubProductId,
        //            MaterialName = pd.MaterialName  
        //        }).ToList()
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    // Optionally, clear the cart
        //    _context.Carts.Remove(cart);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("OrderConfirmation"); // Redirect to a confirmation page
        //}
        //[HttpPost]
        //public async Task<IActionResult> Checkout(CartViewModel model, string orderMessage, string paymentMethod)
        //{
        //    var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
        //    if (redirectResult != null)
        //    {
        //        return redirectResult;
        //    }
        //    var username = HttpContext.User.Identity.Name;
        //    if (username == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var userId = await _context.UserInfos
        //        .Where(u => u.UserName == username)
        //        .Select(u => u.UserId)
        //        .FirstOrDefaultAsync();

        //    if (userId == 0)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var cart = await _context.Carts
        //        .Include(c => c.CartProductLists)
        //            .ThenInclude(cpl => cpl.SubProduct)
        //        .FirstOrDefaultAsync(c => c.UserId == userId);

        //    if (cart == null || !cart.CartProductLists.Any())
        //    {
        //        return View(new CartViewModel { CartProducts = new List<CartProductViewModel>() });
        //    }

        //    // Fetch product details based on ProductId and SubProductId
        //    var productDetails = await _context.CartProductLists
        //        .Include(cpl => cpl.SubProduct)
        //            .ThenInclude(sp => sp.MainProduct)
        //        .Include(cpl => cpl.SubProduct)
        //            .ThenInclude(sp => sp.Color)
        //        .Include(cpl => cpl.SubProduct)
        //            .ThenInclude(sp => sp.Size)
        //        .Include(cpl => cpl.SubProduct)
        //            .ThenInclude(sp => sp.Material)
        //        .Where(cpl => cart.CartProductLists.Select(list => list.CartProductList1).Contains(cpl.CartProductList1))
        //        .Select(cpl => new
        //        {
        //            ProductId = cpl.ProductId,
        //            ProductName = cpl.SubProduct.MainProduct.ProductName,
        //            ColorName = cpl.SubProduct.Color.ColorName,
        //            SizeName = cpl.SubProduct.Size.SizeName,
        //            MaterialName = cpl.SubProduct.Material.MaterialName,
        //            Quantity = cpl.Quantity,
        //            SubProductId = cpl.SubProductId,
        //            DiscountedPrice = cpl.SubProduct.DiscountedPrice ?? cpl.SubProduct.OriginalPrice ?? 0
        //        })
        //        .ToListAsync();

        //    // Tính tổng OrderTotal
        //    double orderTotal = productDetails.Sum(pd => pd.DiscountedPrice * (pd.Quantity ?? 0));
        //    // Tạo đối tượng Order
        //    var order = new Order
        //    {
        //        UserId = userId,
        //        Message = orderMessage,
        //        PaymentMethod = paymentMethod,
        //        OrderStatus = "Pending",
        //        OrderTotal = orderTotal, // Gán giá trị OrderTotal
        //        OrderDate = DateTime.Now,
        //        OrderProductLists = productDetails.Select(pd => new OrderProductList
        //        {
        //            ProductId = pd.ProductId,
        //            ProductName = pd.ProductName,
        //            Quanity = pd.Quantity,
        //            ColorName = pd.ColorName,
        //            SizeName = pd.SizeName,
        //            Price = pd.DiscountedPrice,
        //            SubproductId = pd.SubProductId,
        //            MaterialName = pd.MaterialName
        //        }).ToList()
        //    };

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        _context.Orders.Add(order);
        //        await _context.SaveChangesAsync();

        //        // Dictionary to track InventoryTransactions per warehouse
        //        var warehouseTransactions = new Dictionary<int, InventoryTransaction>();

        //        // Process each product in the cart using FIFO
        //        foreach (var product in productDetails)
        //        {
        //            int remainingQuantity = product.Quantity ?? 0;
        //            var SubproductskuId = await _context.SubProducts
        //                    .Where(sp => sp.SubProductId == product.SubProductId)
        //                    .Select(sp => sp.SkuId)
        //                    .FirstOrDefaultAsync();
        //            int? skuId = SubproductskuId;

        //            if (skuId == null) continue; // Skip if no SKU is associated

        //            // Fetch WarehouseBatches for this SKU, ordered by ImportDate (FIFO)
        //            var warehouseBatches = await _context.WarehouseBatches
        //                .Where(wb => wb.SkuId == skuId && wb.QuantityAvailable > 0)
        //                .OrderBy(wb => wb.ImportDate)
        //                .ToListAsync();

        //            foreach (var batch in warehouseBatches)
        //            {
        //                if (remainingQuantity <= 0) break;

        //                int quantityToDeduct = Math.Min(remainingQuantity, batch.QuantityAvailable);

        //                // Update WarehouseBatch
        //                batch.QuantityAvailable -= quantityToDeduct;
        //                _context.WarehouseBatches.Update(batch);

        //                // Update WarehouseProduct
        //                var warehouseProduct = await _context.WarehouseProducts
        //                    .FirstOrDefaultAsync(wp => wp.WarehouseId == batch.WarehouseId && wp.SkuId == skuId);
        //                if (warehouseProduct != null)
        //                {
        //                    warehouseProduct.Quantity -= quantityToDeduct;
        //                    warehouseProduct.LastUpdated = DateTime.Now;
        //                    _context.WarehouseProducts.Update(warehouseProduct);
        //                }

        //                // Get or create InventoryTransaction for this warehouse
        //                if (!warehouseTransactions.TryGetValue(batch.WarehouseId, out var invTransaction))
        //                {
        //                    invTransaction = new InventoryTransaction
        //                    {
        //                        TransactionType = "Xuất Hàng",
        //                        TransactionDate = DateTime.Now,
        //                        WarehouseId = batch.WarehouseId,
        //                        Notes = $"Sale for Order ID: {order.OrderId} from Warehouse ID: {batch.WarehouseId}"
        //                    };
        //                    _context.InventoryTransactions.Add(invTransaction);
        //                    await _context.SaveChangesAsync(); // Save to get TransactionId
        //                    warehouseTransactions[batch.WarehouseId] = invTransaction;
        //                }

        //                // Create InventoryTransactionDetail
        //                var transactionDetail = new InventoryTransactionDetail
        //                {
        //                    TransactionId = invTransaction.TransactionId,
        //                    SubProductId = product.SubProductId ?? 0,
        //                    Quantity = quantityToDeduct,
        //                    UnitPrice = (decimal)product.DiscountedPrice
        //                };
        //                _context.InventoryTransactionDetails.Add(transactionDetail);

        //                remainingQuantity -= quantityToDeduct;
        //            }

        //            if (remainingQuantity > 0)
        //            {
        //                throw new Exception($"Insufficient inventory for SKU ID: {skuId}");
        //            }
        //        }

        //        // Clear the cart
        //        _context.Carts.Remove(cart);
        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();

        //        // Optionally pass error to view
        //        ViewBag.ErrorMessage = "Có lỗi xảy ra trong quá trình thanh toán. Vui lòng thử lại.";
        //        return View("Error"); 
        //    }
        //    return RedirectToAction("OrderConfirmation"); // Chuyển hướng đến trang xác nhận
        //}
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
