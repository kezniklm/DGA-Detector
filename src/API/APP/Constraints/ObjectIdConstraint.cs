using MongoDB.Bson;

namespace APP.Constraints;

public class ObjectIdConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out object? value) && value != null)
        {
            return ObjectId.TryParse(value.ToString(), out _);
        }

        return false;
    }
}
