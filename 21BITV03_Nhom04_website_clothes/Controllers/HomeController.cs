using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Helper;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class HomeController : Controller
    {
        /*        private readonly ILogger<HomeController> _logger;

                public HomeController(ILogger<HomeController> logger)
                {
                    _logger = logger;
                }*/

        private readonly WebsiteClothesContext _context;
        public HomeController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            var productTypes = _context.ProductTypes
                    .Include(pt => pt.ProductTypeLinks)
                        .ThenInclude(ptl => ptl.Product)
                            .ThenInclude(p => p.SubProducts)
                    .Where(pt => pt.ProductTypeLinks.Any(ptl => ptl.Product.SubProducts.Any()))
                    .ToList();

            // Mapping to ViewModel
            var productTypeViewModels = productTypes.Select(pt => new ProductTypeViewModel
            {
                ProductTypeId = pt.ProductTypeId,
                ProductTypeName = pt.ProductTypeName,
                Products = pt.ProductTypeLinks
                    .Where(ptl => ptl.Product != null && ptl.Product.SubProducts.Any())
                    .Select(ptl => new ListProductTypeViewModel
                    {
                        ProductId = ptl.Product.ProductId,
                        ProductName = ptl.Product.ProductName,
                        Description = ptl.Product.Description,
                        SubProducts = ptl.Product.SubProducts.Select(sp => new SubProductTypeViewModel
                        {
                            SubProductId = sp.SubProductId,
                            MainProductId = sp.MainProductId ?? 0,
                            OriginalPrice = sp.OriginalPrice ?? 0,
                            DiscountedPrice = sp.DiscountedPrice,
                            LinkImage = sp.Linkimage,
                            CreationDate = sp.CreationDate ?? DateTime.Now
                        }).ToList()
                    }).ToList()
            }).ToList();

            // Flatten all products from all product types into a single list for "All Products" tab
            var allProducts = productTypeViewModels
                .SelectMany(pt => pt.Products)
                .ToList();

            // Pass the view model to the view
            var viewModel = new HomeProductViewModel
            {
                ProductTypes = productTypeViewModels,
                AllProducts = allProducts // Adding all products to the view model
            };

            return View(viewModel);
        }

        public IActionResult ThuNghiem()
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            var productTypes = _context.ProductTypes
        .Include(pt => pt.ProductTypeLinks)
            .ThenInclude(ptl => ptl.Product)
                .ThenInclude(p => p.SubProducts)
        .Where(pt => pt.ProductTypeLinks.Any(ptl => ptl.Product.SubProducts.Any()))
        .ToList();

            // Mapping to ViewModel
            var productTypeViewModels = productTypes.Select(pt => new ProductTypeViewModel
            {
                ProductTypeId = pt.ProductTypeId,
                ProductTypeName = pt.ProductTypeName,
                Products = pt.ProductTypeLinks
                    .Where(ptl => ptl.Product != null && ptl.Product.SubProducts.Any())
                    .Select(ptl => new ListProductTypeViewModel
                    {
                        ProductId = ptl.Product.ProductId,
                        ProductName = ptl.Product.ProductName,
                        Description = ptl.Product.Description,
                        SubProducts = ptl.Product.SubProducts.Select(sp => new SubProductTypeViewModel
                        {
                            SubProductId = sp.SubProductId,
                            MainProductId = sp.MainProductId ?? 0,
                            OriginalPrice = sp.OriginalPrice ?? 0,
                            DiscountedPrice = sp.DiscountedPrice,
                            LinkImage = sp.Linkimage,
                            CreationDate = sp.CreationDate ?? DateTime.Now
                        }).ToList()
                    }).ToList()
            }).ToList();

            // Flatten all products from all product types into a single list for "All Products" tab
            var allProducts = productTypeViewModels
                .SelectMany(pt => pt.Products)
                .ToList();

            // Pass the view model to the view
            var viewModel = new HomeProductViewModel
            {
                ProductTypes = productTypeViewModels,
                AllProducts = allProducts // Adding all products to the view model
            };

            return View(viewModel);
        }


        /*        public IActionResult Privacy()
                {
                    return View();
                }

                [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
                public IActionResult Error()
                {
                    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }*/
    }
}
