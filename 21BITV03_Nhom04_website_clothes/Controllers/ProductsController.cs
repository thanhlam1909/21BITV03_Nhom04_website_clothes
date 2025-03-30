using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class ProductsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public ProductsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new ManageProduct
            {
                Product = new Product(),
                AvailableProductTypes = _context.ProductTypes.ToList()
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ManageProduct model)
        {
            // Nếu chưa chọn loại sản phẩm
            if (model.SelectedProductTypeIds == null || !model.SelectedProductTypeIds.Any())
            {
                ModelState.AddModelError("SelectedProductTypeIds", "Vui lòng chọn ít nhất một loại sản phẩm.");
            }

            // In lỗi ra console (nếu có)
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    var key = state.Key;
                    var errors = state.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"ModelState Error for {key}: {error.ErrorMessage}");
                    }

                }

                // Nếu có lỗi → load lại danh sách loại sản phẩm để hiển thị lại View
                model.AvailableProductTypes = _context.ProductTypes.ToList();
                return View(model);
            }

            // Nếu hợp lệ thì lưu sản phẩm
            var newProduct = model.Product;
            newProduct.DeleteStatus = false;
            newProduct.DeletionDate = null;

            _context.Products.Add(newProduct);
            _context.SaveChanges();

            foreach (var typeId in model.SelectedProductTypeIds)
            {
                var link = new ProductTypeLink
                {
                    ProductId = newProduct.ProductId,
                    ProductTypeId = typeId
                };
                _context.ProductTypeLinks.Add(link);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }





        // GET: Products/Edit/5
        // GET: Products/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách ProductTypeId đang liên kết với sản phẩm
            var selectedTypeIds = _context.ProductTypeLinks
                .Where(l => l.ProductId == product.ProductId)
                .Select(l => l.ProductTypeId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();


            var model = new ManageProduct
            {
                Product = product,
                AvailableProductTypes = _context.ProductTypes.ToList(),
                SelectedProductTypeIds = selectedTypeIds
            };

            return View(model);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ManageProduct model)
        {
            if (id != model.Product.ProductId)
            {
                return NotFound();
            }

            // Kiểm tra nếu chưa chọn loại sản phẩm
            if (model.SelectedProductTypeIds == null || !model.SelectedProductTypeIds.Any())
            {
                ModelState.AddModelError("SelectedProductTypeIds", "Vui lòng chọn ít nhất một loại sản phẩm.");
            }

            if (!ModelState.IsValid)
            {
                // In lỗi ra console (giúp debug)
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error - {state.Key}: {error.ErrorMessage}");
                    }
                }

                model.AvailableProductTypes = _context.ProductTypes.ToList();
                return View(model);
            }

            // Cập nhật thông tin sản phẩm
            var existingProduct = _context.Products.Find(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.ProductName = model.Product.ProductName;
            existingProduct.Description = model.Product.Description;
            existingProduct.DeleteStatus = model.Product.DeleteStatus;
            existingProduct.DeletionDate = model.Product.DeletionDate;

            _context.Update(existingProduct);

            // Xóa các liên kết cũ trong ProductTypeLink
            var oldLinks = _context.ProductTypeLinks.Where(l => l.ProductId == id);
            _context.ProductTypeLinks.RemoveRange(oldLinks);

            // Thêm lại các liên kết mới
            foreach (var typeId in model.SelectedProductTypeIds)
            {
                _context.ProductTypeLinks.Add(new ProductTypeLink
                {
                    ProductId = id,
                    ProductTypeId = typeId
                });
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
