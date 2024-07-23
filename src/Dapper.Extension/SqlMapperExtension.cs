using System.Data;
using System.Linq.Expressions;

namespace Kvr.Dapper;

/// <summary>
/// Provides extension methods for configuring Dapper SQL mapping
/// </summary>
public static class SqlMapperExtension
{
    /// <summary>
    /// Configures a mapper for complex object relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity to be returned</typeparam>
    /// <typeparam name="TKey">The type of the key used for entity lookup</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="expressions">Array of expressions defining relationships</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey>(
        this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        params LambdaExpression[] expressions) where TKey : notnull
    {
        return new SqlMapperWrapper<TReturn, TKey>(connection, keySelector, expressions);
    }

    /// <summary>
    /// Configures a mapper with strongly typed expressions for relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="expressions">Array of expressions defining relationships</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey>(
        this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        params Expression<Func<TReturn, object>>[] expressions) where TKey : notnull
    {
        return new SqlMapperWrapper<TReturn, TKey>(connection, keySelector, expressions);
    }

    /// <summary>
    /// Configures a mapper with a single child relationship
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild>(
        this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector, new LambdaExpression[] { firstChildSelector });
    }

    /// <summary>
    /// Configures a mapper with two child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild>(
        this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[] { firstChildSelector, secondChildSelector });
    }

    /// <summary>
    /// Configures a mapper with three child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild>(
        this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[] { firstChildSelector, secondChildSelector, thirdChildSelector });
    }

    /// <summary>
    /// Configures a mapper with four child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>  
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <param name="fourthChildSelector">Expression defining the fourth child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns> 
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild,
        TFourthChild>(this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector,
        Expression<Func<TReturn, TFourthChild>> fourthChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[]
                { firstChildSelector, secondChildSelector, thirdChildSelector, fourthChildSelector });
    }

    /// <summary>
    /// Configures a mapper with five child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <param name="fourthChildSelector">Expression defining the fourth child relationship</param>
    /// <param name="fifthChildSelector">Expression defining the fifth child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild,
        TFourthChild, TFifthChild>(this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector,
        Expression<Func<TReturn, TFourthChild>> fourthChildSelector,
        Expression<Func<TReturn, TFifthChild>> fifthChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[]
            {
                firstChildSelector, secondChildSelector, thirdChildSelector, fourthChildSelector, fifthChildSelector
            });
    }

    /// <summary>
    /// Configures a mapper with six child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <param name="fourthChildSelector">Expression defining the fourth child relationship</param>
    /// <param name="fifthChildSelector">Expression defining the fifth child relationship</param>
    /// <param name="sixthChildSelector">Expression defining the sixth child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild,
        TFourthChild, TFifthChild, TSixthChild>(this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector,
        Expression<Func<TReturn, TFourthChild>> fourthChildSelector,
        Expression<Func<TReturn, TFifthChild>> fifthChildSelector,
        Expression<Func<TReturn, TSixthChild>> sixthChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[]
            {
                firstChildSelector, secondChildSelector, thirdChildSelector, fourthChildSelector, fifthChildSelector,
                sixthChildSelector
            });
    }

    /// <summary>
    /// Configures a mapper with seven child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <typeparam name="TSeventhChild">Type of the seventh child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <param name="fourthChildSelector">Expression defining the fourth child relationship</param>
    /// <param name="fifthChildSelector">Expression defining the fifth child relationship</param>
    /// <param name="sixthChildSelector">Expression defining the sixth child relationship</param>
    /// <param name="seventhChildSelector">Expression defining the seventh child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild,
        TFourthChild, TFifthChild, TSixthChild, TSeventhChild>(this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector,
        Expression<Func<TReturn, TFourthChild>> fourthChildSelector,
        Expression<Func<TReturn, TFifthChild>> fifthChildSelector,
        Expression<Func<TReturn, TSixthChild>> sixthChildSelector,
        Expression<Func<TReturn, TSeventhChild>> seventhChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[]
            {
                firstChildSelector, secondChildSelector, thirdChildSelector, fourthChildSelector, fifthChildSelector,
                sixthChildSelector, seventhChildSelector
            });
    }

    /// <summary>
    /// Configures a mapper with eight child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <typeparam name="TSeventhChild">Type of the seventh child entity</typeparam>
    /// <typeparam name="TEighthChild">Type of the eighth child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <param name="fourthChildSelector">Expression defining the fourth child relationship</param>
    /// <param name="fifthChildSelector">Expression defining the fifth child relationship</param>
    /// <param name="sixthChildSelector">Expression defining the sixth child relationship</param>
    /// <param name="seventhChildSelector">Expression defining the seventh child relationship</param>
    /// <param name="eighthChildSelector"></param>
    /// <returns></returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild,
        TFourthChild, TFifthChild, TSixthChild, TSeventhChild, TEighthChild>(this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector,
        Expression<Func<TReturn, TFourthChild>> fourthChildSelector,
        Expression<Func<TReturn, TFifthChild>> fifthChildSelector,
        Expression<Func<TReturn, TSixthChild>> sixthChildSelector,
        Expression<Func<TReturn, TSeventhChild>> seventhChildSelector,
        Expression<Func<TReturn, TEighthChild>> eighthChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[]
            {
                firstChildSelector, secondChildSelector, thirdChildSelector, fourthChildSelector, fifthChildSelector,
                sixthChildSelector, seventhChildSelector, eighthChildSelector
            });
    }

    /// <summary>
    /// Configures a mapper with nine child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TFirstChild">Type of the first child entity</typeparam>
    /// <typeparam name="TSecondChild">Type of the second child entity</typeparam>
    /// <typeparam name="TThirdChild">Type of the third child entity</typeparam>
    /// <typeparam name="TFourthChild">Type of the fourth child entity</typeparam>
    /// <typeparam name="TFifthChild">Type of the fifth child entity</typeparam>
    /// <typeparam name="TSixthChild">Type of the sixth child entity</typeparam>
    /// <typeparam name="TSeventhChild">Type of the seventh child entity</typeparam>
    /// <typeparam name="TEighthChild">Type of the eighth child entity</typeparam>
    /// <typeparam name="TNinthChild">Type of the ninth child entity</typeparam>
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <param name="fourthChildSelector">Expression defining the fourth child relationship</param>
    /// <param name="fifthChildSelector">Expression defining the fifth child relationship</param>
    /// <param name="sixthChildSelector">Expression defining the sixth child relationship</param>
    /// <param name="seventhChildSelector">Expression defining the seventh child relationship</param>
    /// <param name="eighthChildSelector">Expression defining the eighth child relationship</param>
    /// <param name="ninthChildSelector">Expression defining the ninth child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild,
        TFourthChild, TFifthChild, TSixthChild, TSeventhChild, TEighthChild, TNinthChild>(this IDbConnection connection,
        Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector,
        Expression<Func<TReturn, TFourthChild>> fourthChildSelector,
        Expression<Func<TReturn, TFifthChild>> fifthChildSelector,
        Expression<Func<TReturn, TSixthChild>> sixthChildSelector,
        Expression<Func<TReturn, TSeventhChild>> seventhChildSelector,
        Expression<Func<TReturn, TEighthChild>> eighthChildSelector,
        Expression<Func<TReturn, TNinthChild>> ninthChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[]
            {
                firstChildSelector, secondChildSelector, thirdChildSelector, fourthChildSelector, fifthChildSelector,
                sixthChildSelector, seventhChildSelector, eighthChildSelector, ninthChildSelector
            });
    }

    /// <summary>
    /// Configures a mapper with ten child relationships
    /// </summary>
    /// <typeparam name="TReturn">The type of the main entity</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
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
    /// <param name="connection">Database connection</param>
    /// <param name="keySelector">Expression to select the key property</param>
    /// <param name="firstChildSelector">Expression defining the first child relationship</param>
    /// <param name="secondChildSelector">Expression defining the second child relationship</param>
    /// <param name="thirdChildSelector">Expression defining the third child relationship</param>
    /// <param name="fourthChildSelector">Expression defining the fourth child relationship</param>
    /// <param name="fifthChildSelector">Expression defining the fifth child relationship</param>
    /// <param name="sixthChildSelector">Expression defining the sixth child relationship</param>
    /// <param name="seventhChildSelector">Expression defining the seventh child relationship</param>
    /// <param name="eighthChildSelector">Expression defining the eighth child relationship</param>
    /// <param name="ninthChildSelector">Expression defining the ninth child relationship</param>
    /// <param name="tenthChildSelector">Expression defining the tenth child relationship</param>
    /// <returns>Configured SqlMapperWrapper instance</returns>
    public static SqlMapperWrapper<TReturn, TKey> ConfigMapper<TReturn, TKey, TFirstChild, TSecondChild, TThirdChild,
        TFourthChild, TFifthChild, TSixthChild, TSeventhChild, TEighthChild, TNinthChild, TTenthChild>(
        this IDbConnection connection, Expression<Func<TReturn, TKey>> keySelector,
        Expression<Func<TReturn, TFirstChild>> firstChildSelector,
        Expression<Func<TReturn, TSecondChild>> secondChildSelector,
        Expression<Func<TReturn, TThirdChild>> thirdChildSelector,
        Expression<Func<TReturn, TFourthChild>> fourthChildSelector,
        Expression<Func<TReturn, TFifthChild>> fifthChildSelector,
        Expression<Func<TReturn, TSixthChild>> sixthChildSelector,
        Expression<Func<TReturn, TSeventhChild>> seventhChildSelector,
        Expression<Func<TReturn, TEighthChild>> eighthChildSelector,
        Expression<Func<TReturn, TNinthChild>> ninthChildSelector,
        Expression<Func<TReturn, TTenthChild>> tenthChildSelector) where TKey : notnull
    {
        return ConfigMapper(connection, keySelector,
            new LambdaExpression[]
            {
                firstChildSelector, secondChildSelector, thirdChildSelector, fourthChildSelector, fifthChildSelector,
                sixthChildSelector, seventhChildSelector, eighthChildSelector, ninthChildSelector, tenthChildSelector
            });
    }
}