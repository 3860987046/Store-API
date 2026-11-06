using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreAPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        //هر سفارشی مربوط به یک User است
        [ForeignKey("User")]
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        // Navigation Property
        //اما یک سفارش لیستی از OrderItemها را دارد 
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public IdentityUser User { get; set; } = null!;

    }
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        //هر OrderItem مربوط به یک سفارش است 
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        //هر OrdderItem مربوط به یک محصول است
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation Property
        public Order Order { get; set; } = null!;
        public Product Product { get; set; }
    }
}
