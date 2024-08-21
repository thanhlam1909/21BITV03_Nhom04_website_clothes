using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class CartProductList
{
    public int CartProductList1 { get; set; }

    public int? SubProductId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public int? CartId { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Product? Product { get; set; }

    public virtual SubProduct? SubProduct { get; set; }
}
