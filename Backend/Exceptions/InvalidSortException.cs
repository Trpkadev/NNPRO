namespace Backend.Exceptions;

public class InvalidSortException(string sortBy) : Exception {
    public string SortBy { get; } = sortBy;
}
