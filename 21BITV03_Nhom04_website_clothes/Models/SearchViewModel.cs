using _21BITV03_Nhom04_website_clothes.Data;
using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class SearchViewModel
    {

        public string? ProductName { get; set; }
        public int? ProductTypeId { get; set; }
        public string? Description { get; set; }
        public double? DiscountedPrice { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public string? ColorName { get; set; }
        public string? SizeName { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalResults { get; set; } = 0; // Tổng số kết quả
        public int TotalPages => (int)Math.Ceiling(TotalResults / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public IEnumerable<Product> SearchResults { get; set; } = new List<Product>();
        public IEnumerable<Product> Products { get; set; } = new List<Product>(); // Optional: populate other filters if needed
        public IEnumerable<ProductColor> Colors { get; set; } = new List<ProductColor>();
        public IEnumerable<ProductSize> Sizes { get; set; } = new List<ProductSize>();
        public List<int> SelectedColorIds { get; set; } = new List<int>();
        public List<int> SelectedSizeIds { get; set; } = new List<int>();
        public IEnumerable<ProductType> ProductTypes { get; set; } = new List<ProductType>();
        public Dictionary<int, int> ProductTypeCounts { get; set; } = new Dictionary<int, int>();


    }
}
