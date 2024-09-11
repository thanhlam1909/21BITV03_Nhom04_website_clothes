using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class ProductViewAdminModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public bool? DeleteStatus { get; set; }
        public DateTime? DeletionDate { get; set; }
        public bool? Status { get; set; }

        // List of available ProductTypes
        public List<ProductType> AvailableProductTypes { get; set; } = new List<ProductType>();

        // List of selected ProductTypeIds
        public List<int> SelectedProductTypeIds { get; set; } = new List<int>();
    }
}
