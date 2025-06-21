using System.Text.Json.Serialization;

namespace VacationAdvisor.WinUi.Entities;

public record Place
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }
    
    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }
}
