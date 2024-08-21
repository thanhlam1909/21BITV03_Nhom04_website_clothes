using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public bool? DeleteStatus { get; set; }

    public DateTime? DeletionDate { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<CartProductList> CartProductLists { get; set; } = new List<CartProductList>();

    public virtual ICollection<DiscountedProductList> DiscountedProductLists { get; set; } = new List<DiscountedProductList>();

    public virtual ICollection<OrderProductList> OrderProductLists { get; set; } = new List<OrderProductList>();

    public virtual ICollection<ProductTypeLink> ProductTypeLinks { get; set; } = new List<ProductTypeLink>();

    public virtual ICollection<SubProduct> SubProducts { get; set; } = new List<SubProduct>();
}
