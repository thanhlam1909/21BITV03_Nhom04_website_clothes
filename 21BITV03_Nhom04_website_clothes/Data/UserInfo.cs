using System;
using System.Collections.Generic;

namespace _21BITV03_Nhom04_website_clothes.Data;

public partial class UserInfo
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? Address { get; set; }

    public int? Phone { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual AspNetUser User { get; set; } = null!;
}
