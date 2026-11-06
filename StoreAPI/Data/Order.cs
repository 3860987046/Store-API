using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreAPI.Models;

namespace StoreAPI.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);    //  18 رقم کل، 2 رقم اعشار

            builder.HasOne(o => o.User)                   // هر سفارش یک کاربر دارد
                   .WithMany()                           // هر کاربر می‌تواند چندین سفارش داشته باشد.
                                                        // چون در IdentityUser فیلید Ordersتعریف نشده ان را فعلا پاک میکنیم
                   .HasForeignKey(o => o.UserId)       // فیلد ارتباطی: UserId در جدول Orders
                   .OnDelete(DeleteBehavior.Restrict);// اگر کاربر حذف شد، سفارش‌هایش حذف نشوند
        }

    }
}
