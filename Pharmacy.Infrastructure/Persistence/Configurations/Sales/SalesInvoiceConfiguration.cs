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
    public class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
    {
        public void Configure(EntityTypeBuilder<SalesInvoice> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();

            builder.Property(i => i.Subtotal).HasPrecision(18, 2);
            builder.Property(i => i.GrandTotal).HasPrecision(18, 2);
            builder.Property(i => i.PaidAmount).HasPrecision(18, 2);
            builder.Property(i => i.RemainingAmount).HasPrecision(18, 2);

            builder.Property(i => i.PaymentMethod).HasConversion<string>();
            builder.Property(i => i.Status).HasConversion<string>();

            builder.HasOne(i => i.Customer).WithMany().HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(i => i.Branch).WithMany().HasForeignKey(i => i.BranchId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.Items)
                   .WithOne(item => item.SalesInvoice)
                   .HasForeignKey(item => item.SalesInvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.User)
                   .WithMany()
                   .HasForeignKey(i => i.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(i => i.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
            builder.Property(i => i.TaxRate).HasPrecision(5, 2);
            builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        }
    }
}
