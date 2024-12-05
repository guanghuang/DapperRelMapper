using System.Data;
using Dapper;

namespace Kvr.Dapper.MultipleQuery;

/// <summary>
/// This class is used to map the result of a query to a model with fourteen child relationships.
/// </summary>
/// <typeparam name="TParent">The type of the parent model.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TChild1">The type of the first child model.</typeparam>
/// <typeparam name="TChild2">The type of the second child model.</typeparam>
/// <typeparam name="TChild3">The type of the third child model.</typeparam>
/// <typeparam name="TChild4">The type of the fourth child model.</typeparam>
/// <typeparam name="TChild5">The type of the fifth child model.</typeparam>
/// <typeparam name="TChild6">The type of the sixth child model.</typeparam>
/// <typeparam name="TChild7">The type of the seventh child model.</typeparam>
/// <typeparam name="TChild8">The type of the eighth child model.</typeparam>
/// <typeparam name="TChild9">The type of the ninth child model.</typeparam>
/// <typeparam name="TChild10">The type of the tenth child model.</typeparam>
/// <typeparam name="TChild11">The type of the eleventh child model.</typeparam>
/// <typeparam name="TChild12">The type of the twelfth child model.</typeparam>
/// <typeparam name="TChild13">The type of the thirteenth child model.</typeparam>
/// <typeparam name="TChild14">The type of the fourteenth child model.</typeparam>
public class SqlMultipleQueryWrapper<TParent, TKey, TChild1, TChild2, TChild3, TChild4, TChild5, TChild6, TChild7, TChild8, TChild9, TChild10, TChild11, TChild12, TChild13, TChild14> : BaseSqlMultipleQueryWrapper<TParent, TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMultipleQueryWrapper{TParent, TKey, TChild1, TChild2, TChild3, TChild4, TChild5, TChild6, TChild7, TChild8, TChild9, TChild10, TChild11, TChild12, TChild13, TChild14}"/> class.
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
            case 6:
                await SetChildDataAsync<TChild7>(queryConfiguration, parent, reader);
                break;
            case 7:
                await SetChildDataAsync<TChild8>(queryConfiguration, parent, reader);
                break;
            case 8:
                await SetChildDataAsync<TChild9>(queryConfiguration, parent, reader);
                break;
            case 9:
                await SetChildDataAsync<TChild10>(queryConfiguration, parent, reader);
                break;
            case 10:
                await SetChildDataAsync<TChild11>(queryConfiguration, parent, reader);
                break;
            case 11:
                await SetChildDataAsync<TChild12>(queryConfiguration, parent, reader);
                break;
            case 12:
                await SetChildDataAsync<TChild13>(queryConfiguration, parent, reader);
                break;
            case 13:
                await SetChildDataAsync<TChild14>(queryConfiguration, parent, reader);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(childIndex), "Child index must be between 0 and 13");
        }
    }
} 