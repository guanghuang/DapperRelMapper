using System.Data;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Dapper;

namespace Kvr.Dapper.MultipleQuery;

/// <summary>
/// This class is used to map the result of a query to a model.
/// </summary>
public record QueryConfiguration(string SqlForChild, LambdaExpression? Expression, string? SplitOn, LambdaExpression? KeySelector, LambdaExpression[]? RelExpressions);

/// <summary>
/// This class is used to map the result of a query to a model.
/// </summary>
public abstract class BaseSqlMultipleQueryWrapper<TReturn, TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSqlMultipleQueryWrapper{TReturn, TKey}"/> class.
    /// </summary>
    private readonly IDbConnection _connection;

    /// <summary>
    /// The query configuration for the parent.
    /// </summary>
    private QueryConfiguration _queryConfigurationForParent;

    /// <summary>
    /// The query configurations for the children.
    /// </summary>
    private readonly List<QueryConfiguration> _childQueryConfigs = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSqlMultipleQueryWrapper{TReturn, TKey}"/> class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    protected BaseSqlMultipleQueryWrapper(IDbConnection connection)
    {
        _connection = connection;
    }
    
    /// <summary>
    /// Configures the parent query.
    /// </summary>
    /// <param name="sqlByKeyForParent">The SQL query for the parent.</param>
    /// <param name="splitOn">The field to split the results on.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="relExpressions">The relationship expressions.</param>
    /// <returns>The configured parent query wrapper.</returns>
    public BaseSqlMultipleQueryWrapper<TReturn, TKey> ConfigParent(string sqlByKeyForParent, string? splitOn = null, Expression<Func<TReturn, TKey>>? keySelector = null, LambdaExpression[]? relExpressions = null)
    {
        _queryConfigurationForParent = new QueryConfiguration(sqlByKeyForParent, null, splitOn, keySelector, relExpressions);
        return this;
    }
    
    /// <summary>
    /// Configures the child query.
    /// </summary>
    /// <typeparam name="TChild">The type of the child.</typeparam>
    /// <typeparam name="TCKey">The type of the child key.</typeparam>
    /// <param name="sqlByKeyForChild">The SQL query for the child.</param>
    /// <param name="childSelector">The child selector.</param>
    /// <param name="splitOn">The field to split the results on.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="relExpressions">The relationship expressions.</param>
    /// <returns>The configured child query wrapper.</returns>
    public BaseSqlMultipleQueryWrapper<TReturn, TKey> ConfigChild<TChild, TCKey>(string sqlByKeyForChild,
        Expression<Func<TReturn, TChild>> childSelector, string? splitOn = null, Expression<Func<TChild, TCKey>>? keySelector = null, LambdaExpression[]? relExpressions = null)
    {
        _childQueryConfigs.Add(new QueryConfiguration(sqlByKeyForChild, childSelector, splitOn, keySelector, relExpressions));
        return this;
    }

    /// <summary>
    /// Configures the child query.
    /// </summary>
    /// <typeparam name="TChild">The type of the child.</typeparam>
    /// <param name="sqlByKeyForChild">The SQL query for the child.</param>
    /// <param name="childSelector">The child selector.</param>
    /// <param name="splitOn">The field to split the results on.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="relExpressions">The relationship expressions.</param>
    /// <returns>The configured child query wrapper.</returns>
    public BaseSqlMultipleQueryWrapper<TReturn, TKey> ConfigChild<TChild>(string sqlByKeyForChild,
        Expression<Func<TReturn, TChild>> childSelector, string? splitOn = null, Expression<Func<TChild, object>>? keySelector = null, LambdaExpression[]? relExpressions = null)
    {
        _childQueryConfigs.Add(new QueryConfiguration(sqlByKeyForChild, childSelector, splitOn, keySelector, relExpressions));
        return this;
    }

    /// <summary>
    /// Configures the child query.
    /// </summary>
    /// <typeparam name="TChild">The type of the child.</typeparam>
    /// <typeparam name="TCKey">The key of the child</typeparam>
    /// <param name="sqlByKeyForChild">The SQL query for the child.</param>
    /// <param name="childSelector">The child selector.</param>
    /// <param name="splitOn">The field to split the results on.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="relExpressions">The relationship expressions.</param>
    /// <returns>The configured child query wrapper.</returns>
    public BaseSqlMultipleQueryWrapper<TReturn, TKey> ConfigChild<TChild, TCKey>(string sqlByKeyForChild,
        Expression<Func<TReturn, ICollection<TChild>>> childSelector, string? splitOn = null, Expression<Func<TChild, TCKey>>? keySelector = null, LambdaExpression[]? relExpressions = null)
    {
        _childQueryConfigs.Add(new QueryConfiguration(sqlByKeyForChild, childSelector, splitOn, keySelector, relExpressions));
        return this;
    }

    public BaseSqlMultipleQueryWrapper<TReturn, TKey> ConfigChild<TChild>(string sqlByKeyForChild,
        Expression<Func<TReturn, ICollection<TChild>>> childSelector, string? splitOn = null, Expression<Func<TChild, object>>? keySelector = null, LambdaExpression[]? relExpressions = null)
    {
        _childQueryConfigs.Add(new QueryConfiguration(sqlByKeyForChild, childSelector, splitOn, keySelector, relExpressions));
        return this;
    }

    public async Task<TReturn> QueryAsync(TKey key, IDbTransaction? transaction = null, int? commandTimeout = null,
        CommandType? commandType = null)
    {
        // Create parameters for all queries using the same key value
        var param = CreateDynamicParameters(
            _childQueryConfigs.Select(c => GetFirstParameterName(c.SqlForChild)).Distinct().Where(n => n != null), key);
        var reader = await _connection.QueryMultipleAsync(GenerateMultipleQuerySql(), (object)param, transaction,
            commandTimeout, commandType);

        var parent = (await ReadFromReaderAsync<TReturn>(reader, _queryConfigurationForParent)).FirstOrDefault();

        if (parent != null)
        {
            for(int i = 0; i < _childQueryConfigs.Count; i++)
            {
                await SetChildDataAsync(_childQueryConfigs[i], parent, reader, i);
            }
        }

        return parent;
    }

    /// <summary>
    /// Sets the child data asynchronously.
    /// </summary>
    /// <param name="queryConfiguration">The query configuration.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="reader">The reader.</param>
    /// <param name="childIndex">The child index.</param>
    protected abstract Task SetChildDataAsync(QueryConfiguration queryConfiguration, TReturn parent, SqlMapper.GridReader reader, int childIndex);
    
    /// <summary>
    /// Sets the child data asynchronously.
    /// </summary>
    /// <typeparam name="TChild">The type of the child.</typeparam>
    /// <param name="queryConfiguration">The query configuration.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="reader">The reader.</param>
    protected async Task SetChildDataAsync<TChild>(QueryConfiguration queryConfiguration, TReturn parent, SqlMapper.GridReader reader)
    {
        var memberExpression = queryConfiguration.Expression!.GetMemberExpression();
        var childrenData = await ReadFromReaderAsync<TChild>(reader, queryConfiguration);
        if (Utils.IsCollectionType(memberExpression.Type))
        {
            // Handle collection properties by creating a new List<T> and adding children
            Utils.SetPropertyValue(parent, memberExpression,
                Activator.CreateInstance(typeof(List<TChild>)));
            foreach (var childData in childrenData)
            {
                Utils.GetPropertyValue<TReturn, List<TChild>>(parent, memberExpression)
                    .Add(childData);
            }
        }
        else
        {
            // Handle single-valued properties by taking the first child
            Utils.SetPropertyValue(parent, memberExpression, childrenData.FirstOrDefault());
        }
    }
    
    /// <summary>
    /// Reads the data from the reader.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="queryConfiguration">The query configuration.</param>
    /// <returns>The data.</returns>
    private async Task<IEnumerable<T>> ReadFromReaderAsync<T>(SqlMapper.GridReader reader, QueryConfiguration queryConfiguration)
    {
        var lookup = new Dictionary<object, T>();
        IEnumerable<T> data;
        if (string.IsNullOrEmpty(queryConfiguration.SplitOn))
        {
            data = await reader.ReadAsync<T>(false);
        }
        else
        {
            var splitOnModel = MapperHelper.GetSplitOnModel(queryConfiguration.KeySelector!, queryConfiguration.RelExpressions!, lookup);
            // must use ToList to eagerly read the data, otherwise the reader will be disposed and no data will be returned
            reader.Read(splitOnModel.Types, splitOnModel.Func, queryConfiguration.SplitOn!, false).ToList();
            data = lookup.Values;
        }

        return data;
    }
    
    /// <summary>
    /// Combines all SQL queries into a single multi-query statement.
    /// </summary>
    private string GenerateMultipleQuerySql()
    {
        return _queryConfigurationForParent.SqlForChild + ";" + string.Join(";", _childQueryConfigs.Select(c => c.SqlForChild)) + ";";
    }

    /// <summary>
    /// Extracts the first parameter name from a SQL query.
    /// </summary>
    /// <param name="sql">The SQL query to parse.</param>
    /// <returns>The first parameter name found, or null if none found.</returns>
    private static string? GetFirstParameterName(string sql)
    {
        var regex = new Regex(@"@([\w\d_]+)(?![.\w\d_]@)", RegexOptions.IgnoreCase);
        var match = regex.Match(sql);

        // Return the captured group (without @)
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Converts a list of strings into a dynamic object where each string becomes a property with a specified value.
    /// </summary>
    /// <param name="keys">The parameter names to include</param>
    /// <param name="keyValue">The value to assign to each parameter</param>
    /// <returns>A dynamic object containing the parameters</returns>
    private static dynamic CreateDynamicParameters(IEnumerable<string?> keys, object keyValue)
    {
        var expando = new ExpandoObject() as IDictionary<string, object>;
        foreach (var key in keys)
        {
            expando[key!] = keyValue;
        }
        return expando;
    }    
}

