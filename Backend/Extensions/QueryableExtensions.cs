using System.Linq.Expressions;
using System.Reflection;

namespace Backend.Extensions;

public static class QueryableExtensions {
    /// <summary>
    /// Aplikuje dynamické řazení podle názvu vlastnosti v textové podobě.
    /// </summary>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string? propertyName, bool descending = false)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return source;

        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        if (property is null)
            throw new ArgumentException($"Vlastnost '{propertyName}' neexistuje na typu '{typeof(T).Name}'.", nameof(propertyName));

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var selector = Expression.Lambda(propertyAccess, parameter);

        string methodName = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var resultExpression = Expression.Call(typeof(Queryable), methodName, [typeof(T), property.PropertyType], source.Expression, Expression.Quote(selector));

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}