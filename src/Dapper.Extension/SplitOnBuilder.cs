using System.Linq.Expressions;
using System.Text;

namespace Kvr.Dapper;

public class SplitOnBuilder
{
    private readonly StringBuilder splitOnBuilder = new();
    
    private SplitOnBuilder()
    {
    }
    
    /// <summary>
    /// Creates a new SplitOnBuilder instance
    /// </summary>
    /// <returns>The SplitOnBuilder instance</returns>
    public static SplitOnBuilder Create()
    {
        return new SplitOnBuilder();
    }

    /// <summary>
    /// Sets the splitOn parameter for the query
    /// </summary>
    /// <param name="expressions">Expressions defining the splitOn fields</param>
    /// <returns>The SplitOnBuilder instance</returns>
    public SplitOnBuilder SplitOn(params LambdaExpression[] expressions)
    {
        if (splitOnBuilder.Length > 0)
        {
            splitOnBuilder.Append(",");
        }
        splitOnBuilder.Append(string.Join(",", expressions.Select(e => e.GetMemberExpression().Member.Name)));
        return this;
    }

    /// <summary>
    /// Sets the splitOn parameter for the query
    /// </summary>
    /// <typeparam name="T">The type of the expression</typeparam>
    /// <param name="expression">The expression defining the splitOn field</param>
    /// <param name="repeat">The number of times to repeat the splitOn field</param>
    /// <returns>The SplitOnBuilder instance</returns>
    public SplitOnBuilder SplitOn<T>(Expression<Func<T, object>> expression, int repeat = 1)
    {
        var memberName = expression.GetMemberExpression().Member.Name;
        var repeatedNames = string.Join(",", Enumerable.Repeat(memberName, repeat));
        if (splitOnBuilder.Length > 0)
        {
            splitOnBuilder.Append(",");
        }
        splitOnBuilder.Append(repeatedNames);
        return this;
    }
    
    /// <summary>
    /// Sets the splitOn parameter for the query
    /// </summary>
    /// <param name="splitOn">The field to split the results on</param>
    /// <param name="repeat">The number of times to repeat the splitOn field</param>
    /// <returns>The SplitOnBuilder instance</returns>
    public SplitOnBuilder SplitOn(string splitOn, int repeat = 1)
    {
        if (splitOnBuilder.Length > 0)
        {
            splitOnBuilder.Append(",");
        }
        splitOnBuilder.Append(string.Join(",", Enumerable.Repeat(splitOn, repeat)));
        return this;
    }

    /// <summary>
    /// Builds the splitOn parameter for the query
    /// </summary>
    /// <returns>The splitOn parameter</returns>
    public string Build()
    {
        return splitOnBuilder.ToString();
    }
}
