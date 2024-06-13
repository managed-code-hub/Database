using FluentAssertions;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.DynamoDbTests;

#if DYNAMO_DB || DEBUG
[Collection(nameof(DynamoDBTestContainer))]
public class DynamoDbCollectionTests : BaseCollectionTests<string, TestDynamoDbItem>
{
    public DynamoDbCollectionTests(DynamoDBTestContainer container) : base(container)
    {
    }

    [Fact]
    public override async Task DeleteAll()
    {
        // Arrange
        int itemsCount = 5;
        List<TestDynamoDbItem> list = new();

        for (var i = 0; i < itemsCount; i++)
        {
            list.Add(CreateNewItem());
        }

        await Collection.InsertAsync(list);

        // Act
        var deletedItems = await Collection.DeleteCollectionAsync();

        // Assert
        deletedItems.Should().BeTrue();
    }

    [Fact]
    public override async Task DeleteItemById_WhenItemDoesntExists()
    {
        // Arrange
        var item = CreateNewItem();

        // Act
        var deleted = () => Collection.DeleteAsync(item.Id);

        // Assert
        deleted.Should().ThrowExactlyAsync<DatabaseException>();
    }

    [Fact]
    public override async Task GetById_ReturnOk()
    {
        // Arrange
        var itemId = GenerateId();
        await Collection.InsertAsync(CreateNewItem(itemId));

        try
        {
            // Act
            var getItemResult = await Collection.GetAsync(itemId);

            // Assert
            getItemResult.Should().NotBeNull();
        }
        catch (DatabaseException e)
        {
            // Act
            var getItemResult = () => Collection.GetAsync(itemId);

            // Assert
            await getItemResult.Should().ThrowAsync<DatabaseException>();
        }
    }

    [Fact]
    public override async Task InsertItem_WhenItemExist_ShouldThrowDatabaseException()
    {
        // Arrange
        var item = CreateNewItem();

        // Act
        await Collection.InsertAsync(item);
        var insertItem = await Collection.InsertAsync(item);

        // Assert
        insertItem.Should().NotBeNull();
    }

    [Fact]
    public override async Task UpdateItem_WhenItemDoesntExists()
    {
        // Arrange
        var id = GenerateId();

        // Act
        var updateItem = await Collection.UpdateAsync(CreateNewItem(id));

        // Assert
        updateItem.Should().BeNull();
    }
}
#endif