using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Partners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Partners
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
            builder.Property(c => c.Phone).HasMaxLength(20);
            builder.Property(c => c.DebtAmount).HasPrecision(18, 2);
            builder.Property(c => c.TotalPurchases)
                   .HasPrecision(18, 2);
            builder.HasOne(c => c.Branch)
                   .WithMany()
                   .HasForeignKey(c => c.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
