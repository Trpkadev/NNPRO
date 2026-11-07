namespace Backend.Extensions;

public class UintRouteConstraint : IRouteConstraint {
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out object? value) && value is not null)
            return uint.TryParse(value.ToString(), out _);

        return false;
    }
}