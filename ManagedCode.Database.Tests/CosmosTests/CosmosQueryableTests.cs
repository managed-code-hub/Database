using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System.Threading.Tasks;
using System;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.CosmosTests;

#if COSMOS_DB || DEBUG
[Collection(nameof(CosmosTestContainer))]
public class CosmosQueryableTests : BaseQueryableTests<string, TestCosmosItem>
{
    public CosmosQueryableTests(ITestOutputHelper testOutputHelper, CosmosTestContainer container) : base(container)
    {
    }

    public override async Task OrderBy_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    public override async Task OrderByDescending_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        var guid = Guid.NewGuid().ToString();
        var unfaithfulGuid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = () => Collection.Query
            .OrderByDescending(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    public override async Task Where_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;


        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .Where(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    public override async Task ThenByDescending_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(o => o.StringData)
            .ThenByDescending(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    public override async Task Skip_NegativeNumber_ReturnZero()
    {
        var baseMethod = async () => await base.Skip_NegativeNumber_ReturnZero();

        await baseMethod.Should().ThrowExactlyAsync<ArgumentException>();
    }

    public override async Task Take_NegativeNumber_ReturnZero()
    {
        var baseMethod = () => base.Take_NegativeNumber_ReturnZero();

        await baseMethod.Should().ThrowExactlyAsync<ArgumentException>();
    }
}
#endif