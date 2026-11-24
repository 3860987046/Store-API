using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreAPI.Models;

namespace StoreAPI.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            // ==========================================
            // 1. تنظیمات فیلدها
            // ==========================================
            builder.Property(c => c.Text)
                .IsRequired()
                .HasMaxLength(500); // حداکثر ۵۰۰ کاراکتر

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.IsApproved)
                .IsRequired()
                .HasDefaultValue(false); // پیش‌فرض تایید نشده

            // ==========================================
            // 2. ارتباط با Product (یک به چند)
            // ==========================================
            // هر کامنت به یک محصول تعلق دارد
            builder.HasOne(c => c.Product)
                .WithMany(p => p.Comments) // چون در کلاس Product پراپرتی List<Comment> نداریم
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // اگر محصول حذف شد، کامنت‌هاش هم حذف شوند

            // ==========================================
            // 3. ارتباط با User (IdentityUser) (یک به چند)
            // ==========================================
            // هر کامنت توسط یک کاربر نوشته شده است
            builder.HasOne(c => c.User)
                .WithMany() // چون IdentityUser لیست کامنت‌ها را ندارد
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // اگر کاربر حذف شد، کامنت‌هاش حذف نشوند (برای حفظ سابقه)
        }
    }
}