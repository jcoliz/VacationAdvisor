using HereMaps.SearchApi.Options;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddTomlFile("config.toml", optional: true)
    .Build();

var hereMapsOptions = new HereMapsOptions();
configuration.Bind(HereMapsOptions.Section, hereMapsOptions);

var httpClient = new HttpClient();
var client = new HereMaps.SearchApi.ApiClient(httpClient) {
    BaseUrl = hereMapsOptions.BaseUrl,
    ApiKey =  hereMapsOptions.ApiKey
};
var result = await client.GeocodeAsync(q: "1600 Amphitheatre Parkway, Mountain View, CA");

foreach(var item in result.Items)
{
    Console.WriteLine($"Title: {item.Title}");
    Console.WriteLine($"Address: {item.Address.Label}");
    Console.WriteLine($"Position: {item.Position.Lat}, {item.Position.Lng}");
    Console.WriteLine();
}
// Note: Make sure to set the API key in the environment variable