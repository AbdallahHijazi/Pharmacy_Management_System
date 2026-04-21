using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Configurations.Organization
{
    public class PharmacyConfiguration : IEntityTypeConfiguration<PharmacyInfo>
    {
        public void Configure(EntityTypeBuilder<PharmacyInfo> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Currency).HasMaxLength(10);
            builder.Property(p => p.ExchangeRate).HasPrecision(18, 4);
            builder.Property(p => p.TaxRate).HasPrecision(5, 2);
        }
    }
}
