using _21BITV03_Nhom04_website_clothes.Data;
using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class ProductViewModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Comment { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public double OriginalPrice { get; set; }
        public double? DiscountedPrice { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public List<SubProductViewModel> SubProducts { get; set; } = new List<SubProductViewModel>();
        public List<ProductColor> ColorOptions { get; set; } = new List<ProductColor>();
        public List<ProductSize> SizeOptions { get; set; } = new List<ProductSize>();
        public List<ReviewProductView> Reviews { get; set; } = new List<ReviewProductView>(); 

    }


    public class SubProductViewModel
    {
        public int SubProductId { get; set; }
        public int MainProductId { get; set; }
        public double OriginalPrice { get; set; }
        public double? DiscountedPrice { get; set; }
        public int ColorId { get; set; }
        public string? ColorName { get; set; }
        public int SizeId { get; set; }
        public string? SizeName { get; set; }
        public string LinkImage { get; set; }
        public DateTime CreationDate { get; set; }
    }
    public class ReviewProductView // Corrected class declaration
    {
        public int IdRv { get; set; }
        public int? ProductId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Comment { get; set; }
    }

}
