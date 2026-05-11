using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Sales
{
    public class SalesInvoiceItemConfiguration : IEntityTypeConfiguration<SalesInvoiceItem>
    {
        public void Configure(EntityTypeBuilder<SalesInvoiceItem> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
            builder.Property(i => i.Subtotal).HasPrecision(18, 2);
            builder.Property(i => i.UnitEffectiveCostAtSale).HasPrecision(18, 4);
            builder.Property(i => i.BatchNominalPurchasePriceAtSale).HasPrecision(18, 4);

            builder.HasOne(i => i.StockBatch)
                   .WithMany(sb => sb.SalesItems)
                   .HasForeignKey(i => i.StockBatchId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
