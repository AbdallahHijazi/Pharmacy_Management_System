using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Purchases
{
    public class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
        {
            builder.HasKey(pri => pri.Id);

            builder.Property(pri => pri.Quantity).IsRequired();
            builder.Property(pri => pri.UnitPrice).HasPrecision(18, 2);

            builder.HasOne(pri => pri.PurchaseReturn)
                   .WithMany(pr => pr.Items)
                   .HasForeignKey(pri => pri.PurchaseReturnId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pri => pri.Product)
                   .WithMany()
                   .HasForeignKey(pri => pri.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pri => pri.ProductId);
        }
    }
}
