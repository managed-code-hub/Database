using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.LiteDBTests;

#if LITE_DB || DEBUG
public class LiteDBQueryableTests : BaseQueryableTests<string, TestLiteDBItem>
{
    public LiteDBQueryableTests() : base(new LiteDBTestContainer())
    {
    }

    public override async Task OrderByDescendingTakeSkip_ReturnOk()
    {

        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    public override async Task OrderByTakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeOrderByDescendingSkipWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeOrderByDescendingWhereSkip_ReturnOk()
    {

        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeOrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeOrderBySkipWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeOrderByWhereSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        for (var i = 0; i < itemsCountToInsert; i++)
        {
            await Collection.InsertAsync(CreateNewItem());
        }

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult.Count.Should().Be(itemsCountToTake);
    }

    public override async Task TakeSkipOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeSkipOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeSkipOrderByDescendingWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeSkipOrderByWhere_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .Where(w => w.StringData == guid)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeSkipWhereOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeSkipWhereOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeWhereOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip).ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeWhereSkipOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task TakeWhereSkipOrderByDescending_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task TakeWhereOrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Take(itemsCountToTake)
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task ThenBy_AfterOrderBy_ReturnOk()
    {
        var baseMethod = () => base.ThenBy_AfterOrderBy_ReturnOk();

        await baseMethod.Should().ThrowExactlyAsync<ArgumentException>("ORDER BY already defined in this query builder");
    }

    public override async Task ThenBy_AfterOrderByDescending_ReturnOk()
    {
        var baseMethod = () => base.ThenBy_AfterOrderBy_ReturnOk();

        await baseMethod.Should().ThrowExactlyAsync<ArgumentException>("ORDER BY already defined in this query builder");
    }

    public override async Task ThenByDescending_AfterOrderBy_ReturnOk()
    {
        var baseMethod = () => base.ThenBy_AfterOrderBy_ReturnOk();

        await baseMethod.Should().ThrowExactlyAsync<ArgumentException>("ORDER BY already defined in this query builder");

    }

    public override async Task WhereOrderByDescendingTakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderByDescending(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToInsert - itemsCountToSkip - 1);
    }

    public override async Task WhereOrderByTakeSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .OrderBy(o => o.IntData)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);

        itemsResult.First().IntData.Should().Be(itemsCountToSkip);
    }

    public override async Task WhereTakeOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderByDescending(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task WhereTakeOrderBySkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .OrderBy(o => o.IntData)
            .Skip(itemsCountToSkip)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task WhereTakeSkipOrderBy_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderBy(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInAscendingOrder(o => o.IntData);
    }

    public override async Task WhereTakeSkipOrderByDescendingSkip_ReturnOk()
    {
        // Arrange
        int itemsCountToInsert = 5;
        int itemsCountToTake = 3;
        int itemsCountToSkip = 2;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert, guid);

        // Act
        var itemsResult = await Collection.Query
            .Where(w => w.StringData == guid)
            .Take(itemsCountToTake)
            .Skip(itemsCountToSkip)
            .OrderByDescending(o => o.IntData)
            .ToListAsync();

        // Assert
        itemsResult
            .Should()
            .HaveCount(itemsCountToTake)
            .And
            .BeInDescendingOrder(o => o.IntData);
    }

    public override async Task ThenByDescending_AfterOrderByDescending_ReturnOk()
    {
        string expectedErrorMessage = "ORDER BY already defined in this query builder";

        var baseMethod = async () => await base.ThenByDescending_AfterOrderByDescending_ReturnOk();
        await baseMethod.Should().ThrowExactlyAsync<ArgumentException>(expectedErrorMessage); 
    }
}
#endif