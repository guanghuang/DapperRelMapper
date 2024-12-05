using System.Data;
using Dapper;

namespace Kvr.Dapper.MultipleQuery;

/// <summary>
/// This class is used to map the result of a query to a model.
/// </summary>
/// <typeparam name="TParent">The type of the parent model.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TChild">The type of the child model.</typeparam>   
public class SqlMultipleQueryWrapper<TParent, TKey, TChild>: BaseSqlMultipleQueryWrapper<TParent, TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMultipleQueryWrapper{TParent, TKey, TChild}"/> class.
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
        if (childIndex != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(childIndex), "Child index must be 0");
        }
        await SetChildDataAsync<TChild>(queryConfiguration, parent, reader);
    }
}