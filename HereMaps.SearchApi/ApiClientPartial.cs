using System.Net.Http.Headers;

namespace HereMaps.SearchApi;
public partial class ApiClient
{
    public string ApiKey { get; set; } = string.Empty;
    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        request.RequestUri = new Uri(request.RequestUri + $"&apikey={ApiKey}");
        request.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse("Weather.Worker/0.0.0"));
    }
}
