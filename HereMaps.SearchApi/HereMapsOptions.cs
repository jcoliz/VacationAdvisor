namespace HereMaps.SearchApi.Options;

/// <summary>
/// Options describing the connection to Here Maps Search API.
/// </summary>
public class HereMapsOptions
{
    /// <summary>
    /// Config file section
    /// </summary>
    public static readonly string Section = "HereMaps";

    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Application (client) ID
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;
}
