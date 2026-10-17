using System.Text.Json.Serialization;

namespace PlatformWellSync.Models;

public class LoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class PlatformDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string? UniqueName { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("lastUpdate")]
    public DateTime? LastUpdate { get; set; }

    [JsonPropertyName("well")]
    public List<WellDto>? Wells { get; set; }
}

public class WellDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("platformId")]
    public int PlatformId { get; set; }

    [JsonPropertyName("uniqueName")]
    public string? UniqueName { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("lastUpdate")]
    public DateTime? LastUpdate { get; set; }
}
