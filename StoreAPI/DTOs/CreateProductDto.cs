using System.ComponentModel.DataAnnotations;

namespace StoreAPI.DTOs
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "نام محصول الزامی است")]
        [MaxLength(150, ErrorMessage = "نام محصول نمی‌تواند بیش از ۱۵۰ کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "قیمت الزامی است")]
        [Range(0.01, double.MaxValue, ErrorMessage = "قیمت باید بزرگتر از صفر باشد")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "تعداد موجودی الزامی است")]
        [Range(0, int.MaxValue, ErrorMessage = "موجودی نمی‌تواند منفی باشد")]
        public int StockQuantity { get; set; }

    }
}
