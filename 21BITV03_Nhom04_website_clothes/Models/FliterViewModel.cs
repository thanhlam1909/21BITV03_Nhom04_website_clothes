using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class FilterViewModel
    {
        public List<ProductColor> AvailableColors { get; set; } = new List<ProductColor>();
        public List<ProductSize> AvailableSizes { get; set; } = new List<ProductSize>();

        public List<int> SelectedColorIds { get; set; } = new List<int>();
        public List<int> SelectedSizeIds { get; set; } = new List<int>();

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public List<Product> FilteredProducts { get; set; } = new List<Product>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 2;
        public int TotalResults { get; set; } = 0; // Tổng số kết quả
        public int TotalPages => (int)Math.Ceiling(TotalResults / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

}
