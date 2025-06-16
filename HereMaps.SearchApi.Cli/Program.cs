using HereMaps.SearchApi.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var configuration = new ConfigurationBuilder()
    .AddTomlFile("config.toml", optional: true)
    .Build();

var hereMapsOptions = new Options();
configuration.Bind(HereMapsOptions.Section, hereMapsOptions.Value);

var httpClient = new HttpClient();
var client = new HereMaps.SearchApi.ApiClient(hereMapsOptions,httpClient) {
};
var result = await client.GeocodeAsync(q: "1600 Amphitheatre Parkway, Mountain View, CA");

foreach(var item in result.Items)
{
    Console.WriteLine($"Title: {item.Title}");
    Console.WriteLine($"Address: {item.Address.Label}");
    Console.WriteLine($"Position: {item.Position.Lat}, {item.Position.Lng}");
    Console.WriteLine();
}

public class Options : IOptions<HereMapsOptions>
{
    public HereMapsOptions Value { get; set; } = new();
}