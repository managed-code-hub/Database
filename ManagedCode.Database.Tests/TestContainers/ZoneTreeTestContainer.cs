using System;
using System.IO;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.ZoneTree;

namespace ManagedCode.Database.Tests.TestContainers;

public class ZoneTreeTestContainer : ITestContainer<string, TestZoneTreeItem>
{
    private readonly ZoneTreeDatabase _database;

    public ZoneTreeTestContainer()
    {
        var databasePath  = Path.Combine(Environment.CurrentDirectory, $"zone{Guid.NewGuid():N}");

        _database = new ZoneTreeDatabase(new ZoneTreeOptions()
        {
            Path = databasePath,
        });
    }

    public IDatabaseCollection<string, TestZoneTreeItem> Collection =>
        _database.GetCollection<string, TestZoneTreeItem>();

    public string GenerateId()
    {
        return $"{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DeleteAsync();
    }
}