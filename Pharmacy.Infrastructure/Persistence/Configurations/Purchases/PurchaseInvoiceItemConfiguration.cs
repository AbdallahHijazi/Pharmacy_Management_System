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
    public class PurchaseInvoiceItemConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> builder)
        {
            builder.HasKey(pii => pii.Id);

            builder.Property(pii => pii.BatchNumber).IsRequired().HasMaxLength(50);
            builder.Property(pii => pii.UnitPrice).HasPrecision(18, 2);

            builder.HasOne(pii => pii.PurchaseInvoice)
                   .WithMany(pi => pi.Items)
                   .HasForeignKey(pii => pii.PurchaseInvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pii => pii.Product)
                   .WithMany()
                   .HasForeignKey(pii => pii.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pii => pii.PurchaseInvoiceId);
            builder.HasIndex(pii => pii.ProductId);

            builder.HasIndex(pii => pii.BatchNumber);
        }
    }
}
