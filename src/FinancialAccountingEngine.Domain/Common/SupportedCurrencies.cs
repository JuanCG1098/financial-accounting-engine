namespace FinancialAccountingEngine.Domain.Common;

/// <summary>
/// The set of ISO-4217 currency codes the engine is configured to accept.
/// Kept intentionally small for the portfolio scenario.
/// </summary>
public static class SupportedCurrencies
{
    public const string Ars = "ARS";
    public const string Usd = "USD";
    public const string Eur = "EUR";

    private static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Ars, Usd, Eur
    };

    public static IReadOnlyCollection<string> Codes => All;

    public static bool IsSupported(string? currency) =>
        !string.IsNullOrWhiteSpace(currency) && All.Contains(currency);
}
