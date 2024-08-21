using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class OrderDiscountList
{
    public int OrderDiscountListId { get; set; }

    public int? DiscountId { get; set; }

    public string? DiscoutType { get; set; }

    public int? OrderId { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual Order? Order { get; set; }
}
