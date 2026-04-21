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
    public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
    {
        public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
        {
            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.InvoiceNumber).IsRequired().HasMaxLength(50);
            builder.HasIndex(pi => pi.InvoiceNumber).IsUnique();

            builder.Property(pi => pi.Subtotal).HasPrecision(18, 2);
            builder.Property(pi => pi.GrandTotal).HasPrecision(18, 2);
            builder.Property(pi => pi.PaidAmount).HasPrecision(18, 2);
            builder.Property(pi => pi.RemainingAmount).HasPrecision(18, 2);
            builder.Property(pi => pi.TaxRate).HasPrecision(5, 2);
            builder.Property(pi => pi.TaxAmount).HasPrecision(18, 2);

            builder.Property(pi => pi.PaymentMethod).HasConversion<string>();
            builder.Property(pi => pi.Status).HasConversion<string>();

            builder.HasOne(pi => pi.Supplier)
                   .WithMany()
                   .HasForeignKey(pi => pi.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pi => pi.User)
                   .WithMany()
                   .HasForeignKey(pi => pi.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pi => pi.Branch)
                   .WithMany()
                   .HasForeignKey(pi => pi.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(pi => pi.Items)
                   .WithOne(item => item.PurchaseInvoice)
                   .HasForeignKey(item => item.PurchaseInvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
