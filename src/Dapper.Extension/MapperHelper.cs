using System.Collections;
using System.Linq.Expressions;

namespace Kvr.Dapper;

/// <summary>
/// This class is used to map the result of a query to a model.
/// </summary>
/// <typeparam name="T">The type of the model to map to.</typeparam>
class SplitOnModel<T>
{
    /// <summary>   
    /// The types of the model to map to.
    /// </summary>
    public Type[] Types { get; set; }
    /// <summary>
    /// The function to map the result to the model.
    /// </summary>
    public Func<object[], T> Func { get; set; }
}

/// <summary>
/// This class is used to map the result of a query to a model.
/// </summary>
static class MapperHelper
{
    /// <summary>
    /// Get the split on model.
    /// </summary>
    /// <param name="keySelection">The key selection.</param>
    /// <param name="expressions">The expressions.</param>
    /// <param name="lookup">The lookup.</param>
    /// <param name="callbackAfterMapRow">The callback after map row.</param>
    /// <returns>The split on model.</returns>
    public static SplitOnModel<T> GetSplitOnModel<T>(LambdaExpression keySelection, LambdaExpression[] expressions, 
        Dictionary<object, T> lookup, Action<object[]>? callbackAfterMapRow = null)
    {
        var memberExpressions = expressions.Select(e => e.GetMemberExpression()).ToArray();
        var types = new[] { typeof(T) }.Concat(memberExpressions.Select(e => e.GetMapType())).ToArray();
        var memberTypeParentIndex = new int[memberExpressions.Length];
        for (var i = 0; i < memberExpressions.Length; i++)
        {
            var memberExpression = memberExpressions[i];
            if (memberExpression.Member.DeclaringType == typeof(T)) {
                memberTypeParentIndex[i] = 0;
            } 
            else 
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    if (memberExpressions[i].Member.DeclaringType == types[j + 1])
                    {
                        memberTypeParentIndex[i] = j + 1;
                        break;
                    }
                }
            }
        }
        return new SplitOnModel<T>()
        {
            Types = types,
            Func = objects =>
            {
                var result = (T)objects[0];
                var key = Utils.GetPropertyValue<object, object>(result, keySelection.GetMemberExpression());
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
                    if (memberExpression.Member.DeclaringType == typeof(T)) 
                    {
                        if (Utils.IsCollectionType(memberExpression.Type))
                        {
                            if (newRecord)
                            {
                                Utils.SetPropertyValue(result, memberExpression,
                                    Activator.CreateInstance(typeof(List<>).MakeGenericType(types[i])));
                            }

                            if (objects[i] != null)
                            {
                                Utils.GetPropertyValue<object, IList>(result, memberExpression).Add(objects[i]);
                            }
                        }
                        else
                        {
                            Utils.SetPropertyValue(result, memberExpression, objects[i]);
                        }
                    }
                    else 
                    {
                        var parentResult = objects[memberTypeParentIndex[i - 1]];
                        // only deal with many to one
                        // skip many to many/one
                        if (!Utils.IsCollectionType(memberExpression.Type) && objects[i] != null && types[i] == objects[i].GetType())
                        {
                            Utils.SetPropertyValue(parentResult, memberExpression, objects[i]);
                        }
                    }
                }

                callbackAfterMapRow?.Invoke(objects);
                return result;
            }
        };
    }
}