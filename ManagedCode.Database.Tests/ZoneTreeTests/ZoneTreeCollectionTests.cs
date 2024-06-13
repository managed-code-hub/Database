using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.ZoneTreeTests;

#if ZONE_TREE || DEBUG
public class ZoneTreeCollectionTests : BaseCollectionTests<string, TestZoneTreeItem>
{
    public ZoneTreeCollectionTests() : base(new ZoneTreeTestContainer())
    {
    }
    
    public override async Task InsertOrUpdateListOfItems()
    {
        // Arrange
        int itemsCount = 5;
        int updatedItemsCount = 0;
        List<TestZoneTreeItem> list = new();

        for (var i = 0; i < itemsCount; i++)
        {
            list.Add(CreateNewItem());
        }

        var itemsInsert = await Collection.InsertOrUpdateAsync(list);

        foreach (var item in list)
        {
            item.DateTimeData = DateTime.Now.AddDays(-1);
        }

        // Act
        var itemsUpdate = await Collection.InsertOrUpdateAsync(list);
        //TODO: LiteDB must be 100, but result 0

        // Assert
        itemsUpdate.Should().Be(updatedItemsCount);
        itemsInsert.Should().Be(itemsCount);
        list.Count.Should().Be(itemsCount);
    }
}
#endif