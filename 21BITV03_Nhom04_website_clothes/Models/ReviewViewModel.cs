namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class ReviewViewModel
    {
        public int ProductId { get; set; }  // Id of the product to which the comment is added
        public string Username { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}
