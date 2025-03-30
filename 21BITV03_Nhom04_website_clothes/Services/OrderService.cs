using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _21BITV03_Nhom04_website_clothes.Services
{
    public class OrderService
    {
        private readonly WebsiteClothesContext _context;

        public OrderService(WebsiteClothesContext context)
        {
            _context = context;
        }

        // Lấy thông tin người dùng từ username
        public async Task<int> GetUserIdFromUsernameAsync(string username)
        {
            return await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();
        }

        // Lấy giỏ hàng của người dùng
        public async Task<Cart> GetUserCartAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.CartProductLists)
                    .ThenInclude(cpl => cpl.SubProduct)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // Lấy chi tiết sản phẩm từ giỏ hàng
        public async Task<List<ProductDetail>> GetProductDetailsFromCartAsync(Cart cart)
        {
            return await _context.CartProductLists
                .Include(cpl => cpl.SubProduct)
                    .ThenInclude(sp => sp.MainProduct)
                .Include(cpl => cpl.SubProduct)
                    .ThenInclude(sp => sp.Color)
                .Include(cpl => cpl.SubProduct)
                    .ThenInclude(sp => sp.Size)
                .Include(cpl => cpl.SubProduct)
                    .ThenInclude(sp => sp.Material)
                .Where(cpl => cart.CartProductLists.Select(list => list.CartProductList1).Contains(cpl.CartProductList1))
                .Select(cpl => new ProductDetail
                {
                    ProductId = cpl.ProductId,
                    ProductName = cpl.SubProduct.MainProduct.ProductName,
                    ColorName = cpl.SubProduct.Color.ColorName,
                    SizeName = cpl.SubProduct.Size.SizeName,
                    MaterialName = cpl.SubProduct.Material.MaterialName,
                    Quantity = cpl.Quantity,
                    SubProductId = cpl.SubProductId,
                    DiscountedPrice = cpl.SubProduct.DiscountedPrice ?? cpl.SubProduct.OriginalPrice ?? 0
                })
                .ToListAsync();
        }

        // Tạo đơn hàng và tính tổng
        public Order CreateOrder(int userId, string orderMessage, string paymentMethod, List<ProductDetail> productDetails)
        {
            double orderTotal = productDetails.Sum(pd => pd.DiscountedPrice * (pd.Quantity ?? 0));
            var order = new Order
            {
                UserId = userId,
                Message = orderMessage,
                PaymentMethod = paymentMethod,
                OrderStatus = "Pending",
                OrderTotal = orderTotal,
                OrderDate = DateTime.Now,
                OrderProductLists = productDetails.Select(pd => new OrderProductList
                {
                    ProductId = pd.ProductId,
                    ProductName = pd.ProductName,
                    Quanity = pd.Quantity,
                    ColorName = pd.ColorName,
                    SizeName = pd.SizeName,
                    Price = pd.DiscountedPrice,
                    SubproductId = pd.SubProductId,
                    MaterialName = pd.MaterialName
                }).ToList()
            };
            return order;
        }

        // Xử lý kho và giao dịch (FIFO)
        public async Task ProcessInventoryAsync(Order order, List<ProductDetail> productDetails)
        {
            var warehouseTransactions = new Dictionary<int, InventoryTransaction>();

            foreach (var product in productDetails)
            {
                int remainingQuantity = product.Quantity ?? 0;
                var subProductSkuId = await _context.SubProducts
                    .Where(sp => sp.SubProductId == product.SubProductId)
                    .Select(sp => sp.SkuId)
                    .FirstOrDefaultAsync();
                int? skuId = subProductSkuId;

                if (skuId == null) continue;

                var warehouseBatches = await _context.WarehouseBatches
                    .Where(wb => wb.SkuId == skuId && wb.QuantityAvailable > 0)
                    .OrderBy(wb => wb.ImportDate)
                    .ToListAsync();

                foreach (var batch in warehouseBatches)
                {
                    if (remainingQuantity <= 0) break;

                    int quantityToDeduct = Math.Min(remainingQuantity, batch.QuantityAvailable);

                    batch.QuantityAvailable -= quantityToDeduct;
                    _context.WarehouseBatches.Update(batch);

                    var warehouseProduct = await _context.WarehouseProducts
                        .FirstOrDefaultAsync(wp => wp.WarehouseId == batch.WarehouseId && wp.SkuId == skuId);
                    if (warehouseProduct != null)
                    {
                        warehouseProduct.Quantity -= quantityToDeduct;
                        warehouseProduct.LastUpdated = DateTime.Now;
                        _context.WarehouseProducts.Update(warehouseProduct);
                    }

                    if (!warehouseTransactions.TryGetValue(batch.WarehouseId, out var invTransaction))
                    {
                        invTransaction = new InventoryTransaction
                        {
                            TransactionType = "Xuất Hàng",
                            TransactionDate = DateTime.Now,
                            WarehouseId = batch.WarehouseId,
                            Notes = $"Sale for Order ID: {order.OrderId} from Warehouse ID: {batch.WarehouseId}"
                        };
                        _context.InventoryTransactions.Add(invTransaction);
                        await _context.SaveChangesAsync(); // Lưu để lấy TransactionId
                        warehouseTransactions[batch.WarehouseId] = invTransaction;
                    }

                    var transactionDetail = new InventoryTransactionDetail
                    {
                        TransactionId = invTransaction.TransactionId,
                        SubProductId = product.SubProductId ?? 0,
                        Quantity = quantityToDeduct,
                        UnitPrice = (decimal)product.DiscountedPrice
                    };
                    _context.InventoryTransactionDetails.Add(transactionDetail);

                    remainingQuantity -= quantityToDeduct;
                }

                if (remainingQuantity > 0)
                {
                    throw new Exception($"Insufficient inventory for SKU ID: {skuId}");
                }
            }
        }

        // Lớp hỗ trợ để lưu thông tin chi tiết sản phẩm
        public class ProductDetail
        {
            public int? ProductId { get; set; }
            public string ProductName { get; set; }
            public string ColorName { get; set; }
            public string SizeName { get; set; }
            public string MaterialName { get; set; }
            public int? Quantity { get; set; }
            public int? SubProductId { get; set; }
            public double DiscountedPrice { get; set; }
        }
    }
}