namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class UserRoleViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
        public int SelectedRoleId { get; set; }  // Added property for selected role ID
    }

    public class RoleViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
