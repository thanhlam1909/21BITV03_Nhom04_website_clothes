using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class User
    {
        public AspNetUser Users { get; set; }
        public UserInfo UserInfo { get; set; }
        public AspNetRole RoleUser { get; set; }
        public IEnumerable<AspNetRole> Roles { get; set; }
    }
}