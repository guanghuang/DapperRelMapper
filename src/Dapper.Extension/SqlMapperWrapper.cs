// Copyright © 2024 Kvr.DapperRelMapper. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Data;
using System.Linq.Expressions;
using Dapper;

namespace Kvr.Dapper;

/// <summary>
/// Provides wrapper functionality for Dapper SQL mapping with support for complex object relationships
/// </summary>
/// <typeparam name="TReturn">The type of the main entity to be returned</typeparam>
/// <typeparam name="TKey">The type of the key used for entity lookup</typeparam>
public class SqlMapperWrapper<TReturn, TKey> where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of SqlMapperWrapper
    /// </summary>
    private readonly IDbConnection _connection;
    /// <summary>   
    /// Collection of expressions defining relationships
    /// </summary>
    private readonly LambdaExpression[] _expressions;
    /// <summary>
    /// Expression to select the key property
    /// </summary>
    private readonly Expression<Func<TReturn, TKey>> _keySelector;
    /// <summary>
    /// SplitOn parameter for the query
    /// </summary>
    private string? _splitOn;
    /// <summary>
    /// Post-processing action
    /// </summary>
    private Action<TReturn> _postProcess;

    /// <summary>
    /// Initializes a new instance of SqlMapperWrapper
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="expressions">Collection of expressions defining relationships</param>
    public SqlMapperWrapper(IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        LambdaExpression[] expressions)
    {
        _expressions = expressions;
        _keySelector = keySelector;
        _connection = connection;
    }

    /// <summary>
    /// Sets the post-processing action
    /// </summary>
    /// <param name="postProcess">Action to perform on each returned object</param>
    /// <returns>The SqlMapperWrapper instance</returns>
    public SqlMapperWrapper<TReturn, TKey> PostProcess(Action<TReturn> postProcess)
    {
        _postProcess = postProcess;
        return this;
    }

    /// <summary>
    /// Executes the query and maps the results to strongly typed objects with support for nested relationships
    /// </summary>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<object[]>? callbackAfterMapRow = null)
    {
        var lookup = new Dictionary<TKey, TReturn>();
        var memberExpressions = _expressions.Select(e => e.GetMemberExpression()).ToArray();
        var types = new[] { typeof(TReturn) }.Concat(memberExpressions.Select(e => e.GetMapType())).ToArray();

        await _connection.QueryAsync(sql, types, objects =>
        {
            var result = (TReturn)objects[0];
            var key = Utils.GetPropertyValue(result, _keySelector);
            var newRecord = !lookup.ContainsKey(key);
            if (newRecord)
            {
                lookup.Add(key, result);
            }
            else
            {
                result = lookup[key];
            }

            for (var i = 1; i < objects.Length; i++)
            {
                var memberExpression = memberExpressions[i - 1];
                if (memberExpression.Member.DeclaringType != typeof(TReturn))
                {
                    continue;
                }

                if (Utils.IsCollectionType(memberExpression.Type))
                {
                    if (newRecord)
                    {
                        Utils.SetPropertyValue(result, memberExpression,
                            Activator.CreateInstance(typeof(List<>).MakeGenericType(types[i])));
                    }

                    if (objects[i] != null)
                    {
                        Utils.GetPropertyValue<TReturn, IList>(result, memberExpression).Add(objects[i]);
                    }
                }
                else
                {
                    Utils.SetPropertyValue(result, memberExpression, objects[i]);
                }
            }

            callbackAfterMapRow?.Invoke(objects);
            return result;
        }, param, transaction, buffered, _splitOn ?? splitOn, commandTimeout, commandType);
        foreach (var item in lookup.Values)
        {
            _postProcess?.Invoke(item);
        }
        return lookup.Values;
    }
    
    /// <summary>
    /// Sets the splitOn parameter for the query
    /// </summary>
    /// <param name="expressions">Expressions defining the splitOn fields</param>
    /// <returns>The SqlMapperWrapper instance</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn(params LambdaExpression[] expressions)
    {
        _splitOn = (_splitOn == null ? "" : _splitOn + ",") + string.Join(",", expressions.Select(e => e.GetMemberExpression().Member.Name));
        return this;
    }

    /// <summary>
    /// Sets the splitOn parameter for the query
    /// </summary>
    /// <typeparam name="T">The type of the expression</typeparam>
    /// <typeparam name="K">The type of the splitOn field</typeparam>
    /// <param name="expression">The expression defining the splitOn field</param>
    /// <param name="repeat">The number of times to repeat the splitOn field</param>
    /// <returns>The SqlMapperWrapper instance</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, K>(Expression<Func<T, K>> expression, int repeat = 1)
    {
        var memberName = expression.GetMemberExpression().Member.Name;
        var repeatedNames = string.Join(",", Enumerable.Repeat(memberName, repeat));
        _splitOn = (_splitOn == null ? "" : _splitOn + ",") + repeatedNames;
        return this;
    }
    
    /// <summary>
    /// Sets the splitOn parameter for the query
    /// </summary>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="repeat">The number of times to repeat the splitOn field</param>
    /// <returns>The SqlMapperWrapper instance</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn(string splitOn, int repeat = 1)
    {
        _splitOn = (_splitOn == null ? "" : _splitOn + ",") + string.Join(",", Enumerable.Repeat(splitOn, repeat));
        return this;
    }

    /// <summary>
    /// Executes the query and maps the results with a single child relationship
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1]));
    }

    /// <summary>
    /// Executes the query and maps the results with two child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2]));
    }

    /// <summary>   
    /// Executes the query and maps the results with three child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild, TThirdChild>(string sql,
        object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild, TThirdChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2], (TThirdChild)objects[3]));
    }

    /// <summary>
    /// Executes the query and maps the results with four child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild, TThirdChild, TFourthChild>(string sql,
        object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild, TThirdChild, TFourthChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2], (TThirdChild)objects[3], (TFourthChild)objects[4]));
    }

    /// <summary>
    /// Executes the query and maps the results with five child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild, TThirdChild, TFourthChild,
        TFifthChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild, TThirdChild, TFourthChild, TFifthChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2], (TThirdChild)objects[3], (TFourthChild)objects[4], (TFifthChild)objects[5]));
    }

    /// <summary>
    /// Executes the query and maps the results with six child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild, TThirdChild, TFourthChild,
        TFifthChild, TSixthChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild, TThirdChild, TFourthChild, TFifthChild, TSixthChild>?
            callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2], (TThirdChild)objects[3], (TFourthChild)objects[4], (TFifthChild)objects[5],
                (TSixthChild)objects[6]));
    }

    /// <summary>
    /// Executes the query and maps the results with seven child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <typeparam name="TSeventhChild">Type of the seventh child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild, TThirdChild, TFourthChild,
        TFifthChild, TSixthChild, TSeventhChild, TEighthChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild, TThirdChild, TFourthChild, TFifthChild, TSixthChild, TSeventhChild,
            TEighthChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2], (TThirdChild)objects[3], (TFourthChild)objects[4], (TFifthChild)objects[5],
                (TSixthChild)objects[6], (TSeventhChild)objects[7], (TEighthChild)objects[8]));
    }


    /// <summary>
    /// Executes the query and maps the results with eight child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <typeparam name="TSeventhChild">Type of the seventh child entity</typeparam>
    /// <typeparam name="TEighthChild">Type of the eighth child entity</typeparam>
    /// <typeparam name="TNinthChild">Type of the ninth child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild, TThirdChild, TFourthChild,
        TFifthChild, TSixthChild, TSeventhChild, TEighthChild, TNinthChild>(string sql, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild, TThirdChild, TFourthChild, TFifthChild, TSixthChild, TSeventhChild,
            TEighthChild, TNinthChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2], (TThirdChild)objects[3], (TFourthChild)objects[4], (TFifthChild)objects[5],
                (TSixthChild)objects[6], (TSeventhChild)objects[7], (TEighthChild)objects[8], (TNinthChild)objects[9]));
    }

    /// <summary>
    /// Executes the query and maps the results with nine child relationships
    /// </summary>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <typeparam name="TSeventhChild">Type of the seventh child entity</typeparam>
    /// <typeparam name="TEighthChild">Type of the eighth child entity</typeparam>
    /// <typeparam name="TNinthChild">Type of the ninth child entity</typeparam>
    /// <typeparam name="TTenthChild">Type of the tenth child entity</typeparam>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="param">The parameters to pass to the query</param>
    /// <param name="transaction">The transaction to use (if any)</param>
    /// <param name="buffered">Whether to buffer the results</param>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="commandTimeout">The command timeout (in seconds)</param>
    /// <param name="commandType">The type of command to execute</param>
    /// <param name="callbackAfterMapRow">Callback to execute after mapping each row</param>
    /// <returns>Collection of mapped entities with child relationships</returns>
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirstChild, TSecondChild, TThirdChild, TFourthChild,
        TFifthChild, TSixthChild, TSeventhChild, TEighthChild, TNinthChild, TTenthChild>(string sql,
        object? param = null,
        IDbTransaction? transaction = null, bool buffered = true,
        string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        Action<TReturn, TFirstChild, TSecondChild, TThirdChild, TFourthChild, TFifthChild, TSixthChild, TSeventhChild,
            TEighthChild, TNinthChild, TTenthChild>? callbackAfterMapRow = null)
    {
        return await QueryAsync(sql, param, transaction, buffered, splitOn, commandTimeout, commandType,
            objects => callbackAfterMapRow?.Invoke((TReturn)objects[0], (TFirstChild)objects[1],
                (TSecondChild)objects[2], (TThirdChild)objects[3], (TFourthChild)objects[4], (TFifthChild)objects[5],
                (TSixthChild)objects[6], (TSeventhChild)objects[7], (TEighthChild)objects[8], (TNinthChild)objects[9],
                (TTenthChild)objects[10]));
    }
}