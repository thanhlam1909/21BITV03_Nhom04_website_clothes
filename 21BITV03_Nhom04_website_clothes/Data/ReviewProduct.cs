using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class ReviewProduct
{
    public int IdRv { get; set; }

    public int? ProductId { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Comment { get; set; }

    public virtual Product? Product { get; set; }
}
