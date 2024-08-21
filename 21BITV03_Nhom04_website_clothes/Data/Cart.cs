using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class Cart
{
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<CartProductList> CartProductLists { get; set; } = new List<CartProductList>();

    public virtual UserInfo? User { get; set; }
}
