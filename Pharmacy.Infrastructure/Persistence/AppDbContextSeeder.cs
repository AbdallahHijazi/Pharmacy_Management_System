using Microsoft.EntityFrameworkCore;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Entities.Organization;
using Pharmacy.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence
{
    public static class AppDbContextSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Pharmacies.AnyAsync() ||
                await context.Branches.AnyAsync() ||
                await context.Roles.AnyAsync() ||
                await context.Users.AnyAsync())
            {
                return;
            }

            var systemUserId = Guid.Empty;

            var pharmacy = new PharmacyInfo
            {
                Id = Guid.NewGuid(),
                Name = "Main Pharmacy",
                Address = "Default Address",
                Phone = "0000000000",
                Currency = "SYP",
                ExchangeRate = 1,
                TaxRate = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = systemUserId,
                IsDeleted = false
            };

            context.Pharmacies.Add(pharmacy);
            await context.SaveChangesAsync();

            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                PharmacyId = pharmacy.Id,
                Name = "Main Branch",
                Address = "Default Branch Address",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = systemUserId,
                IsDeleted = false
            };

            context.Branches.Add(branch);
            await context.SaveChangesAsync();

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Description = "System Administrator",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = systemUserId,
                IsDeleted = false
            };

            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var passwordHasher = new PasswordHasher();

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "System Admin",
                Email = "admin@pharmacy.com",
                Phone = "0000000000",
                PasswordHash = passwordHasher.Hash("Admin@123"),
                RoleId = role.Id,
                BranchId = branch.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = systemUserId,
                IsDeleted = false
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
    }
}
