﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ManagedCode.Repository.CosmosDB
{
    public class CosmosDbRepository<TItem> : BaseRepository<string, TItem>
        where TItem : CosmosDbRepositoryItem, IRepositoryItem<string>, new()
    {
        private readonly CosmosDbAdapter<TItem> _cosmosDbAdapter;

        public CosmosDbRepository(string connectionString)
        {
            _cosmosDbAdapter = new CosmosDbAdapter<TItem>(connectionString);
        }

        public CosmosDbRepository(string connectionString, string databaseName, string collectionName)
        {
            _cosmosDbAdapter = new CosmosDbAdapter<TItem>(connectionString, databaseName, collectionName);
        }

        public CosmosDbRepository(string connectionString, CosmosClientOptions cosmosClientOptions)
        {
            _cosmosDbAdapter = new CosmosDbAdapter<TItem>(connectionString, cosmosClientOptions);
        }

        public CosmosDbRepository(string connectionString, CosmosClientOptions cosmosClientOptions, string databaseName, string collectionName)
        {
            _cosmosDbAdapter = new CosmosDbAdapter<TItem>(connectionString, cosmosClientOptions, databaseName, collectionName);
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            IsInitialized = true;
            return _cosmosDbAdapter.GetContainer();
        }

        #region Insert

        protected override async Task<bool> InsertAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var result = await container.CreateItemAsync(item, item.PartitionKey, cancellationToken: token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();

                var batch = new List<Task>(10);
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
            catch (Exception e)
            {
                return count;
            }
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<bool> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var result = await container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var count = 0;
                var batch = new List<Task>(10);
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
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region Update

        protected override async Task<bool> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var result = await container.ReplaceItemAsync(item, item.Id, cancellationToken: token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var count = 0;
                var batch = new List<Task>(10);
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
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(string id, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var item = await GetAsync(g => g.Id == id, token);
                if (item == null)
                {
                    return false;
                }

                var result = await container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var result = await container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<string> ids, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var count = 0;
                var batch = new List<Task>(10);
                foreach (var item in ids)
                {
                    token.ThrowIfCancellationRequested();
                    batch.Add(DeleteAsync(item, token)
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
            catch (Exception e)
            {
                return 0;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                var container = await _cosmosDbAdapter.GetContainer();
                var count = 0;
                var batch = new List<Task>(10);
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
            catch (Exception e)
            {
                return 0;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            try
            {
                var count = 0;
                var container = await _cosmosDbAdapter.GetContainer();
                var feedIterator = container.GetItemLinqQueryable<TItem>()
                    .Where(predicate)
                    .ToFeedIterator();

                var batch = new List<Task>(10);
                using (var iterator = feedIterator)
                {
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
                }

                return count;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
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

        #endregion

        #region Find

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            var container = await _cosmosDbAdapter.GetContainer();
            var query = container.GetItemLinqQueryable<TItem>().Where(predicate);

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

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            var container = await _cosmosDbAdapter.GetContainer();
            var query = container.GetItemLinqQueryable<TItem>().Where(predicate);

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

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            var container = await _cosmosDbAdapter.GetContainer();
            var query = container.GetItemLinqQueryable<TItem>().Where(predicate);

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

        protected override async Task<uint> CountAsyncInternal(CancellationToken token = default)
        {
            var container = await _cosmosDbAdapter.GetContainer();
            return Convert.ToUInt32(await container.GetItemLinqQueryable<TItem>().CountAsync(token));
        }

        protected override async Task<uint> CountAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            var container = await _cosmosDbAdapter.GetContainer();
            return Convert.ToUInt32(await container.GetItemLinqQueryable<TItem>().Where(predicate).CountAsync(token));
        }

        #endregion
    }
}