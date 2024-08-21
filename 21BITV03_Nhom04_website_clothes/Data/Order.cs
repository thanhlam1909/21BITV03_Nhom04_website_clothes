using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public string? Message { get; set; }

    public string? PaymentMethod { get; set; }

    public string? OrderStatus { get; set; }

    public virtual ICollection<OrderDiscountList> OrderDiscountLists { get; set; } = new List<OrderDiscountList>();

    public virtual ICollection<OrderProductList> OrderProductLists { get; set; } = new List<OrderProductList>();

    public virtual UserInfo? User { get; set; }
}
