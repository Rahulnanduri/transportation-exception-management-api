using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransportationExceptionManagement.Tests.Support;

internal static class TestHttp
{
    public static JsonSerializerOptions JsonOptions { get; } = CreateJsonOptions();

    public static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return Assert.IsType<T>(result);
    }

    public static async Task<string?> ReadProblemCodeAsync(this HttpResponseMessage response)
    {
        await using var content = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(content);
        return document.RootElement.TryGetProperty("code", out var code)
            ? code.GetString()
            : null;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        return options;
    }
}
