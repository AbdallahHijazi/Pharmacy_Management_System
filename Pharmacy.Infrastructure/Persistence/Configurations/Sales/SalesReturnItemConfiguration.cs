using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Purchases
{
    public class SalesReturnItemConfiguration : IEntityTypeConfiguration<SalesReturnItem>
    {
        public void Configure(EntityTypeBuilder<SalesReturnItem> builder)
        {
            builder.HasKey(ri => ri.Id);

            builder.Property(ri => ri.Quantity).IsRequired();
            builder.Property(ri => ri.UnitPrice).HasPrecision(18, 2);

            builder.HasOne(ri => ri.SalesReturn)
                   .WithMany(r => r.Items)
                   .HasForeignKey(ri => ri.SalesReturnId)
                   .OnDelete(DeleteBehavior.Cascade);   // Cascade آمن

            builder.HasOne(ri => ri.StockBatch)
                   .WithMany()
                   .HasForeignKey(ri => ri.StockBatchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ri => ri.SalesInvoiceItem)
                .WithMany()
                .HasForeignKey(ri => ri.SalesInvoiceItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasIndex(ri => ri.SalesReturnId);

            builder.HasIndex(ri => ri.StockBatchId);
        }
    }
}
