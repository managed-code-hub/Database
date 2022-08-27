﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDatabase : BaseDatabase, IDatabase<CosmosClient>
{
    private readonly CosmosDbRepositoryOptions _options;

    public CosmosDatabase([NotNull] CosmosDbRepositoryOptions options)
    {
        _options = options;
    }
    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
    
    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
    }

    protected override IDBCollection<TId, TItem> GetCollectionInternal<TId, TItem>(string name)
    {
        return new CosmosDBCollection<TItem>(_options);
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public CosmosClient DataBase { get; }
}

public class CosmosDBCollection<TId, TItem> : BaseDBCollection<TId, TItem>
    where TItem : CosmosDbItem, IItem<TId>, new()
{
    private readonly CosmosDbAdapter<TItem> _cosmosDbAdapter;
    private readonly bool _splitByType;
    private readonly int _capacity = 50;
    private readonly bool _useItemIdAsPartitionKey;

    public CosmosDBCollection([NotNull] CosmosDbRepositoryOptions options)
    {
        _splitByType = options.SplitByType;
        _useItemIdAsPartitionKey = options.UseItemIdAsPartitionKey;
        _cosmosDbAdapter = new CosmosDbAdapter<TItem>(options.ConnectionString, options.CosmosClientOptions, options.DatabaseName, options.CollectionName);
    }

    private Expression<Func<TItem, bool>> SplitByType()
    {
        if (_splitByType)
        {
            return w => w.Type == typeof(TItem).Name;
        }

        return w => true;
    }
    

    #region Insert

    public override void Dispose()
    {
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();

        var result = await container.CreateItemAsync(item, item.PartitionKey, cancellationToken: token);
        return result.Resource;
    }

    protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;

        var container = await _cosmosDbAdapter.GetContainer();

        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.CreateItemAsync(item, item.PartitionKey, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: token);
        return result.Resource;
    }

    protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.ReplaceItemAsync(item, item.Id, cancellationToken: token);
        return result.Resource;
    }

    protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.ReplaceItemAsync(item, item.Id, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();

        if (_useItemIdAsPartitionKey)
        {
            var deleteItemResult = await container.DeleteItemAsync<TItem>(id, new PartitionKey(id), cancellationToken: token);
            return deleteItemResult != null;
        }
        
        var item = await GetAsync(g => g.Id == id, token);
        if (item == null)
        {
            return false;
        }

        var result = await container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
        return result != null;
    }

    protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
        return result != null;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<string> ids, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in ids)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(DeleteAsync(item, token)
                .ContinueWith(task =>
                {
                    if (task.Result)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
    {
        var count = 0;
        var container = await _cosmosDbAdapter.GetContainer();
        var feedIterator = container.GetItemLinqQueryable<TItem>()
            .Where(SplitByType())
            .Where(predicate)
            .ToFeedIterator();

        var batch = new List<Task>(_capacity);
        using var iterator = feedIterator;
        while (iterator.HasMoreResults)
        {
            token.ThrowIfCancellationRequested();

            foreach (var item in await iterator.ReadNextAsync(token))
            {
                token.ThrowIfCancellationRequested();
                batch.Add(container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token)
                    .ContinueWith(task =>
                    {
                        if (task.Result != null)
                        {
                            Interlocked.Increment(ref count);
                        }
                    }, token));

                if (count == batch.Capacity)
                {
                    await Task.WhenAll(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        return count;
    }

    protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        if (_splitByType)
        {
            return await DeleteAsyncInternal(item => true, token) > 0;
        }

        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.DeleteContainerAsync(cancellationToken: token);
        return result != null;
    }

    #endregion

    #region Get

    protected override async Task<TItem> GetAsyncInternal(string id, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var feedIterator = container.GetItemLinqQueryable<TItem>()
            .Where(w => w.Id == id)
            .ToFeedIterator();
        using (var iterator = feedIterator)
        {
            if (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    return item;
                }
            }
        }

        return null;
    }

    protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var feedIterator = container.GetItemLinqQueryable<TItem>()
            .Where(SplitByType())
            .Where(predicate)
            .ToFeedIterator();
        using (var iterator = feedIterator)
        {
            if (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    return item;
                }
            }
        }

        return null;
    }

    protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var query = container.GetItemLinqQueryable<TItem>().Where(SplitByType());

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        var feedIterator = query.ToFeedIterator();
        using (var iterator = feedIterator)
        {
            while (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    yield return item;
                }
            }
        }
    }

    protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(Expression<Func<TItem, object>> orderBy,
        Order orderType,
        int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var query = container.GetItemLinqQueryable<TItem>().Where(SplitByType());

        if (orderType == Order.By)
        {
            query = query.OrderBy(orderBy);
        }
        else
        {
            query = query.OrderByDescending(orderBy);
        }

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        var feedIterator = query.ToFeedIterator();
        using (var iterator = feedIterator)
        {
            while (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    yield return item;
                }
            }
        }
    }

    #endregion

    #region Find

    protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var query = container.GetItemLinqQueryable<TItem>().Where(SplitByType());

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        var feedIterator = query.ToFeedIterator();
        using (var iterator = feedIterator)
        {
            while (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    yield return item;
                }
            }
        }
    }

    protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();

        var query = container.GetItemLinqQueryable<TItem>().Where(SplitByType());

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        if (orderType == Order.By)
        {
            query = query.OrderBy(orderBy);
        }
        else
        {
            query = query.OrderByDescending(orderBy);
        }

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        var feedIterator = query.ToFeedIterator();
        using (var iterator = feedIterator)
        {
            while (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    yield return item;
                }
            }
        }
    }

    protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        Expression<Func<TItem, object>> thenBy,
        Order thenType,
        int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var query = container.GetItemLinqQueryable<TItem>().Where(SplitByType());

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        IOrderedQueryable<TItem> ordered;
        if (orderType == Order.By)
        {
            ordered = query.OrderBy(orderBy);
        }
        else
        {
            ordered = query.OrderByDescending(orderBy);
        }

        if (thenType == Order.By)
        {
            query = ordered.ThenBy(thenBy);
        }
        else
        {
            query = ordered.ThenByDescending(thenBy);
        }

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        var feedIterator = query.ToFeedIterator();
        using (var iterator = feedIterator)
        {
            while (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    yield return item;
                }
            }
        }
    }

    #endregion

    #region Count

    protected override async Task<int> CountAsyncInternal(CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        return await container.GetItemLinqQueryable<TItem>().Where(SplitByType()).CountAsync(token);
    }

    protected override async Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var query = container.GetItemLinqQueryable<TItem>().Where(SplitByType());

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync(token);
    }

    #endregion
}