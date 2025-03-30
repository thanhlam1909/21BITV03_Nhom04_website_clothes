using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class ManageProduct
    {
        public Product Product { get; set; }
  
        public List<ProductType> AvailableProductTypes { get; set; } = new List<ProductType>();
        public List<int> SelectedProductTypeIds { get; set; } = new List<int>();
    }
}
