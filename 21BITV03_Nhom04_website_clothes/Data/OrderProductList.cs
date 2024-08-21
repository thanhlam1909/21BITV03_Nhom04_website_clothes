using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class OrderProductList
{
    public int OrderProductListId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? Quanity { get; set; }

    public string? ColorName { get; set; }

    public string? SizeName { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
