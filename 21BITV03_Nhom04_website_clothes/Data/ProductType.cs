using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public string? ProductTypeName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<ProductTypeLink> ProductTypeLinks { get; set; } = new List<ProductTypeLink>();
}
