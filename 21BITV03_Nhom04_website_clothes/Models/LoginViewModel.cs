using System.ComponentModel.DataAnnotations;

namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class LoginViewModel
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public bool KeepLoggedIn { get; set; }
    }

}
