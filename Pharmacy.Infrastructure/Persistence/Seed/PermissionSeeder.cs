using Microsoft.EntityFrameworkCore;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Entities.Partners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence.Seed
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var permissions = new List<Permission>
        {

            new Permission { Module = "categories", Name = "View" },
            new Permission { Module = "categories", Name = "Create" },
            new Permission { Module = "categories", Name = "Update" },
            new Permission { Module = "categories", Name = "Delete" },

            new Permission { Module = "customers", Name = "View" },
            new Permission { Module = "customers", Name = "Create" },
            new Permission { Module = "customers", Name = "Update" },
            new Permission { Module = "customers", Name = "Delete" },

            new Permission { Module = "dashboard", Name = "View" },

            new Permission { Module = "Products", Name = "View" },
            new Permission { Module = "Products", Name = "Create" },
            new Permission { Module = "Products", Name = "Update" },
            new Permission { Module = "Products", Name = "Delete" },

            new Permission { Module = "Customers", Name = "View" },
            new Permission { Module = "Customers", Name = "Create" },
            new Permission { Module = "Customers", Name = "Update" },
            new Permission { Module = "Customers", Name = "Delete" },

            new Permission { Module = "Suppliers", Name = "View" },
            new Permission { Module = "Suppliers", Name = "Create" },
            new Permission { Module = "Suppliers", Name = "Update" },
            new Permission { Module = "Suppliers", Name = "Delete" },

            new Permission { Module = "SalesInvoices", Name = "View" },
            new Permission { Module = "SalesInvoices", Name = "Create" },

            new Permission { Module = "PurchaseInvoices", Name = "View" },
            new Permission { Module = "PurchaseInvoices", Name = "Create" },

            new Permission { Module = "Reports", Name = "View" },

            new Permission { Module = "Users", Name = "View" },
            new Permission { Module = "Users", Name = "Create" },
            new Permission { Module = "Users", Name = "Update" },
            new Permission { Module = "Users", Name = "Delete" },

            new Permission { Module = "Roles", Name = "View" },
            new Permission { Module = "Roles", Name = "AssignPermissions" },
        };

            foreach (var permission in permissions)
            {
                bool exists = await context.Permissions.AnyAsync(p =>
                    p.Module == permission.Module &&
                    p.Name == permission.Name);

                if (!exists)
                {
                    context.Permissions.Add(permission);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
