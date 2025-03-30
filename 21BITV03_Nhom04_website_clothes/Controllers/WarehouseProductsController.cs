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
    public class WarehouseProductsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public WarehouseProductsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? warehouseId)
        {
            var query = _context.WarehouseProducts
                                .Include(wp => wp.Sku)
                                .Include(wp => wp.Warehouse)
                                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(wp => wp.WarehouseId == warehouseId.Value);
            }

            return View(await query.ToListAsync());
        }

        // GET: WarehouseProducts/Create
        public IActionResult Create()
        {
            ViewData["SkuId"] = new SelectList(_context.Skus, "SkuId", "SkuId");
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseId");
            return View();
        }

        // POST: WarehouseProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WarehouseProductId,WarehouseId,Quantity,LastUpdated,SkuId")] WarehouseProduct warehouseProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(warehouseProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SkuId"] = new SelectList(_context.Skus, "SkuId", "SkuId", warehouseProduct.SkuId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseId", warehouseProduct.WarehouseId);
            return View(warehouseProduct);
        }

        // GET: WarehouseProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouseProduct = await _context.WarehouseProducts.FindAsync(id);
            if (warehouseProduct == null)
            {
                return NotFound();
            }
            ViewData["SkuId"] = new SelectList(_context.Skus, "SkuId", "SkuId", warehouseProduct.SkuId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseId", warehouseProduct.WarehouseId);
            ViewData["CurrentWarehouseId"] = warehouseProduct.WarehouseId;

            return View(warehouseProduct);
        }

        // POST: WarehouseProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WarehouseProductId,WarehouseId,Quantity,LastUpdated,SkuId")] WarehouseProduct warehouseProduct)
        {
            if (id != warehouseProduct.WarehouseProductId)
            {
                return NotFound();
            }

            if (warehouseProduct !=null)
            {
                try
                {
                    _context.Update(warehouseProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarehouseProductExists(warehouseProduct.WarehouseProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { warehouseId = warehouseProduct.WarehouseId });
            }
            ViewData["SkuId"] = new SelectList(_context.Skus, "SkuId", "SkuId", warehouseProduct.SkuId);
            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseId", warehouseProduct.WarehouseId);
            return View(warehouseProduct);
        }

        // GET: WarehouseProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouseProduct = await _context.WarehouseProducts
                .Include(w => w.Sku)
                .Include(w => w.Warehouse)
                .FirstOrDefaultAsync(m => m.WarehouseProductId == id);
            if (warehouseProduct == null)
            {
                return NotFound();
            }
            ViewData["CurrentWarehouseId"] = warehouseProduct.WarehouseId;

            return View(warehouseProduct);
        }

        // POST: WarehouseProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouseProduct = await _context.WarehouseProducts.FindAsync(id);
            int? warehouseId = null;
            if (warehouseProduct != null)
            {
                warehouseId = warehouseProduct.WarehouseId;
                _context.WarehouseProducts.Remove(warehouseProduct);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { warehouseId = warehouseId });
        }

        private bool WarehouseProductExists(int id)
        {
            return _context.WarehouseProducts.Any(e => e.WarehouseProductId == id);
        }
    }
}
