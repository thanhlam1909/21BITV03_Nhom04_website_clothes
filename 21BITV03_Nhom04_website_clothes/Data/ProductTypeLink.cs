using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class ProductTypeLink
{
    public int ProductTypeLinkId { get; set; }

    public int? ProductId { get; set; }

    public int? ProductTypeId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual ProductType? ProductType { get; set; }
}
