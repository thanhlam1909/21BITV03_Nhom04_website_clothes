namespace _21BITV03_Nhom04_website_clothes.Models
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartProductViewModel> CartProducts { get; set; } = new List<CartProductViewModel>();
    }

    public class CartProductViewModel
    {
        public int ProductId { get; set; }
        public int SubProductId { get; set; }
        public string ProductName { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public int Quantity { get; set; }
        public double OriginalPrice { get; set; }
        public double DiscountedPrice { get; set; }
        public string ImageUrl { get; set; }
        public int CartProductListId { get; set; }  // Assuming it's an integer type

    }


}
