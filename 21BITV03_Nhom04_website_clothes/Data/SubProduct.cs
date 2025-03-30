using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class SubProduct
{
    public int SubProductId { get; set; }

    public int? MainProductId { get; set; }

    public double? OriginalPrice { get; set; }

    public double? DiscountedPrice { get; set; }

    public int? ColorId { get; set; }

    public int? SizeId { get; set; }

    public DateTime? CreationDate { get; set; }

    public string? Linkimage { get; set; }

    public string? Status { get; set; }

    public int? MaterialId { get; set; }

    public int? SkuId { get; set; }

    public virtual ICollection<CartProductList> CartProductLists { get; set; } = new List<CartProductList>();

    public virtual ProductColor? Color { get; set; }

    public virtual ICollection<InventoryTransactionDetail> InventoryTransactionDetails { get; set; } = new List<InventoryTransactionDetail>();

    public virtual Product? MainProduct { get; set; }

    public virtual Material? Material { get; set; }

    public virtual ProductSize? Size { get; set; }

    public virtual Sku? Sku { get; set; }
}
