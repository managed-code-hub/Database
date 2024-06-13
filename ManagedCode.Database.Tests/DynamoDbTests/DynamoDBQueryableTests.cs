using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.DynamoDbTests;

#if DYNAMO_DB || DEBUG
[Collection(nameof(DynamoDBTestContainer))]
public class DynamoDBQueryableTests : BaseQueryableTests<string, TestDynamoDbItem>
{
    public DynamoDBQueryableTests(DynamoDBTestContainer container) : base(container)
    {
    }

    [Fact]
    public override async Task ThenBy_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(o => o.StringData)
            .ThenBy(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
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
}
#endif