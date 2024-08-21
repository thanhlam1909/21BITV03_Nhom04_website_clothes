using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class AspNetUserLogin
{
    public string LoginProvider { get; set; } = null!;

    public string ProviderKey { get; set; } = null!;

    public string? ProviderDisplayName { get; set; }

    public int UserId { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
