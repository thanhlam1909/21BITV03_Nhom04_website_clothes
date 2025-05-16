using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class DiscountViewModel
    {
        public Discount Discount { get; set; }

        // Danh sách sản phẩm để hiển thị checkbox trong form
        public List<Product> Products { get; set; }

        // ID các sản phẩm được chọn trong form (checkbox)
        public List<int> SelectedProductIds { get; set; }
    }

}
