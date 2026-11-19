namespace Backend.Exceptions;

public sealed class ServiceException(int statusCode, string message) : Exception(message) {
    public int StatusCode { get; } = statusCode;

    public static ServiceException BadRequest(string message) => new(StatusCodes.Status400BadRequest, message);

    public static ServiceException Forbidden(string message) => new(StatusCodes.Status403Forbidden, message);

    public static ServiceException NotFound(string resource) => new(StatusCodes.Status404NotFound, string.Format(Resources.ErrorResourceNotFound, resource));

    public static ServiceException Conflict(string message) => new(StatusCodes.Status409Conflict, message);
}