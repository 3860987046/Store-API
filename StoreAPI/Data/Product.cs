using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreAPI.Models;

namespace StoreAPI.Data
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // تنظیمات قبلی که توی OnModelCreating بود رو اینجا بیار
            builder.Property(p => p.Price).HasPrecision(18, 2);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Price", "[Price] >= 0");
                t.HasCheckConstraint("CK_Stock", "[StockQuantity] >= 0");
            });
        }
    }
}
