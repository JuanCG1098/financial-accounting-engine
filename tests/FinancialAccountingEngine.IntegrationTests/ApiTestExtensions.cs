using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinancialAccountingEngine.IntegrationTests;

/// <summary>
/// Shared JSON options that mirror the API configuration (enums as SCREAMING_SNAKE_CASE), plus
/// small helpers for typed requests/responses.
/// </summary>
internal static class ApiTestExtensions
{
    public static readonly JsonSerializerOptions JsonOptions = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
        return options;
    }

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body)
        => client.PostAsJsonAsync(url, body, JsonOptions);

    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
}
