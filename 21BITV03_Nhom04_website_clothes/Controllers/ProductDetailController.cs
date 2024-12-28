using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Mvc;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class ProductDetailController : Controller
    {
        private readonly WebsiteClothesContext _context;
        public ProductDetailController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
