using System;
using System.IO;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.TestContainers;

public class LiteDBTestContainer : ITestContainer<string, TestLiteDBItem>
{
    private readonly LiteDBDatabase _database;
    private readonly string _databasePath;

    public LiteDBTestContainer()
    {
        _databasePath = Path.Combine(Environment.CurrentDirectory, $"{Guid.NewGuid():N}.db");

        _database = new LiteDBDatabase(new LiteDBOptions
        {
            ConnectionString = _databasePath,
        });
    }

    public IDatabaseCollection<string, TestLiteDBItem> Collection =>
        _database.GetCollection<string, TestLiteDBItem>();

    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        File.Delete(_databasePath);
    }
}