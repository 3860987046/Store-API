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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            // این خط به EF میگه برو توی اسمبلی فعلی، هر کلاسی که IEntityTypeConfiguration داره رو پیدا کن و اعمال کن
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            /*چون IdentityUserخودش این تنظیمات را دارد 
             modelBuilder.Entity<IdentityUser>()
             .HasIndex(u => u.UserName)       //روی این ستون یه ایندکس تعریف کن
             .IsUnique();                    //مقدار این ستون نباید تکراری باشد
            */

        }
    }
}
