using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class ProductColor
{
    public int ColorId { get; set; }

    public string? ColorName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<SubProduct> SubProducts { get; set; } = new List<SubProduct>();
}
