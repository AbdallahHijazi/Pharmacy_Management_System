using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Finance
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Amount).HasPrecision(18, 2);
            builder.Property(p => p.PartyType).HasConversion<string>().IsRequired();
            builder.Property(p => p.PaymentMethod).HasConversion<string>().IsRequired();
            builder.Property(p => p.InvoiceType).HasConversion<string>().IsRequired();

            builder.HasOne(p => p.Branch).WithMany().HasForeignKey(p => p.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(p => p.PartyId);
            builder.HasIndex(p => p.InvoiceId);
        }
    }
}
