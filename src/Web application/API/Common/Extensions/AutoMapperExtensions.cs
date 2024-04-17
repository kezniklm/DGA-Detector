/**
 * @file AutoMapperExtensions.cs
 *
 * @brief Extends AutoMapper capabilities with custom extension methods.
 *
 * This file contains a set of extension methods for AutoMapper's IMappingExpression interface that enhance
 * the flexibility of mapping configurations between source and destination types.
 *
 * The main functionalities of this file include:
 * - Mapping individual members from source to destination objects using expressions.
 * - Setting fixed values on destination members.
 * - Ignoring certain destination members during the mapping process.
 * - Ignoring certain source members in validation during the mapping process.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Linq.Expressions;
using AutoMapper;

namespace Common.Extensions;

/// <summary>
///     Provides extension methods for AutoMapper configurations to simplify common mapping tasks.
/// </summary>
public static class AutoMapperExtensions
{
    /// <summary>
    ///     Configures a mapping between a source member and a destination member.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <typeparam name="TSourceMember">The type of the member in the source object.</typeparam>
    /// <param name="map">The mapping expression to extend.</param>
    /// <param name="dstSelector">Expression to select the destination member.</param>
    /// <param name="srcSelector">Expression to select the source member.</param>
    /// <returns>The original mapping expression for chaining.</returns>
    public static IMappingExpression<TSource, TDestination> MapMember<TSource, TDestination, TSourceMember>(
        this IMappingExpression<TSource, TDestination> map,
        Expression<Func<TDestination, object>> dstSelector,
        Expression<Func<TSource, TSourceMember>> srcSelector)
    {
        map.ForMember(dstSelector, config => config.MapFrom(srcSelector));
        return map;
    }

    /// <summary>
    ///     Configures a mapping to use a constant value for a destination member.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <typeparam name="TValue">The type of the value to map to the destination member.</typeparam>
    /// <param name="map">The mapping expression to extend.</param>
    /// <param name="dstSelector">Expression to select the destination member.</param>
    /// <param name="value">The constant value to assign to the destination member.</param>
    /// <returns>The original mapping expression for chaining.</returns>
    public static IMappingExpression<TSource, TDestination> UseValue<TSource, TDestination, TValue>(
        this IMappingExpression<TSource, TDestination> map,
        Expression<Func<TDestination, object>> dstSelector,
        TValue value)
    {
        map.ForMember(dstSelector, config => config.MapFrom(src => value));
        return map;
    }

    /// <summary>
    ///     Configures a mapping to ignore a specific destination member during mapping.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <param name="map">The mapping expression to extend.</param>
    /// <param name="selector">Expression to select the destination member to ignore.</param>
    /// <returns>The original mapping expression for chaining.</returns>
    public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map,
        Expression<Func<TDestination, object?>> selector)
    {
        if (selector is not null)
        {
            map.ForMember(selector, opt => opt.Ignore());
        }

        return map;
    }

    /// <summary>
    ///     Configures a mapping to ignore a specific source member for validation purposes.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <param name="map">The mapping expression to extend.</param>
    /// <param name="selector">Expression to select the source member to ignore for validation.</param>
    /// <returns>The original mapping expression for chaining.</returns>
    public static IMappingExpression<TSource, TDestination> IgnoreSource<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map,
        Expression<Func<TSource, object>> selector)
    {
        map.ForSourceMember(selector, opt => opt.DoNotValidate());
        return map;
    }
}
