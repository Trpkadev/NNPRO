using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Backend.Exceptions;
using Backend.Models.DTOs.Request;
using Backend.Models.DTOs.Response;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Extensions;

public static class QueryableExtensions {
    /// <summary>
    ///     Applies dynamic sorting by property name.
    /// </summary>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string? propertyName, bool? descending)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return source;

        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        if (property is null)
            throw new ArgumentException(string.Format(Resources.ErrorInvalidPropertyForType, propertyName, typeof(T).Name), nameof(propertyName));

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var selector = Expression.Lambda(propertyAccess, parameter);

        string methodName = descending ?? false ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var resultExpression = Expression.Call(typeof(Queryable), methodName, [typeof(T), property.PropertyType], source.Expression, Expression.Quote(selector));

        return source.Provider.CreateQuery<T>(resultExpression);
    }

    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, SortingMetadata? sortingMetadata) => sortingMetadata is null ? source : source.OrderByProperty(sortingMetadata.SortBy, sortingMetadata.IsDescending);

    /// <summary>
    ///     Universal method for pagination of any IQueryable type.
    ///     If pagination params are unset, checks if DB would return more rows than specified in config -> throws
    ///     PaginationNeededException.
    ///     Applies Skip & Take and returns the according data page.
    /// </summary>
    public async static Task<(T[] Items, PaginationMetadata Metadata)> ToPagedResultAsync<T>(this IQueryable<T> query, RequestQuery request, int maxItemsWithoutPagination, CancellationToken ct = default)
    {
        int totalItems = await query.CountAsync(ct);

        // Paging is ON
        if (request is { Page: {} page, PageSize: {} pageSize })
        {
            var pagedItems = await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct);
            return (pagedItems, new PaginationMetadata { Page = page, PageSize = pageSize, TotalItems = totalItems });
        }

        // Paging is OFF
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

            try
            {
                var converter = TypeDescriptor.GetConverter(targetType);
                object? convertedValue = converter.ConvertFromInvariantString(rawValue);
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
            catch
            {
                // If conversion fails we ignore the filter (fe. client sent "abc" in uint ID)
            }
        }

        if (combinedExpression is null)
            return source;

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return source.Where(lambda);
    }

    /// <summary>
    ///     Applies fulltext search on courses.
    /// </summary>
    public static IQueryable<Course> ApplyCourseSearch(this IQueryable<Course> query, string? search) => string.IsNullOrWhiteSpace(search) ? query : query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
}