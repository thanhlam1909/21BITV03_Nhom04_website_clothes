using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;  // namespace chứa DbContext và entity Discount
using System.Threading.Tasks;
using System;
using _21BITV03_Nhom04_website_clothes.Models;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class DisCountController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public DisCountController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách khuyến mãi
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách Discount bao gồm bảng liên kết và thông tin sản phẩm
            var discounts = await _context.Discounts
                .Include(d => d.DiscountedProductLists)
                    .ThenInclude(dp => dp.Product)
                .ToListAsync();

            // Mapping sang ViewModel
            var viewModels = discounts.Select(d => new DiscountViewModel
            {
                Discount = d,
                Products = new List<Product>(), // không cần thiết cho Index, có thể bỏ
                SelectedProductIds = new List<int>() // cũng không cần, để tránh null
            }).ToList();

            return View(viewModels);
        }


        // GET: Create form + danh sách sản phẩm để chọn
        public IActionResult Create()
        {
            var vm = new DiscountViewModel
            {
                Discount = new Discount(),
                Products = _context.Products.ToList(),
                SelectedProductIds = new List<int>()
            };
            return View(vm);
        }

        // POST: Tạo mới Discount + liên kết sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountViewModel vm)
        {
            if (vm == null)
            {
                // Nếu vm null thì trả về View với dữ liệu mặc định (hoặc lỗi)
                vm = new DiscountViewModel
                {
                    Discount = new Discount(),
                    Products = _context.Products.ToList(),
                    SelectedProductIds = new List<int>()
                };
                return View(vm);
            }

            // Kiểm tra ngày bắt đầu không được lớn hơn ngày kết thúc
            if (vm.Discount.StartTime.HasValue && vm.Discount.EndTime.HasValue && vm.Discount.StartTime > vm.Discount.EndTime)
            {
                ModelState.AddModelError(string.Empty, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
                vm.Products = _context.Products.ToList(); // Load lại sản phẩm để hiển thị lại View
                return View(vm);
            }

            if (vm != null)
            {
                // Thêm Discount
                _context.Discounts.Add(vm.Discount);

                // Lưu Discount trước để lấy DiscountId
                await _context.SaveChangesAsync();

                // Thêm liên kết sản phẩm với discount
                if (vm.SelectedProductIds != null && vm.SelectedProductIds.Any())
                {
                    foreach (var productId in vm.SelectedProductIds)
                    {
                        _context.DiscountedProductLists.Add(new DiscountedProductList
                        {
                            DiscountId = vm.Discount.DiscountId,
                            ProductId = productId
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, load lại sản phẩm
            vm.Products = _context.Products.ToList();
            return View(vm);
        }

        // GET: Edit form với dữ liệu hiện có (discount + sản phẩm được chọn)
        // GET: Edit form với dữ liệu hiện có (discount + sản phẩm được chọn)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            var productIds = await _context.DiscountedProductLists
                .Where(d => d.DiscountId == id)
                .Select(d => d.ProductId ?? 0)
                .ToListAsync();

            var vm = new DiscountViewModel
            {
                Discount = discount,
                Products = await _context.Products.ToListAsync(),
                SelectedProductIds = productIds
            };

            return View(vm);
        }

        // POST: Lưu thay đổi Discount + cập nhật danh sách sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiscountViewModel vm)
        {
            if (id != vm.Discount.DiscountId) return NotFound();

            // Kiểm tra ngày bắt đầu không được lớn hơn ngày kết thúc
            if (vm.Discount.StartTime.HasValue && vm.Discount.EndTime.HasValue && vm.Discount.StartTime > vm.Discount.EndTime)
            {
                ModelState.AddModelError(string.Empty, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
                vm.Products = await _context.Products.ToListAsync();
                return View(vm);
            }

            if (vm !=null)
            {
                try
                {
                    _context.Update(vm.Discount);
                    await _context.SaveChangesAsync();

                    // Xóa hết liên kết sản phẩm cũ
                    var oldLinks = _context.DiscountedProductLists.Where(d => d.DiscountId == id);
                    _context.DiscountedProductLists.RemoveRange(oldLinks);

                    // Thêm liên kết sản phẩm mới nếu có
                    if (vm.SelectedProductIds != null && vm.SelectedProductIds.Any())
                    {
                        foreach (var productId in vm.SelectedProductIds)
                        {
                            _context.DiscountedProductLists.Add(new DiscountedProductList
                            {
                                DiscountId = vm.Discount.DiscountId,
                                ProductId = productId
                            });
                        }
                    }
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscountExists(vm.Discount.DiscountId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Nếu ModelState không hợp lệ, load lại sản phẩm để trả về View
            vm.Products = await _context.Products.ToListAsync();
            return View(vm);
        }


        // Xóa discount (giữ nguyên như trước, tự động xóa liên kết qua cascade hoặc xóa tay)
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null) return NotFound();

        //    var discount = await _context.Discounts
        //        .FirstOrDefaultAsync(m => m.DiscountId == id);
        //    if (discount == null) return NotFound();

        //    return View(discount);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                // Xóa liên kết sản phẩm trước (nếu không cascade)
                var links = _context.DiscountedProductLists.Where(d => d.DiscountId == id);
                _context.DiscountedProductLists.RemoveRange(links);

                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.DiscountId == id);
        }
    }
}
