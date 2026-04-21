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
    public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
    {
        public void Configure(EntityTypeBuilder<SalesReturn> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Reason).HasMaxLength(500);
            builder.Property(r => r.RefundAmount).HasPrecision(18, 2);

            builder.HasOne(r => r.SalesInvoice)
                   .WithMany(i => i.Returns)
                   .HasForeignKey(r => r.SalesInvoiceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(r => r.SalesInvoiceId);
        }
    }
}
