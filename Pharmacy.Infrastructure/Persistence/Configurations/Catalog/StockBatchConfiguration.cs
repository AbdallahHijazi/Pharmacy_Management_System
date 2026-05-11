using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Catalog
{
    public class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
    {
        public void Configure(EntityTypeBuilder<StockBatch> builder)
        {
            builder.HasKey(sb => sb.Id);
            builder.Property(sb => sb.BatchNumber).IsRequired().HasMaxLength(50);
            builder.HasIndex(sb => new { sb.ProductId, sb.BatchNumber }).IsUnique();
            builder.Property(sb => sb.PurchasePrice).HasPrecision(18, 2);

            builder.HasIndex(sb => sb.ExpiryDate);
            builder.HasIndex(sb => new { sb.ProductId, sb.ExpiryDate });
            builder.HasOne(sb => sb.Product)
                   .WithMany(p => p.StockBatches)
                   .HasForeignKey(sb => sb.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sb => sb.Branch)
                   .WithMany(b => b.StockBatches)
                   .HasForeignKey(sb => sb.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sb => sb.Supplier)
                   .WithMany()
                   .HasForeignKey(sb => sb.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(sb => sb.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
