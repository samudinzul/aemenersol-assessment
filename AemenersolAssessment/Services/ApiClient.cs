using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AemenersolAssessment.Models;

namespace AemenersolAssessment.Services;

public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private string? _bearerToken;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync(
            "/api/Account/Login",
            new LoginRequest { Username = username, Password = password });

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadAsStringAsync();
        _bearerToken = token.Trim('"');
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _bearerToken);
    }

    public async Task<List<PlatformDto>> GetPlatformWellAsync(string endpoint)
    {
        var result = await _http.GetFromJsonAsync<List<PlatformDto>>(
            endpoint, JsonOptions);
        return result ?? new List<PlatformDto>();
    }
}
