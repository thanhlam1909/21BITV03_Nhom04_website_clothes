using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class InventoryTransactionDetailsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public InventoryTransactionDetailsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: InventoryTransactionDetails
        public async Task<IActionResult> Index(int? transactionId)
        {
            if (transactionId == null)
            {
                return NotFound();
            }

            var transaction = await _context.InventoryTransactions
                .Include(t => t.Warehouse)
                .Include(t => t.Supplier)
                .Include(t => t.InventoryTransactionDetails)
                    .ThenInclude(d => d.SubProduct)
                        .ThenInclude(sp => sp.MainProduct)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                return NotFound();
            }

            var details = await _context.InventoryTransactionDetails
                .Where(d => d.TransactionId == transactionId)
                .Include(d => d.SubProduct)
                .ToListAsync();

            // Tạo ViewData cho SubProducts với tên hiển thị đầy đủ
            var subProducts = _context.SubProducts
                .Include(sp => sp.MainProduct)
                .Include(sp => sp.Color)
                .Include(sp => sp.Size)
                .Include(sp => sp.Material)
                .ToList()
                .Select(sp => new
                {
                    SubProductId = sp.SubProductId,
                    DisplayName = $"{sp.MainProduct?.ProductName ?? "Không rõ"} - {sp.Color?.ColorName ?? "Không rõ"} - {sp.Size?.SizeName ?? "Không rõ"} - {sp.Material?.MaterialName ?? "Không rõ"}"
                });

            ViewData["SubProductId"] = new SelectList(subProducts, "SubProductId", "DisplayName");
            ViewBag.TransactionId = transactionId;

            return View(transaction.InventoryTransactionDetails.ToList());
        }


        // GET: InventoryTransactionDetails/Details/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("TransactionDetailId,TransactionId,SubProductId,Quantity,UnitPrice")] InventoryTransactionDetail inventoryTransactionDetail)
        //{
        //    if (inventoryTransactionDetail!=null)
        //    { 

        //        _context.Add(inventoryTransactionDetail);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index), new { transactionId = inventoryTransactionDetail.TransactionId });
        //    }

        //    // Nếu validation fail, load lại dropdown với giá trị đã chọn
        //    ViewData["SubProductId"] = new SelectList(_context.SubProducts, "SubProductId", "SubProductId", inventoryTransactionDetail.SubProductId);
        //    ViewData["TransactionId"] = new SelectList(_context.InventoryTransactions, "TransactionId", "TransactionId", inventoryTransactionDetail.TransactionId);
        //    ViewBag.TransactionId = inventoryTransactionDetail.TransactionId;
        //    return RedirectToAction(nameof(Index), new { transactionId = inventoryTransactionDetail.TransactionId });
        //}
        // GET: InventoryTransactionDetails/Create
        public IActionResult Create(int? transactionId)
        {
            var subProducts = _context.SubProducts
                .Include(sp => sp.MainProduct)
                .Include(sp => sp.Color)
                .Include(sp => sp.Size)
                .Include(sp => sp.Material)
                .ToList()
                .Select(sp => new
                {
                    SubProductId = sp.SubProductId,
                    DisplayName = $"{sp.MainProduct?.ProductName ?? "Không rõ"} - {sp.Color?.ColorName ?? "Không rõ"} - {sp.Size?.SizeName ?? "Không rõ"} - {sp.Material?.MaterialName ?? "Không rõ"}"
                });

            ViewData["SubProductId"] = new SelectList(subProducts, "SubProductId", "DisplayName");

            ViewData["TransactionId"] = new SelectList(_context.InventoryTransactions, "TransactionId", "TransactionId", transactionId);
            ViewBag.TransactionId = transactionId;

            return View(new InventoryTransactionDetail { TransactionId = transactionId ?? 0 });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionDetailId,TransactionId,SubProductId,Quantity,UnitPrice")] InventoryTransactionDetail inventoryTransactionDetail)
        {
            if (inventoryTransactionDetail != null)
            {
                _context.Add(inventoryTransactionDetail);
                await _context.SaveChangesAsync();

                // Sau khi thêm chi tiết giao dịch, kiểm tra xem giao dịch này là loại nhập hàng
                var transaction = await _context.InventoryTransactions
                    .Include(t => t.Warehouse)
                    .FirstOrDefaultAsync(t => t.TransactionId == inventoryTransactionDetail.TransactionId);

                if (transaction != null &&
                   (transaction.TransactionType?.ToLower() == "nhập hàng" || transaction.TransactionType?.ToLower() == "import"))
                {
                    // Lấy thông tin SKU từ SubProduct
                    var subProduct = await _context.SubProducts
                        .Include(sp => sp.Sku)
                        .FirstOrDefaultAsync(sp => sp.SubProductId == inventoryTransactionDetail.SubProductId);

                    if (subProduct != null && subProduct.SkuId.HasValue)
                    {
                        var skuId = subProduct.SkuId.Value;

                        // Tìm WarehouseProduct tương ứng
                        var warehouseProduct = await _context.WarehouseProducts
                            .FirstOrDefaultAsync(wp => wp.SkuId == skuId && wp.WarehouseId == transaction.WarehouseId);

                        if (warehouseProduct != null)
                        {
                            // Cộng thêm số lượng
                            warehouseProduct.Quantity += inventoryTransactionDetail.Quantity;
                            warehouseProduct.LastUpdated = DateTime.Now;
                        }
                        else
                        {
                            // Nếu chưa có, tạo mới bản ghi WarehouseProduct
                            _context.WarehouseProducts.Add(new WarehouseProduct
                            {
                                SkuId = skuId,
                                WarehouseId = transaction.WarehouseId,
                                Quantity = inventoryTransactionDetail.Quantity,
                                LastUpdated = DateTime.Now
                            });
                        }
                        var newBatch = new WarehouseBatch
                        {
                            SkuId = skuId,
                            WarehouseId = transaction.WarehouseId,
                            SupplierId = transaction.SupplierId ?? 0, // Giả định InventoryTransaction có SupplierId
                            QuantityImported = inventoryTransactionDetail.Quantity,
                            QuantityAvailable = inventoryTransactionDetail.Quantity,
                            ImportDate = (DateTime)transaction.TransactionDate,
                            UnitCost = inventoryTransactionDetail.UnitPrice ?? 0m,
                            Note = $"Tạo từ giao dịch số {transaction.TransactionId}"
                        };

                        _context.WarehouseBatches.Add(newBatch);

                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index), new { transactionId = inventoryTransactionDetail.TransactionId });
            }

            // Nếu validation fail, load lại dropdown với giá trị đã chọn
            ViewData["SubProductId"] = new SelectList(_context.SubProducts, "SubProductId", "SubProductId", inventoryTransactionDetail.SubProductId);
            ViewData["TransactionId"] = new SelectList(_context.InventoryTransactions, "TransactionId", "TransactionId", inventoryTransactionDetail.TransactionId);
            ViewBag.TransactionId = inventoryTransactionDetail.TransactionId;
            return View(inventoryTransactionDetail); // Fix: Không Redirect nếu lỗi mà nên return View
        }

        // GET: InventoryTransactionDetails/Edit/5
        public async Task<IActionResult> Edit(int? id, int transactionId)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.TransactionId = transactionId;

            var inventoryTransactionDetail = await _context.InventoryTransactionDetails.FindAsync(id);
            if (inventoryTransactionDetail == null)
            {
                return NotFound();
            }

            // Load dữ liệu SubProduct kèm thông tin liên quan
            var subProducts = await _context.SubProducts
                .Include(sp => sp.MainProduct)
                .Include(sp => sp.Color)
                .Include(sp => sp.Size)
                .Include(sp => sp.Material)
                .ToListAsync();

            var subProductSelectList = subProducts.Select(sp => new
            {
                SubProductId = sp.SubProductId,
                DisplayName = $"{sp.MainProduct?.ProductName ?? "Không rõ"} - {sp.Color?.ColorName ?? "Không rõ"} - {sp.Size?.SizeName ?? "Không rõ"} - {sp.Material?.MaterialName ?? "Không rõ"}"
            });

            ViewData["SubProductId"] = new SelectList(subProductSelectList, "SubProductId", "DisplayName", inventoryTransactionDetail.SubProductId);
            ViewData["TransactionId"] = new SelectList(_context.InventoryTransactions, "TransactionId", "TransactionId", inventoryTransactionDetail.TransactionId);

            return View(inventoryTransactionDetail);
        }


        // POST: InventoryTransactionDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionDetailId,TransactionId,SubProductId,Quantity,UnitPrice")] InventoryTransactionDetail inventoryTransactionDetail)
        {
            if (id != inventoryTransactionDetail.TransactionDetailId)
            {
                return NotFound();
            }

            if (inventoryTransactionDetail != null)
            {
                try
                {
                    _context.Update(inventoryTransactionDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryTransactionDetailExists(inventoryTransactionDetail.TransactionDetailId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { transactionId = inventoryTransactionDetail.TransactionId });
            }
            ViewData["SubProductId"] = new SelectList(_context.SubProducts, "SubProductId", "SubProductId", inventoryTransactionDetail.SubProductId);
            ViewData["TransactionId"] = new SelectList(_context.InventoryTransactions, "TransactionId", "TransactionId", inventoryTransactionDetail.TransactionId);
            return View(inventoryTransactionDetail);
        }

        // GET: InventoryTransactionDetails/Delete/5
        // GET: InventoryTransactionDetails/Delete/5
        public async Task<IActionResult> Delete(int? id, int transactionId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detail = await _context.InventoryTransactionDetails
                .Include(d => d.SubProduct)
                .FirstOrDefaultAsync(m => m.TransactionDetailId == id);

            if (detail == null)
            {
                return NotFound();
            }

            ViewBag.TransactionId = transactionId;
            return View(detail);
        }

        // POST: InventoryTransactionDetails/Delete/5
        // POST: InventoryTransactionDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detail = await _context.InventoryTransactionDetails.FindAsync(id);
            if (detail != null)
            {
                int transactionId = detail.TransactionId; // 👈 Lưu lại transactionId trước khi xóa

                _context.InventoryTransactionDetails.Remove(detail);
                await _context.SaveChangesAsync();

                // 🔥 Sau khi xóa, quay lại Index với transactionId
                return RedirectToAction(nameof(Index), new { transactionId = transactionId });
            }

            return NotFound();
        }


        private bool InventoryTransactionDetailExists(int id)
        {
            return _context.InventoryTransactionDetails.Any(e => e.TransactionDetailId == id);
        }
    }
}
