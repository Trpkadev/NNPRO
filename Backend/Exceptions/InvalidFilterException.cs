namespace Backend.Exceptions;

public class InvalidFilterException(string filterParam) : Exception {
    public string FilterParam { get; init; } = filterParam;
}