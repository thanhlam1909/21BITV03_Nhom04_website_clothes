using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;
using ClosedXML.Excel;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class InventoryTransactionsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public InventoryTransactionsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: InventoryTransactions
        public async Task<IActionResult> Index()
        {
            var websiteClothesContext = _context.InventoryTransactions.Include(i => i.Supplier).Include(i => i.Warehouse);
            return View(await websiteClothesContext.ToListAsync());
        }

        // GET: InventoryTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryTransaction = await _context.InventoryTransactions
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (inventoryTransaction == null)
            {
                return NotFound();
            }

            return View(inventoryTransaction);
        }

        // GET: InventoryTransactions/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName");
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName");
            ViewData["TransactionTypeList"] = new SelectList(new[] { "Nhập hàng", "Xuất hàng", "Điều chuyển" });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,TransactionType,TransactionDate,WarehouseId,SupplierId,Notes")] InventoryTransaction inventoryTransaction)
        {
            if (inventoryTransaction!=null)
            {

                    inventoryTransaction.TransactionDate = DateTime.Now;
 

                _context.Add(inventoryTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", inventoryTransaction.SupplierId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName", inventoryTransaction.WarehouseId);
            ViewData["TransactionTypeList"] = new SelectList(new[] { "Nhập hàng", "Xuất hàng", "Điều chuyển" }, inventoryTransaction.TransactionType);
            return View(inventoryTransaction);
        }


        // GET: InventoryTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryTransaction = await _context.InventoryTransactions.FindAsync(id);
            if (inventoryTransaction == null)
            {
                return NotFound();
            }
            ViewData["TransactionTypeList"] = new SelectList(new[] { "Nhập hàng", "Xuất hàng", "Điều chuyển" });

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", inventoryTransaction.SupplierId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName", inventoryTransaction.WarehouseId);
            return View(inventoryTransaction);
        }

        // POST: InventoryTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,TransactionType,TransactionDate,WarehouseId,SupplierId,Notes")] InventoryTransaction inventoryTransaction)
        {
            if (id != inventoryTransaction.TransactionId)
            {
                return NotFound();
            }

            if (inventoryTransaction != null)
            {
                try
                {
                    inventoryTransaction.TransactionDate = DateTime.Now;

                    _context.Update(inventoryTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryTransactionExists(inventoryTransaction.TransactionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", inventoryTransaction.SupplierId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName", inventoryTransaction.WarehouseId);
            ViewData["TransactionTypeList"] = new SelectList(new[] { "Nhập hàng", "Xuất hàng", "Điều chuyển" }, inventoryTransaction.TransactionType);

            return View(inventoryTransaction);
        }

        // GET: InventoryTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryTransaction = await _context.InventoryTransactions
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (inventoryTransaction == null)
            {
                return NotFound();
            }

            return View(inventoryTransaction);
        }

        // POST: InventoryTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventoryTransaction = await _context.InventoryTransactions.FindAsync(id);
            if (inventoryTransaction != null)
            {
                _context.InventoryTransactions.Remove(inventoryTransaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel.");
                return View();
            }

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Bỏ header

            // Giả định: Tất cả các dòng thuộc cùng một giao dịch
            var firstRow = rows.First();

            string transactionType = firstRow.Cell(1).GetString().Trim();
            DateTime transactionDate = firstRow.Cell(2).GetDateTime();
            string supplierName = firstRow.Cell(6).GetString().Trim();
            string supplierEmail = firstRow.Cell(7).GetString().Trim();
            string warehouseName = firstRow.Cell(8).GetString().Trim();
            string Notetransaction = firstRow.Cell(9).GetString().Trim();

            // Tìm hoặc tạo Supplier
            var supplier = _context.Suppliers.FirstOrDefault(s => s.SupplierName == supplierName && s.Email == supplierEmail);

            if (supplier == null)
            {
                throw new Exception($"Không tìm thấy nhà cung cấp [{supplierName} - {supplierEmail}].");
            }
            // Tìm Warehouse
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseName == warehouseName);
            if (warehouse == null)
            {
                ModelState.AddModelError("", $"Không tìm thấy kho: {warehouseName}");
                return View();
            }

            // Tạo InventoryTransaction
            var transaction = new InventoryTransaction
            {
                TransactionType = transactionType,
                TransactionDate = transactionDate,
                WarehouseId = warehouse.WarehouseId,
                SupplierId = supplier.SupplierId,
                Notes = Notetransaction
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            foreach (var row in rows)
            {
                string skuCode = row.Cell(3).GetString().Trim();
                int quantity = int.TryParse(row.Cell(4).GetString().Trim(), out var q) ? q : 0;
                decimal? unitPrice = decimal.TryParse(row.Cell(5).GetString().Trim(), out var price) ? price : null;

                // Tìm SkuId từ SkuCode
                var sku = await _context.Skus.FirstOrDefaultAsync(s => s.SkuCode == skuCode);
                if (sku == null)
                {
                    ModelState.AddModelError("", $"Không tìm thấy Sku: {skuCode}");
                    continue;
                }
                var skuId = sku.SkuId;
                // Tìm SubProduct theo SkuId
                var subProduct = await _context.SubProducts.FirstOrDefaultAsync(sp => sp.SkuId == sku.SkuId);
                if (subProduct == null)
                {
                    ModelState.AddModelError("", $"Không tìm thấy SubProduct cho SkuId: {sku.SkuId}");
                    continue;
                }


                var detail = new InventoryTransactionDetail
                {
                    TransactionId = transaction.TransactionId,
                    SubProductId = subProduct.SubProductId,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                };

                _context.InventoryTransactionDetails.Add(detail);

                // Cập nhật WarehouseProduct
                var warehouseProduct = await _context.WarehouseProducts
                    .FirstOrDefaultAsync(wp => wp.SkuId == skuId && wp.WarehouseId == warehouse.WarehouseId);

                if (warehouseProduct != null)
                {
                    warehouseProduct.Quantity += quantity;
                    warehouseProduct.LastUpdated = DateTime.Now;
                }
                else
                {
                    _context.WarehouseProducts.Add(new WarehouseProduct
                    {
                        SkuId = skuId,
                        WarehouseId = warehouse.WarehouseId,
                        Quantity = quantity,
                        LastUpdated = DateTime.Now
                    });
                }

                // Tạo WarehouseBatch
                _context.WarehouseBatches.Add(new WarehouseBatch
                {
                    SkuId = skuId,
                    WarehouseId = warehouse.WarehouseId,
                    SupplierId = supplier.SupplierId,
                    QuantityImported = quantity,
                    QuantityAvailable = quantity,
                    ImportDate = transactionDate,
                    UnitCost = unitPrice ?? 0,
                    Note = $"Tạo từ giao dịch Excel {transaction.TransactionId}"
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryTransactionExists(int id)
        {
            return _context.InventoryTransactions.Any(e => e.TransactionId == id);
        }
    }
}
