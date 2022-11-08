using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableAdapter<T> // where T : IItem<TableId>
{
    private readonly bool _allowTableCreation = true;
    private readonly CloudStorageAccount _cloudStorageAccount;
    private readonly int _retryCount = 25;
    private readonly object _sync = new();
    private CloudTableClient _cloudTableClient;
    private bool _tableClientInitialized;

    public AzureTableAdapter(string connectionString)
    {
        _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
    }

    public AzureTableAdapter(StorageCredentials tableStorageCredentials, StorageUri tableStorageUri)
    {
        var cloudStorageAccount = new CloudStorageAccount(tableStorageCredentials, tableStorageUri);
        _cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
    }

    public int BatchSize { get; } = 100;

    private string GetTableName()
    {
        return typeof(T).Name.Pluralize();
    }

    public Task<bool> DropTable(CancellationToken token)
    {
        var table = GetTable();
        _tableClientInitialized = false;
        return table.DeleteIfExistsAsync(token);
    }

    private CloudTable GetTable()
    {
        lock (_sync)
        {
            if (!_tableClientInitialized)
            {
                _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
                var table = _cloudTableClient.GetTableReference(GetTableName());

                if (_allowTableCreation)
                {
                    table.CreateIfNotExists();
                }
                else
                {
                    var exists = table.Exists();
                    if (!exists)
                    {
                        throw new Exception($"Table '{GetTableName()}' does not exist");
                    }
                }

                _tableClientInitialized = true;
                return table;
            }
        }

        return _cloudTableClient.GetTableReference(GetTableName());
    }

    public async Task<T> ExecuteAsync<T>(TableOperation operation, CancellationToken token = default) where T : class
    {
        var table = GetTable();
        var retryCount = _retryCount;
        do
        {
            try
            {
                var result = await table.ExecuteAsync(operation, token).ConfigureAwait(false);
                return result.Result as T;
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.TooManyRequests)
            {
                retryCount--;
                if (retryCount == 0)
                {
                    throw;
                }

                await WaitRandom(token);
            }
        } while (retryCount > 0);

        throw new Exception(nameof(HttpStatusCode.TooManyRequests));
    }

    public async Task<object> ExecuteAsync(TableOperation operation, CancellationToken token = default)
    {
        var table = GetTable();
        var retryCount = _retryCount;
        do
        {
            try
            {
                var result = await table.ExecuteAsync(operation, token).ConfigureAwait(false);
                return result.Result;
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.TooManyRequests)
            {
                retryCount--;
                if (retryCount == 0)
                {
                    throw;
                }

                await WaitRandom(token);
            }
        } while (retryCount > 0);

        throw new Exception(nameof(HttpStatusCode.TooManyRequests));
    }

    public async Task<int> ExecuteBatchAsync(IEnumerable<TableOperation> operations, CancellationToken token = default)
    {
        var table = GetTable();
        var totalCount = 0;
        foreach (var group in operations.GroupBy(o => o.Entity.PartitionKey))
        {
            token.ThrowIfCancellationRequested();
            var batchOperation = new TableBatchOperation();
            foreach (var item in group)
            {
                batchOperation.Add(item);
                if (batchOperation.Count == BatchSize)
                {
                    var retryCount = _retryCount;
                    do
                    {
                        try
                        {
                            var result = await table.ExecuteBatchAsync(batchOperation, token).ConfigureAwait(false);
                            totalCount += result.Count;
                            batchOperation = new TableBatchOperation();
                            break;
                        }
                        catch (StorageException e) when (e.RequestInformation.HttpStatusCode ==
                                                         (int)HttpStatusCode.TooManyRequests)
                        {
                            retryCount--;
                            if (retryCount == 0)
                            {
                                throw;
                            }

                            await WaitRandom(token);
                        }
                    } while (retryCount > 0);
                }

                token.ThrowIfCancellationRequested();
            }

            if (batchOperation.Count > 0)
            {
                var retryCount = _retryCount;
                do
                {
                    try
                    {
                        var result = await table.ExecuteBatchAsync(batchOperation, token).ConfigureAwait(false);
                        totalCount += result.Count;
                        break;
                    }
                    catch (StorageException e) when (e.RequestInformation.HttpStatusCode ==
                                                     (int)HttpStatusCode.TooManyRequests)
                    {
                        retryCount--;
                        if (retryCount == 0)
                        {
                            throw;
                        }

                        await WaitRandom(token);
                    }
                } while (retryCount > 0);
            }
        }

        return totalCount;
    }

    public async IAsyncEnumerable<T> Query<T>(
        List<Expression<Func<T, bool>>> WherePredicates,
        List<Expression<Func<T, object>>> OrderByPredicates,
        List<Expression<Func<T, object>>> OrderByDescendingPredicates,
        Expression<Func<T, object>> selectExpression = null,
        int? take = null,
        int? skip = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : ITableEntity, new()
    {
        string filterString = null;

        var whereString = new List<string>();
        foreach (var expression in WherePredicates)
        {
            whereString.Add(AzureTableQueryTranslator.TranslateExpression(expression));
        }

        filterString = string.Join(" and ", whereString);

        List<string> selectedProperties = null;
        if (selectExpression != null)
        {
            selectedProperties = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(selectExpression);
        }

        TableContinuationToken token = null;
        var table = GetTable();

        var takeCount = 0;

        takeCount = take ?? 1000;

        if (skip.HasValue)
        {
            takeCount += skip.Value;
        }

        var query = new TableQuery<T>
        {
            FilterString = filterString,
            TakeCount = takeCount > 1000 ? 1000 : takeCount
        };

        if (selectedProperties != null)
        {
            query.Select(selectedProperties);
        }

        if (OrderByPredicates.Count > 0)
        {
            foreach (var item in OrderByPredicates)
            {
                var names = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(item);
                foreach (var name in names)
                {
                    query.OrderBy(name);
                }
            }
        }

        if (OrderByDescendingPredicates.Count > 0)
        {
            foreach (var item in OrderByDescendingPredicates)
            {
                var names = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(item);
                foreach (var name in names)
                {
                    query.OrderByDesc(name);
                }
            }
        }

        long count = 0;
        long skipCount = 0;

        do
        {
            TableQuerySegment<T> results = null;
            var retryCount = _retryCount;
            do
            {
                try
                {
                    results = await table.ExecuteQuerySegmentedAsync(query, token, cancellationToken)
                        .ConfigureAwait(false);
                    break;
                }
                catch (StorageException e) when (e.RequestInformation.HttpStatusCode ==
                                                 (int)HttpStatusCode.TooManyRequests)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        throw;
                    }

                    await WaitRandom(cancellationToken);
                }
            } while (retryCount > 0);

            token = results.ContinuationToken;

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var item in results.Results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (skip > 0 && skipCount < skip)
                {
                    skipCount++;
                    continue;
                }

                count++;
                yield return item;

                if (take > 0 && count >= take)
                {
                    break;
                }
            }

            if (take > 0 && count >= take)
            {
                break;
            }
        } while (token != null);
    }

    private Task WaitRandom(CancellationToken token)
    {
        return Task.Delay(new Random().Next(1000, 3500), token);
    }
}