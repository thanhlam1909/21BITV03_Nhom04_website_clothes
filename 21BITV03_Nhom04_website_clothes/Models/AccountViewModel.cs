using _21BITV03_Nhom04_website_clothes.Data;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class AccountViewModel
    {
        public AspNetUser Users { get; set; }
        public UserInfo UserInfoes { get; set; }
        public Order Orders { get; set; }
        public List<OrderProductList> OrderProductLists { get; set; }
    }
}
