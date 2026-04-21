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
    public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
    {
        public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
        {
            builder.HasKey(pr => pr.Id);

            builder.Property(pr => pr.Reason).HasMaxLength(500);
            builder.Property(pr => pr.RefundAmount).HasPrecision(18, 2);

            builder.HasOne(pr => pr.PurchaseInvoice)
                   .WithMany(pi => pi.Returns)
                   .HasForeignKey(pr => pr.PurchaseInvoiceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pr => pr.User)
                   .WithMany()
                   .HasForeignKey(pr => pr.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pr => pr.PurchaseInvoiceId);
        }
    }
}
