namespace FinancialAccountingEngine.Domain.Common;

/// <summary>
/// Represents a domain or application error with a stable machine-readable code
/// and a human-readable message. Used together with <see cref="Result"/> to avoid
/// throwing exceptions for expected business failures.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>A requested resource could not be found.</summary>
    public static Error NotFound(string message) => new("not_found", message);

    /// <summary>The request was structurally valid but violated a business rule.</summary>
    public static Error Validation(string message) => new("validation", message);

    /// <summary>The operation cannot be performed given the current entity state.</summary>
    public static Error Conflict(string message) => new("conflict", message);
}
