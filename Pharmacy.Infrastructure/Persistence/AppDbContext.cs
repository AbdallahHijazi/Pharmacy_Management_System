using Microsoft.EntityFrameworkCore;
using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Finance;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Entities.Organization;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Entities.Purchases;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Entities.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<PharmacyInfo> Pharmacies { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StockBatch> StockBatches { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<SalesInvoice> SalesInvoices { get; set; }
        public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }
        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<SalesReturnItem> SalesReturnItems { get; set; }
        public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }
        public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
        public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // === Global Query Filter لـ Soft Delete ===
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var filter = Expression.Lambda(
                        Expression.Equal(
                            Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                            Expression.Constant(false)),
                        parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }

            modelBuilder.Entity<StockBatch>()
                .HasIndex(sb => sb.ExpiryDate);
            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => t.Type);
            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => t.StockBatchId);

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => t.ReferenceId);
            modelBuilder.Entity<StockBatch>()
                .HasIndex(sb => new { sb.ProductId, sb.ExpiryDate });

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PartyId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.InvoiceId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
