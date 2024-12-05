using System.Data;
using Dapper;

namespace Kvr.Dapper.MultipleQuery;

/// <summary>
/// This class is used to map the result of a query to a model with six child relationships.
/// </summary>
/// <typeparam name="TParent">The type of the parent model.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TChild1">The type of the first child model.</typeparam>
/// <typeparam name="TChild2">The type of the second child model.</typeparam>
/// <typeparam name="TChild3">The type of the third child model.</typeparam>
/// <typeparam name="TChild4">The type of the fourth child model.</typeparam>
/// <typeparam name="TChild5">The type of the fifth child model.</typeparam>
/// <typeparam name="TChild6">The type of the sixth child model.</typeparam>
public class SqlMultipleQueryWrapper<TParent, TKey, TChild1, TChild2, TChild3, TChild4, TChild5, TChild6> : BaseSqlMultipleQueryWrapper<TParent, TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMultipleQueryWrapper{TParent, TKey, TChild1, TChild2, TChild3, TChild4, TChild5, TChild6}"/> class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    public SqlMultipleQueryWrapper(IDbConnection connection) : base(connection)
    {
    }

    /// <summary>
    /// Sets the child data asynchronously.
    /// </summary>
    /// <param name="queryConfiguration">The query configuration.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="reader">The reader.</param>
    /// <param name="childIndex">The child index.</param>
    protected override async Task SetChildDataAsync(QueryConfiguration queryConfiguration, TParent parent, SqlMapper.GridReader reader, int childIndex)
    {
        switch (childIndex)
        {
            case 0:
                await SetChildDataAsync<TChild1>(queryConfiguration, parent, reader);
                break;
            case 1:
                await SetChildDataAsync<TChild2>(queryConfiguration, parent, reader);
                break;
            case 2:
                await SetChildDataAsync<TChild3>(queryConfiguration, parent, reader);
                break;
            case 3:
                await SetChildDataAsync<TChild4>(queryConfiguration, parent, reader);
                break;
            case 4:
                await SetChildDataAsync<TChild5>(queryConfiguration, parent, reader);
                break;
            case 5:
                await SetChildDataAsync<TChild6>(queryConfiguration, parent, reader);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(childIndex), "Child index must be between 0 and 5");
        }
    }
} 