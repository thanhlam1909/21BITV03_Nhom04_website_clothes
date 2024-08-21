using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class Discount
{
    public int DiscountId { get; set; }

    public string? DiscountType { get; set; }

    public double? DiscountAmount { get; set; }

    public string? DiscountConditions { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual ICollection<DiscountedProductList> DiscountedProductLists { get; set; } = new List<DiscountedProductList>();

    public virtual ICollection<OrderDiscountList> OrderDiscountLists { get; set; } = new List<OrderDiscountList>();
}
