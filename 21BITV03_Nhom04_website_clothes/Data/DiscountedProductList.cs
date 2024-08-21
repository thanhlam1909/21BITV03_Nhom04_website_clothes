using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class DiscountedProductList
{
    public int DiscountedProductListId { get; set; }

    public int? DiscountId { get; set; }

    public int? ProductId { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual Product? Product { get; set; }
}
