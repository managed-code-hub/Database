using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem>
    where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _connection;

    public SQLiteDBCollectionQueryable(SQLiteConnection connection)
    {
        _connection = connection;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        foreach (var item in ApplyPredicates(Predicates))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var query = ApplyPredicates(Predicates.Where(p => p.QueryType is QueryType.Where));

        return await Task.Run(() => query.FirstOrDefault(), cancellationToken);
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = ApplyPredicates(Predicates.Where(p => p.QueryType is QueryType.Where));

        return await Task.Run(() => query.LongCount(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        int count = 0;

        foreach (var item in ApplyPredicates(Predicates.Where(p => p.QueryType is QueryType.Where)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            _connection.Delete<TItem>(item);
            count++;
        }

        return count;
    }


    private TableQuery<TItem> ApplyPredicates(IEnumerable<QueryItem> predicates)
    {
        var query = _connection.Table<TItem>();

        // TODO: fix OrderBy
        foreach (var predicate in predicates)
        {
            query = predicate.QueryType switch
            {
                QueryType.Where => query.Where(predicate.ExpressionBool),
                QueryType.OrderBy => query.OrderBy(predicate.ExpressionObject),
                QueryType.OrderByDescending => query.OrderByDescending(predicate.ExpressionObject),
                QueryType.ThenBy => query.ThenBy(predicate.ExpressionObject),
                QueryType.ThenByDescending => query.ThenByDescending(predicate.ExpressionObject),
                QueryType.Take => predicate.Count.HasValue ? query.Take(predicate.Count.Value) : query,
                QueryType.Skip => query.Skip(predicate.Count!.Value),
                _ => query
            };
        }

        return query;
    }
}