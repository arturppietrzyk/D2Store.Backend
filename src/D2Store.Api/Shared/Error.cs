namespace D2Store.Api.Shared;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error Forbidden = new("Error.Forbidden", "You do not have permission to access this resource.");
}

