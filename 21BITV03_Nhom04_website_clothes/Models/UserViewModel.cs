namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public int Phone { get; set; }
        public bool IsAdmin { get; set; }

        public int RoleId { get; set; }  // Store the selected role's ID
    }
}
