using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class ProductSize
{
    public int ProductSizeId { get; set; }

    public string? SizeName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<SubProduct> SubProducts { get; set; } = new List<SubProduct>();
}
