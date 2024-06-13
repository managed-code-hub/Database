using FluentAssertions;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.CosmosTests;

#if COSMOS_DB || DEBUG
[Collection(nameof(CosmosTestContainer))]
public class CosmosCollectionTests : BaseCollectionTests<string, TestCosmosItem>
{
    public CosmosCollectionTests(ITestOutputHelper testOutputHelper, CosmosTestContainer container) : base(container)
    {
    }

        public override async Task DeleteItemById_WhenItemDoesntExists()
        {
            var baseMethod = () => base.DeleteItemById_WhenItemDoesntExists();

            await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
        }

        public override async Task DeleteListOfItemsById_WhenItemsDontExist()
        {
            var baseMethod = () => base.DeleteListOfItemsById_WhenItemsDontExist();

            await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
        }

        public override async Task DeleteListOfItems_WhenItemsDontExist()
        {
            var baseMethod = () => base.DeleteListOfItems_WhenItemsDontExist();

            await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
        }

        [Fact]
        public override async Task DeleteCollectionAsync()
        {
            // Arrange
            int itemsCount = 5;
            List<TestCosmosItem> list = new();

            for (var i = 0; i < itemsCount; i++)
            {
                list.Add(CreateNewItem());
            }

            await Collection.InsertAsync(list);
            // Act
            var isDeleted = await Collection.DeleteCollectionAsync();

            // Assert
            isDeleted.Should().BeTrue();
        }
}
#endif