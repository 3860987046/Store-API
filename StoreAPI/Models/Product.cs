using System.ComponentModel.DataAnnotations;

namespace StoreAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        //string چون یک رفرنس تایپ است بهتر است برای جلوگیری از null بودن مقدار دهی اولیه بشود 
        public string Name { get; set; } =string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        //معمولا Navigation Property را به صورت اینترفیس تعریف کنیم
        //ICollection چون یک رفرنس تایپ است بهتر است برای جلوگیری از null بودن یک مجموعه خالی اولیه بگیرد 
        public ICollection<OrderItem> OrderItem { get; set; } = new List<OrderItem>();

    }
}
