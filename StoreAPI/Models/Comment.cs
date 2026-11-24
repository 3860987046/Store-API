using Microsoft.AspNetCore.Identity;

namespace StoreAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; } // کاربری که کامنت گذاشته
        public int ProductId { get; set; }
        public bool IsApproved { get; set; } = false; // پیش‌فرض تایید نشده
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Product Product { get; set; }
        public IdentityUser User { get; set; }
    }
}