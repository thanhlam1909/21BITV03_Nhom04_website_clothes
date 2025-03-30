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
    public class SubProductsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public SubProductsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: SubProducts
        public async Task<IActionResult> Index(int productId)
        {
            var product = await _context.Products
                           .Include(p => p.SubProducts)
                               .ThenInclude(sp => sp.Color)
                           .Include(p => p.SubProducts)
                               .ThenInclude(sp => sp.Size)
                           .Include(p => p.SubProducts)
                               .ThenInclude(sp => sp.Material)
                           .Include(p => p.SubProducts)
                               .ThenInclude(sp => sp.Sku)
                           .FirstOrDefaultAsync(m => m.ProductId == productId);


            if (product == null)
            {
                return NotFound();
            }
            ViewData["MainProductId"] = productId;

            // Pass the product ID to the View for better context
            ViewBag.MainProductId = product.ProductId;


            return View(product.SubProducts.ToList());
        }

        // GET: SubProducts/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var subProduct = await _context.SubProducts
        //        .Include(s => s.Color)
        //        .Include(s => s.MainProduct)
        //        .Include(s => s.Material)
        //        .Include(s => s.Size)
        //        .Include(s => s.Sku)
        //        .FirstOrDefaultAsync(m => m.SubProductId == id);
        //    if (subProduct == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(subProduct);
        //}

        // GET: SubProducts/Create
        public IActionResult Create(int? productId)
        {
            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName");
            ViewData["MaterialId"] = new SelectList(_context.Materials, "MaterialId", "MaterialName");
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "SizeName");
            ViewData["MainProductId"] = productId;


            return View();
        }


        // POST: SubProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string skuCode, [Bind("SubProductId,MainProductId,OriginalPrice,DiscountedPrice,ColorId,SizeId,CreationDate,Linkimage,Status,MaterialId")] SubProduct subProduct, IFormFile imageFile)
        {
            // Kiểm tra ModelState
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Product_img/", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    subProduct.Linkimage = Path.Combine("/Product_img/", fileName);
                }

                // 1. Kiểm tra xem SkuCode đã tồn tại chưa
                var existingSku = await _context.Skus.FirstOrDefaultAsync(s => s.SkuCode == skuCode);
                if (existingSku != null)
                {
                    ModelState.AddModelError("skuCode", "Mã SKU này đã tồn tại. Vui lòng chọn mã khác.");
                    return View(subProduct);
                }

                // 2. Kiểm tra xem biến thể với tổ hợp MainProductId, ColorId, SizeId, MaterialId đã tồn tại chưa
                var existingVariant = await _context.SubProducts
                    .FirstOrDefaultAsync(sp => sp.MainProductId == subProduct.MainProductId &&
                                               sp.ColorId == subProduct.ColorId &&
                                               sp.SizeId == subProduct.SizeId &&
                                               sp.MaterialId == subProduct.MaterialId);
                if (existingVariant != null)
                {
                    ModelState.AddModelError("", "Biến thể với tổ hợp sản phẩm chính, màu, kích thước và chất liệu này đã tồn tại.");
                    return View(subProduct);
                }

                // Nếu không trùng, tiến hành tạo mới
                // Tạo SKU mới từ input
                var newSku = new Sku { SkuCode = skuCode };
                _context.Skus.Add(newSku);
                await _context.SaveChangesAsync();

                subProduct.SkuId = newSku.SkuId;
                subProduct.CreationDate = DateTime.Now;

                _context.SubProducts.Add(subProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { productId = subProduct.MainProductId });
            }

            // Nếu ModelState không hợp lệ, trả về view với dữ liệu dropdown
            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName", subProduct.ColorId);
            ViewData["MaterialId"] = new SelectList(_context.Materials, "MaterialId", "MaterialName", subProduct.MaterialId);
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "ProductSizeName", subProduct.SizeId);
            return View(subProduct);
        }
/*        // GET: SubProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subProduct = await _context.SubProducts
                .Include(s => s.Sku) // include thêm để lấy được skuCode
                .FirstOrDefaultAsync(s => s.SubProductId == id);
            if (subProduct == null) return NotFound();
            if (subProduct.CreationDate == default(DateTime))
            {
                subProduct.CreationDate = DateTime.Now;
            }
            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName", subProduct.ColorId);
            ViewData["MaterialId"] = new SelectList(_context.Materials, "MaterialId", "MaterialName", subProduct.MaterialId);
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "ProductSizeName", subProduct.SizeId);
            ViewBag.MainProductId = subProduct.MainProductId;
            ViewBag.SkuCode = subProduct.Sku?.SkuCode; // để hiển thị lên View

            return View(subProduct);
        }*/
        // GET: SubProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subProduct = await _context.SubProducts
                .Include(s => s.Sku) // Include để lấy SkuCode
                .FirstOrDefaultAsync(s => s.SubProductId == id);
            if (subProduct == null) return NotFound();

            // Đặt CreationDate mặc định nếu chưa có
            if (subProduct.CreationDate == default(DateTime))
            {
                subProduct.CreationDate = DateTime.Now;
            }

            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName", subProduct.ColorId);
            ViewData["MaterialId"] = new SelectList(_context.Materials, "MaterialId", "MaterialName", subProduct.MaterialId);
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "SizeName", subProduct.SizeId);
            ViewBag.MainProductId = subProduct.MainProductId;
            ViewBag.SkuCode = subProduct.Sku?.SkuCode; // Truyền SkuCode lên view

            return View(subProduct);
        }



        // POST: SubProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string skuCode, int id, [Bind("SubProductId,MainProductId,OriginalPrice,DiscountedPrice,ColorId,SizeId,CreationDate,Status,MaterialId")] SubProduct subProduct, IFormFile? imageFile)
        {
            if (id != subProduct.SubProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy SubProduct hiện tại từ DB kèm Sku
                    var existingSubProduct = await _context.SubProducts
                        .Include(sp => sp.Sku)
                        .FirstOrDefaultAsync(sp => sp.SubProductId == id);
                    if (existingSubProduct == null) return NotFound();

                    // 1. Kiểm tra xem SkuCode có trùng với bản ghi khác không (ngoại trừ bản ghi hiện tại)
                    var existingSku = await _context.Skus
                        .FirstOrDefaultAsync(s => s.SkuCode == skuCode && s.SkuId != existingSubProduct.SkuId);
                    if (existingSku != null)
                    {
                        ModelState.AddModelError("skuCode", "Mã SKU này đã tồn tại. Vui lòng chọn mã khác.");
                        return View(subProduct);
                    }

                    // 2. Kiểm tra xem tổ hợp biến thể có trùng với bản ghi khác không (ngoại trừ bản ghi hiện tại)
                    var existingVariant = await _context.SubProducts
                        .FirstOrDefaultAsync(sp => sp.MainProductId == subProduct.MainProductId &&
                                                   sp.ColorId == subProduct.ColorId &&
                                                   sp.SizeId == subProduct.SizeId &&
                                                   sp.MaterialId == subProduct.MaterialId &&
                                                   sp.SubProductId != subProduct.SubProductId);
                    if (existingVariant != null)
                    {
                        ModelState.AddModelError("", "Biến thể với tổ hợp sản phẩm chính, màu, kích thước và chất liệu này đã tồn tại.");
                        return View(subProduct);
                    }
                    if (imageFile != null)
                    {
                        // Lưu ảnh mới
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Product_img/", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        existingSubProduct.Linkimage = Path.Combine("/Product_img/", fileName);
                    }
                    else
                    {
                        // Giữ lại ảnh cũ nếu không có ảnh mới
                        existingSubProduct.Linkimage = existingSubProduct.Linkimage;
                    }



                    // 4. Cập nhật các thuộc tính SubProduct
                    existingSubProduct.OriginalPrice = subProduct.OriginalPrice;
                    existingSubProduct.DiscountedPrice = subProduct.DiscountedPrice;
                    existingSubProduct.ColorId = subProduct.ColorId;
                    existingSubProduct.SizeId = subProduct.SizeId;
                    existingSubProduct.CreationDate = subProduct.CreationDate;
                    existingSubProduct.Status = subProduct.Status;
                    existingSubProduct.MaterialId = subProduct.MaterialId;

                    // 5. Cập nhật SkuCode nếu thay đổi
                    if (existingSubProduct.Sku != null && existingSubProduct.Sku.SkuCode != skuCode)
                    {
                        existingSubProduct.Sku.SkuCode = skuCode;
                    }

                    _context.Update(existingSubProduct);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index), new { productId = subProduct.MainProductId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubProductExists(subProduct.SubProductId)) return NotFound();
                    else throw;
                }
            }

            // Nếu ModelState không hợp lệ, trả về view với dữ liệu dropdown
            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName", subProduct.ColorId);
            ViewData["MaterialId"] = new SelectList(_context.Materials, "MaterialId", "MaterialName", subProduct.MaterialId);
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "SizeName", subProduct.SizeId);
            ViewBag.MainProductId = subProduct.MainProductId;
            ViewBag.SkuCode = skuCode; // Truyền lại SkuCode để hiển thị

            return View(subProduct);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var subProduct = await _context.SubProducts
                .Include(s => s.Color)
                .Include(s => s.MainProduct)
                .Include(s => s.Material)
                .Include(s => s.Size)
                .Include(s => s.Sku)
                .FirstOrDefaultAsync(m => m.SubProductId == id);

            if (subProduct == null) return NotFound();

            ViewBag.MainProductId = subProduct.MainProductId;
            return View(subProduct);
        }
        // POST: SubProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subProduct = await _context.SubProducts.FindAsync(id);
            int productId = subProduct?.MainProductId ?? 0;

            if (subProduct != null)
            {
                _context.SubProducts.Remove(subProduct);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { productId = productId });
        }


        private bool SubProductExists(int id)
        {
            return _context.SubProducts.Any(e => e.SubProductId == id);
        }
    }
}
