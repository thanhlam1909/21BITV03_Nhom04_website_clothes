using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class HomeProductViewModel
    {
        public List<ListProductTypeViewModel> AllProducts { get; set; } // New property for all products
        public IEnumerable<ProductTypeViewModel> ProductTypes { get; set; }
    }
    public class ProductTypeViewModel
    {
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public IEnumerable<ListProductTypeViewModel> Products { get; set; }
    }
    public class ListProductTypeViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }

        public List<SubProductTypeViewModel> SubProducts { get; set; } = new List<SubProductTypeViewModel>();
    }
    public class SubProductTypeViewModel
    {
        public int SubProductId { get; set; }
        public int MainProductId { get; set; }
        public double OriginalPrice { get; set; }
        public double? DiscountedPrice { get; set; }
        public string LinkImage { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
