// Copyright © 2024 Kvr.DapperRelMapper. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Kvr.Dapper;

/// <summary>
/// Provides utility methods for expression handling and property access
/// </summary>
public static class Utils
{
    /// <summary>
    /// Extracts the MemberExpression from a LambdaExpression
    /// </summary>
    /// <param name="expression">The lambda expression to process</param>
    /// <returns>The extracted MemberExpression</returns>
    /// <exception cref="ArgumentException">Thrown when expression is not a valid member expression</exception>
    public static MemberExpression GetMemberExpression(this LambdaExpression expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null && expression.Body is UnaryExpression unaryExpression)
        {
            memberExpression = unaryExpression.Operand as MemberExpression;
        }

        if (memberExpression == null)
            throw new ArgumentException("Expression must be a member expression");
        
        return memberExpression;
    }

    /// <summary>
    /// Gets the mapping type for a member expression, handling collection types
    /// </summary>
    /// <param name="expression">The member expression to process</param>
    /// <returns>The type to map to</returns>
    public static Type GetMapType(this MemberExpression expression)
    {
        var returnType = expression.Type;
        if (IsCollectionType(returnType))
        {
            return returnType.GetGenericArguments().First();
        }

        return returnType;
    }

    /// <summary>
    /// Determines if a type implements ICollection or ICollection<T>
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is a collection type</returns>
    public static bool IsCollectionType(Type type)
    {
        return typeof(ICollection).IsAssignableFrom(type) || 
               typeof(ICollection<>).IsAssignableFrom(type) ||
               type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
               type.GetInterfaces().Any(i => i.IsGenericType && 
                                            i.GetGenericTypeDefinition() == typeof(ICollection<>));
    }

    /// <summary>
    /// Gets a property value using an expression
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <typeparam name="TValue">The property type</typeparam>
    /// <param name="target">The source object</param>
    /// <param name="propertyExpression">Expression pointing to the property</param>
    /// <returns>The property value</returns>
    public static TValue GetPropertyValue<T, TValue>(T target, 
        Expression<Func<T, TValue>> propertyExpression)
    {
        return GetPropertyValue<T, TValue>(target, propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Sets a property value using a MemberExpression
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <typeparam name="TValue">The property type</typeparam>
    /// <param name="target">The source object</param>
    /// <param name="memberExpression">The MemberExpression pointing to the property</param>
    /// <param name="value">The value to set</param>
    public static void SetPropertyValue<T, TValue>(T target, MemberExpression memberExpression, TValue value)
    {
        var property = memberExpression.Member as PropertyInfo;

        if (property != null)
        {
            property.SetValue(target, value);
        }
    }

    /// <summary>
    /// Gets a property value using a MemberExpression
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <typeparam name="TValue">The property type</typeparam>
    /// <param name="target">The source object</param>
    /// <param name="memberExpression">The MemberExpression pointing to the property</param>
    /// <returns>The property value</returns>
    public static TValue GetPropertyValue<T, TValue>(T target, MemberExpression memberExpression)
    {
        var property = memberExpression?.Member as PropertyInfo;
        
        if (property != null)
        {
            return (TValue)property.GetValue(target);
        }

        return default;
    }
}