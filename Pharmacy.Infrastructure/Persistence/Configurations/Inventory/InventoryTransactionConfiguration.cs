using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Inventory
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Type).HasConversion<string>().IsRequired();
            builder.Property(t => t.ReferenceType).HasConversion<string>().IsRequired();

            builder.HasOne(t => t.StockBatch).WithMany(sb => sb.Transactions).HasForeignKey(t => t.StockBatchId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(t => t.ProductId).IsRequired(false);
            builder.HasIndex(t => t.ProductId);
            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasIndex(t => t.StockBatchId);
            builder.HasIndex(t => t.ReferenceId);
            builder.HasIndex(t => t.Type);
            builder.HasOne(t => t.User)
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Branch)
                   .WithMany()
                   .HasForeignKey(t => t.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
