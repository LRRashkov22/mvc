using System.Net.Http.Headers;
using System.Net.Http.Json;
using WarehouseClient.Models;

namespace WarehouseClient.Services;

/// <summary>Encapsulates HTTP/JSON communication with the Warehouse REST API.</summary>
public sealed class WarehouseApiClient(HttpClient httpClient)
{
    public async Task<string?> RegisterAsync(Credentials credentials)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/register", credentials);
        return response.IsSuccessStatusCode ? null : await ReadErrorAsync(response);
    }

    public async Task<string?> LoginAsync(Credentials credentials)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/login", credentials);
        if (!response.IsSuccessStatusCode) return await ReadErrorAsync(response);

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokens is null || string.IsNullOrWhiteSpace(tokens.AccessToken))
            return "The API returned an invalid token response.";

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return null;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync()
    {
        var products = await httpClient.GetFromJsonAsync<List<ProductDto>>("api/products");
        return products ?? [];
    }

    public async Task<string?> CreateProductAsync(ProductDto product)
    {
        var response = await httpClient.PostAsJsonAsync("api/products", product);
        return response.IsSuccessStatusCode ? null : await ReadErrorAsync(response);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {body}";
    }
}
