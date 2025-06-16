using System.Net.Http.Headers;
using HereMaps.SearchApi.Options;
using Microsoft.Extensions.Options;

namespace HereMaps.SearchApi;

public class ApiClientBase(IOptions<HereMapsOptions> options)
{
    protected readonly HereMapsOptions Options = options.Value;    
}

public partial class ApiClient: ApiClientBase
{
    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        request.RequestUri = new Uri(request.RequestUri + $"&apikey={Options.ApiKey}");
        request.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse("HereMaps.SearchApi/0.0.0"));
    }
}
