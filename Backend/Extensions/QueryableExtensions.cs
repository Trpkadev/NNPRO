using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Backend.Exceptions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Backend.Extensions;

public static class QueryableExtensions {
    private static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string? propertyName, bool? descending)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return source;

        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        if (property is null || !IsSortableProperty(property))
            throw new InvalidSortException(propertyName);

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var selector = Expression.Lambda(propertyAccess, parameter);

        string methodName = descending ?? false ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var resultExpression = Expression.Call(typeof(Queryable), methodName, [typeof(T), property.PropertyType], source.Expression, Expression.Quote(selector));

        return source.Provider.CreateQuery<T>(resultExpression);
    }

    private static bool IsSortableProperty(PropertyInfo property)
    {
        if (property.GetMethod is null || property.GetIndexParameters().Length > 0)
            return false;

        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        return propertyType.IsPrimitive
               || propertyType.IsEnum
               || propertyType == typeof(string)
               || propertyType == typeof(decimal)
               || propertyType == typeof(DateTime)
               || propertyType == typeof(DateTimeOffset)
               || propertyType == typeof(DateOnly)
               || propertyType == typeof(TimeOnly)
               || propertyType == typeof(TimeSpan)
               || propertyType == typeof(Guid);
    }

    /// <summary>
    ///     Applies dynamic sorting by property name.
    /// </summary>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, SortingMetadata? sortingMetadata) => sortingMetadata is null ? source : source.OrderByProperty(sortingMetadata.SortBy, sortingMetadata.IsDescending);

    /// <summary>
    ///     Applies pagination, or requires pagination when the result exceeds the configured limit.
    /// </summary>
    public async static Task<(T[] Items, PaginationMetadata Metadata)> ToPagedResultAsync<T>(this IQueryable<T> query, RequestQuery request, int maxItemsWithoutPagination, CancellationToken ct = default)
    {
        int totalItems = await query.CountAsync(ct);

        if (request is { Page: {} page, PageSize: {} pageSize })
        {
            int totalPages = totalItems > 0 ? 1 + (totalItems - 1) / pageSize : 0;
            if (page > totalPages)
                return ([], new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = totalItems });

            int skip = (int)((long)(page - 1) * pageSize);
            var pagedItems = await query.Skip(skip).Take(pageSize).ToArrayAsync(ct);
            return (pagedItems, new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = totalItems });
        }

        if (totalItems > maxItemsWithoutPagination)
            throw new PaginationNeededException();

        var allItems = await query.ToArrayAsync(ct);
        return (allItems, new PaginationMetadata { Page = 1, PageSize = totalItems, TotalItems = totalItems });
    }

    /// <summary>
    ///     Dynamically applies filters.
    /// </summary>
    public static IQueryable<T> Filter<T>(this IQueryable<T> source, Dictionary<string, string>? filters)
    {
        if (filters is null || filters.Count == 0)
            return source;

        var entityType = typeof(T);
        var parameter = Expression.Parameter(entityType, "x");
        Expression? combinedExpression = null;

        foreach ((string key, string rawValue) in filters)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                continue;

            var property = entityType.GetProperty(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property is null)
                throw new InvalidFilterException(key);

            var propertyAccess = Expression.Property(parameter, property);
            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            object? convertedValue;
            try
            {
                var converter = TypeDescriptor.GetConverter(targetType);
                convertedValue = converter.ConvertFromInvariantString(rawValue);
            }
            catch (Exception exception) when (exception is FormatException or NotSupportedException or ArgumentException or OverflowException)
            {
                throw new InvalidFilterException(key);
            }

            if (convertedValue is null && targetType.IsValueType)
                throw new InvalidFilterException(key);

            var constant = Expression.Constant(convertedValue, targetType);
            Expression filterCondition;

            if (targetType == typeof(string))
            {
                var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
                filterCondition = Expression.Call(propertyAccess, containsMethod, constant);
            }
            else
            {
                Expression adjustedPropertyAccess = property.PropertyType != targetType ? Expression.Convert(propertyAccess, targetType) : propertyAccess;
                filterCondition = Expression.Equal(adjustedPropertyAccess, constant);
            }

            combinedExpression = combinedExpression is null ? filterCondition : Expression.AndAlso(combinedExpression, filterCondition);
        }

        if (combinedExpression is null)
            return source;

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return source.Where(lambda);
    }
}