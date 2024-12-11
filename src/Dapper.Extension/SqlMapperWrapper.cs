// Copyright © 2024 Kvr.DapperRelMapper. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for full license information.

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
        var lookup = new Dictionary<object, TReturn>();
        // var memberExpressions = _expressions.Select(e => e.GetMemberExpression()).ToArray();
        // var types = new[] { typeof(TReturn) }.Concat(memberExpressions.Select(e => e.GetMapType())).ToArray();

        var splitOnModel = MapperHelper.GetSplitOnModel(_keySelector, _expressions, lookup, callbackAfterMapRow);
        await _connection.QueryAsync(sql, splitOnModel.Types, splitOnModel.Func, param, transaction, buffered, _splitOn ?? splitOn, commandTimeout, commandType);
        foreach (var item in lookup.Values)
        {
            _postProcess?.Invoke(item);
        }
        return lookup.Values;
    }
    
    /// <summary>
    /// Sets the splitOn parameter for the query using multiple expressions
    /// </summary>
    /// <param name="expressions">Array of expressions defining the splitOn fields</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    /// <example>
    /// <code>
    /// wrapper.SplitOn(x => x.Id, x => x.ParentId);
    /// </code>
    /// </example>
    public SqlMapperWrapper<TReturn, TKey> SplitOn(params LambdaExpression[] expressions)
    {
        _splitOn = (_splitOn == null ? "" : _splitOn + ",") + string.Join(",", expressions.Select(e => e.GetMemberExpression().Member.Name));
        return this;
    }

    /// <summary>
    /// Sets the splitOn parameter for the query with optional repetition
    /// </summary>
    /// <typeparam name="T">The type containing the property to split on</typeparam>
    /// <param name="expression">Expression defining the splitOn field</param>
    /// <param name="repeat">Number of times to repeat the splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T>(Expression<Func<T, object>> expression, int repeat = 1)
    {
        var memberName = expression.GetMemberExpression().Member.Name;
        var repeatedNames = string.Join(",", Enumerable.Repeat(memberName, repeat));
        _splitOn = (_splitOn == null ? "" : _splitOn + ",") + repeatedNames;
        return this;
    }
    
    /// <summary>
    /// Sets the splitOn parameter for the query using a string value with optional repetition
    /// </summary>
    /// <param name="splitOn">The field name to split the results on</param>
    /// <param name="repeat">Number of times to repeat the splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    /// <example>
    /// <code>
    /// // Results in "Id,Id,Id"
    /// wrapper.SplitOn("Id", 3);
    /// </code>
    /// </example>
    public SqlMapperWrapper<TReturn, TKey> SplitOn(string splitOn, int repeat = 1)
    {
        _splitOn = (_splitOn == null ? "" : _splitOn + ",") + string.Join(",", Enumerable.Repeat(splitOn, repeat));
        return this;
    }

    /// <summary>
    /// Sets the splitOn parameter for the query using two expressions
    /// </summary>
    /// <typeparam name="T">The type containing the first property to split on</typeparam>
    /// <typeparam name="T1">The type containing the second property to split on</typeparam>
    /// <param name="expression">Expression defining the first splitOn field</param>
    /// <param name="expression1">Expression defining the second splitOn field</param>
    /// <param name="repeat">Number of times to repeat the first splitOn field in the resulting string</param>
    /// <param name="repeat1">Number of times to repeat the second splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, T1>(Expression<Func<T, object>> expression, Expression<Func<T1, object>> expression1, int repeat = 1, int repeat1 = 1)
    {
        return SplitOn(expression, repeat).SplitOn(expression1, repeat1);
    }

    /// <summary>
    /// Sets the splitOn parameter for the query using three expressions
    /// </summary>
    /// <typeparam name="T">The type containing the first property to split on</typeparam>
    /// <typeparam name="T1">The type containing the second property to split on</typeparam>
    /// <typeparam name="T2">The type containing the third property to split on</typeparam>
    /// <param name="expression">Expression defining the first splitOn field</param>
    /// <param name="expression1">Expression defining the second splitOn field</param>
    /// <param name="expression2">Expression defining the third splitOn field</param>
    /// <param name="repeat">Number of times to repeat the first splitOn field in the resulting string</param>
    /// <param name="repeat1">Number of times to repeat the second splitOn field in the resulting string</param>
    /// <param name="repeat2">Number of times to repeat the third splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, T1, T2>(Expression<Func<T, object>> expression, Expression<Func<T1, object>> expression1, Expression<Func<T2, object>> expression2, int repeat = 1, int repeat1 = 1, int repeat2 = 1)
    {
        return SplitOn(expression, repeat).SplitOn(expression1, repeat1).SplitOn(expression2, repeat2);
    }

    /// <summary>
    /// Sets the splitOn parameter for the query using four expressions
    /// </summary>
    /// <typeparam name="T">The type containing the first property to split on</typeparam>
    /// <typeparam name="T1">The type containing the second property to split on</typeparam>
    /// <typeparam name="T2">The type containing the third property to split on</typeparam>
    /// <typeparam name="T3">The type containing the fourth property to split on</typeparam>
    /// <param name="expression">Expression defining the first splitOn field</param>
    /// <param name="expression1">Expression defining the second splitOn field</param>
    /// <param name="expression2">Expression defining the third splitOn field</param>
    /// <param name="expression3">Expression defining the fourth splitOn field</param>
    /// <param name="repeat">Number of times to repeat the first splitOn field in the resulting string</param>
    /// <param name="repeat1">Number of times to repeat the second splitOn field in the resulting string</param>
    /// <param name="repeat2">Number of times to repeat the third splitOn field in the resulting string</param>
    /// <param name="repeat3">Number of times to repeat the fourth splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, T1, T2, T3>(Expression<Func<T, object>> expression, Expression<Func<T1, object>> expression1, Expression<Func<T2, object>> expression2, Expression<Func<T3, object>> expression3, int repeat = 1, int repeat1 = 1, int repeat2 = 1, int repeat3 = 1)
    {
        return SplitOn(expression, repeat).SplitOn(expression1, repeat1).SplitOn(expression2, repeat2).SplitOn(expression3, repeat3);
    }

    /// <summary>
    /// Sets the splitOn parameter for the query using five expressions
    /// </summary>
    /// <typeparam name="T">The type containing the first property to split on</typeparam>
    /// <typeparam name="T1">The type containing the second property to split on</typeparam>
    /// <typeparam name="T2">The type containing the third property to split on</typeparam>
    /// <typeparam name="T3">The type containing the fourth property to split on</typeparam>
    /// <typeparam name="T4">The type containing the fifth property to split on</typeparam>
    /// <param name="expression">Expression defining the first splitOn field</param>
    /// <param name="expression1">Expression defining the second splitOn field</param>
    /// <param name="expression2">Expression defining the third splitOn field</param>
    /// <param name="expression3">Expression defining the fourth splitOn field</param>
    /// <param name="expression4">Expression defining the fifth splitOn field</param>
    /// <param name="repeat">Number of times to repeat the first splitOn field in the resulting string</param>
    /// <param name="repeat1">Number of times to repeat the second splitOn field in the resulting string</param>
    /// <param name="repeat2">Number of times to repeat the third splitOn field in the resulting string</param>
    /// <param name="repeat3">Number of times to repeat the fourth splitOn field in the resulting string</param>
    /// <param name="repeat4">Number of times to repeat the fifth splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, T1, T2, T3, T4>(Expression<Func<T, object>> expression, Expression<Func<T1, object>> expression1, Expression<Func<T2, object>> expression2, Expression<Func<T3, object>> expression3, Expression<Func<T4, object>> expression4, int repeat = 1, int repeat1 = 1, int repeat2 = 1, int repeat3 = 1, int repeat4 = 1)
    {
        return SplitOn(expression, repeat).SplitOn(expression1, repeat1).SplitOn(expression2, repeat2).SplitOn(expression3, repeat3).SplitOn(expression4, repeat4);
    }

    /// <summary>
    /// Sets the splitOn parameter for the query using six expressions
    /// </summary>
    /// <typeparam name="T">The type containing the first property to split on</typeparam>
    /// <typeparam name="T1">The type containing the second property to split on</typeparam>
    /// <typeparam name="T2">The type containing the third property to split on</typeparam>
    /// <typeparam name="T3">The type containing the fourth property to split on</typeparam>
    /// <typeparam name="T4">The type containing the fifth property to split on</typeparam>
    /// <typeparam name="T5">The type containing the sixth property to split on</typeparam>
    /// <param name="expression">Expression defining the first splitOn field</param>
    /// <param name="expression1">Expression defining the second splitOn field</param>
    /// <param name="expression2">Expression defining the third splitOn field</param>
    /// <param name="expression3">Expression defining the fourth splitOn field</param>
    /// <param name="expression4">Expression defining the fifth splitOn field</param>
    /// <param name="expression5">Expression defining the sixth splitOn field</param>
    /// <param name="repeat">Number of times to repeat the first splitOn field in the resulting string</param>
    /// <param name="repeat1">Number of times to repeat the second splitOn field in the resulting string</param>
    /// <param name="repeat2">Number of times to repeat the third splitOn field in the resulting string</param>
    /// <param name="repeat3">Number of times to repeat the fourth splitOn field in the resulting string</param>
    /// <param name="repeat4">Number of times to repeat the fifth splitOn field in the resulting string</param>
    /// <param name="repeat5">Number of times to repeat the sixth splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, T1, T2, T3, T4, T5>(Expression<Func<T, object>> expression, Expression<Func<T1, object>> expression1, Expression<Func<T2, object>> expression2, Expression<Func<T3, object>> expression3, Expression<Func<T4, object>> expression4, Expression<Func<T5, object>> expression5, int repeat = 1, int repeat1 = 1, int repeat2 = 1, int repeat3 = 1, int repeat4 = 1, int repeat5 = 1)
    {
        return SplitOn(expression, repeat).SplitOn(expression1, repeat1).SplitOn(expression2, repeat2).SplitOn(expression3, repeat3).SplitOn(expression4, repeat4).SplitOn(expression5, repeat5);
    }

    /// <summary>
    /// Sets the splitOn parameter for the query using seven expressions
    /// </summary>
    /// <typeparam name="T">The type containing the first property to split on</typeparam>
    /// <typeparam name="T1">The type containing the second property to split on</typeparam>
    /// <typeparam name="T2">The type containing the third property to split on</typeparam>
    /// <typeparam name="T3">The type containing the fourth property to split on</typeparam>
    /// <typeparam name="T4">The type containing the fifth property to split on</typeparam>
    /// <typeparam name="T5">The type containing the sixth property to split on</typeparam>
    /// <typeparam name="T6">The type containing the seventh property to split on</typeparam>
    /// <param name="expression">Expression defining the first splitOn field</param>
    /// <param name="expression1">Expression defining the second splitOn field</param>
    /// <param name="expression2">Expression defining the third splitOn field</param>
    /// <param name="expression3">Expression defining the fourth splitOn field</param>
    /// <param name="expression4">Expression defining the fifth splitOn field</param>
    /// <param name="expression5">Expression defining the sixth splitOn field</param>
    /// <param name="repeat">Number of times to repeat the first splitOn field in the resulting string</param>
    /// <param name="repeat1">Number of times to repeat the second splitOn field in the resulting string</param>
    /// <param name="repeat2">Number of times to repeat the third splitOn field in the resulting string</param>
    /// <param name="repeat3">Number of times to repeat the fourth splitOn field in the resulting string</param>
    /// <param name="repeat4">Number of times to repeat the fifth splitOn field in the resulting string</param>
    /// <param name="repeat5">Number of times to repeat the sixth splitOn field in the resulting string</param>
    /// <param name="repeat6">Number of times to repeat the seventh splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, T1, T2, T3, T4, T5, T6>(Expression<Func<T, object>> expression, Expression<Func<T1, object>> expression1, Expression<Func<T2, object>> expression2, Expression<Func<T3, object>> expression3, Expression<Func<T4, object>> expression4, Expression<Func<T5, object>> expression5, Expression<Func<T6, object>> expression6, int repeat = 1, int repeat1 = 1, int repeat2 = 1, int repeat3 = 1, int repeat4 = 1, int repeat5 = 1, int repeat6 = 1)
    {
        return SplitOn(expression, repeat).SplitOn(expression1, repeat1).SplitOn(expression2, repeat2).SplitOn(expression3, repeat3).SplitOn(expression4, repeat4).SplitOn(expression5, repeat5).SplitOn(expression6, repeat6);
    }

    /// <summary>
    /// Sets the splitOn parameter for the query using eight expressions
    /// </summary>
    /// <typeparam name="T">The type containing the first property to split on</typeparam>
    /// <typeparam name="T1">The type containing the second property to split on</typeparam>
    /// <typeparam name="T2">The type containing the third property to split on</typeparam>
    /// <typeparam name="T3">The type containing the fourth property to split on</typeparam>
    /// <typeparam name="T4">The type containing the fifth property to split on</typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6">The type containing the sixth property to split on</typeparam>
    /// <typeparam name="T7">The type containing the seventh property to split on</typeparam>
    /// <param name="expression">Expression defining the first splitOn field</param>
    /// <param name="expression1">Expression defining the second splitOn field</param>
    /// <param name="expression2">Expression defining the third splitOn field</param>
    /// <param name="expression3">Expression defining the fourth splitOn field</param>
    /// <param name="expression4">Expression defining the fifth splitOn field</param>
    /// <param name="expression5">Expression defining the sixth splitOn field</param>
    /// <param name="expression6">Expression defining the seventh splitOn field</param>
    /// <param name="expression7">Expression defining the eighth splitOn field</param>
    /// <param name="repeat">Number of times to repeat the first splitOn field in the resulting string</param>
    /// <param name="repeat1">Number of times to repeat the second splitOn field in the resulting string</param>
    /// <param name="repeat2">Number of times to repeat the third splitOn field in the resulting string</param>
    /// <param name="repeat3">Number of times to repeat the fourth splitOn field in the resulting string</param>
    /// <param name="repeat4">Number of times to repeat the fifth splitOn field in the resulting string</param>
    /// <param name="repeat5">Number of times to repeat the sixth splitOn field in the resulting string</param>
    /// <param name="repeat6">Number of times to repeat the seventh splitOn field in the resulting string</param>
    /// <param name="repeat7">Number of times to repeat the eighth splitOn field in the resulting string</param>
    /// <returns>The SqlMapperWrapper instance for method chaining</returns>
    public SqlMapperWrapper<TReturn, TKey> SplitOn<T, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T, object>> expression, Expression<Func<T1, object>> expression1, Expression<Func<T2, object>> expression2, Expression<Func<T3, object>> expression3, Expression<Func<T4, object>> expression4, Expression<Func<T5, object>> expression5, Expression<Func<T6, object>> expression6, Expression<Func<T7, object>> expression7, int repeat = 1, int repeat1 = 1, int repeat2 = 1, int repeat3 = 1, int repeat4 = 1, int repeat5 = 1, int repeat6 = 1, int repeat7 = 1)
    {
        return SplitOn(expression, repeat).SplitOn(expression1, repeat1).SplitOn(expression2, repeat2).SplitOn(expression3, repeat3).SplitOn(expression4, repeat4).SplitOn(expression5, repeat5).SplitOn(expression6, repeat6).SplitOn(expression7, repeat7);
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
    /// <typeparam name="TEighthChild">Type of the eighth child entity</typeparam>
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