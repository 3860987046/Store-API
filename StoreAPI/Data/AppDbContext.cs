using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Models;
using System;

namespace StoreAPI.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        //تنظیمات مربوط به مدل‌ها و ارتباط آن‌ها با دیتابیس

        //وقتی EF Core مدل دیتابیس را می‌سازد، متد OnModelCreating را فراخوانی می‌کند و یک شیء ModelBuilder .را به آن تحویل می‌دهد
        //اما اگر خودمان new کنیم یک شیء جدید و جداگانه ساخته ایم که لزوماً به فرایند ساخت مدل فعلی EF Core متصل نیست
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
           
            //modelbuilder یک ابزاری در EF است که تنظیمات مربوط به Entity کاربر رو انجام میده.
           /*چون IdentityUserخودش این تنظیمات را دارد 
            modelBuilder.Entity<IdentityUser>()
            .HasIndex(u => u.UserName)       //روی این ستون یه ایندکس تعریف کن
            .IsUnique();                    //مقدار این ستون نباید تکراری باشد
           */

            // رابطه Order با User - اصلاح شده
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)           // هر سفارش یک کاربر دارد
                .WithMany()       // هر کاربر می‌تواند چندین سفارش داشته باشد.
                                  // چون در IdentityUser فیلید Ordersتعریف نشده ان را فعلا پاک میکنیم
                .HasForeignKey(o => o.UserId)  // فیلد ارتباطی: UserId در جدول Orders
                .OnDelete(DeleteBehavior.Restrict); // اگر کاربر حذف شد، سفارش‌هایش حذف نشوند

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            //برای بحث گرد کردن    
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);   //  18 رقم کل، 2 رقم اعشار
           
            modelBuilder.Entity<Product>().ToTable(t =>//موجودیت Product را به جدول Products نگاشت کن
            {
                t.HasCheckConstraint("CK_Price", "[Price] >= 0");  // اولی نام محدودیت دیتابیس است و دومی قیمت نباید منفی باشد
                t.HasCheckConstraint("CK_Stock", "[StockQuantity] >= 0");  // موجودی نباید منفی باشد
            });

            // رابطه OrderItem با Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)        // هر آیتم سفارش به یک سفارش تعلق دارد
                .WithMany(o => o.OrderItems)   // هر سفارش می‌تواند چندین آیتم داشته باشد
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // اگر سفارش حذف شد، آیتم‌هایش هم حذف شوند

            // رابطه OrderItem با Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)      // هر آیتم سفارش یک محصول است
                .WithMany(p => p.OrderItem)   // هر محصول می‌تواند در چندین آیتم سفارش باشد
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // اگر محصولی به یک یا چند آیتم سفارش مرتبط باشد، اجازه نده آن محصول حذف شود
            
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);
            
        }
    }
}
