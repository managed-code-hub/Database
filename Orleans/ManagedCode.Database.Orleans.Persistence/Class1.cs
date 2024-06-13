using System.Net;
using ManagedCode.Database.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace ManagedCode.Database.Orleans.Persistence;

public class DatabaseGrainStorage<TId, TItem> : IGrainStorage, IRestExceptionDecoder, ILifecycleParticipant<ISiloLifecycle> where TItem : IItem<TId>
{
    private readonly IDatabase _database;
    private readonly IDatabaseCollection<TId, TItem> _collection;

    public DatabaseGrainStorage(
        string name,
      //  AzureTableStorageOptions options,
        IOptions<ClusterOptions> clusterOptions,
        IServiceProvider services,
      //  ILogger<AzureTableGrainStorage> logger,
        IDatabase database,
        IDatabaseCollection<TId, TItem> collection
        
        )
    {
        _database = database;
        _collection = collection;
        // this.options = options;
        // this.clusterOptions = clusterOptions.Value;
        // this.name = name;
        // this.storageSerializer = options.GrainStorageSerializer;
        // this.logger = logger;
    }
    
    public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        //_collection.GetAsync()
        return Task.CompletedTask;
    }

    public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        throw new NotImplementedException();
    }

    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        throw new NotImplementedException();
    }

    public bool DecodeException(Exception exception, out HttpStatusCode httpStatusCode, out string restStatus, bool getExtendedErrors = false)
    {
        throw new NotImplementedException();
    }

    public void Participate(ISiloLifecycle observer)
    {
        throw new NotImplementedException();
    }
}