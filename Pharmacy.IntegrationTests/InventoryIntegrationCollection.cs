using Xunit;

namespace Pharmacy.IntegrationTests;

[CollectionDefinition("inventory-integration", DisableParallelization = true)]
public sealed class InventoryIntegrationCollection : ICollectionFixture<PharmacyWebApplicationFactory>
{
}
