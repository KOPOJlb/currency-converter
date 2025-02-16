using CurrencyConverter.ApplicationServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace CurrencyConverter.Infrastructure.FrankfurterService
{
    public class Frankfurter(
        IMemoryCache memoryCache,
        IHttpClientFactory httpClientFactory, 
        IOptions<FrankfurterConfig> frankfurterConfig,
        ResiliencePipelineProvider<string> resiliencePipelineProvider) : ICurrencyProvider
    {
        private const int UtcExpirationTime = 15;
        private const string FrankfurterDateFormat = "yyyy-MM-dd";
        private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient(Constants.HttpClientNames.FrankfurterHttpClient);
        private readonly Uri _baseUri = new(frankfurterConfig.Value.Url ?? throw new ArgumentNullException(nameof(FrankfurterConfig.Url)));
        private readonly ResiliencePipeline _resiliencePipeline = resiliencePipelineProvider.GetPipeline(Constants.ResiliencePolicies.FrankfurterPolicy);

        public async Task<LatestResponse?> Latest(string currencyCode)
        {
            return await _memoryCache.GetOrCreateAsync(currencyCode, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow < DateTimeOffset.UtcNow.Date.AddHours(UtcExpirationTime) ? 
                    DateTimeOffset.UtcNow.Date.AddHours(UtcExpirationTime) : 
                    DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(UtcExpirationTime);

                var queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams.Add("base", currencyCode);
                var uriBuilder = new UriBuilder(new Uri(_baseUri, "v1/latest"))
                {
                    Query = queryParams.ToString()
                };

                var response = await _resiliencePipeline.ExecuteAsync(async (_) => await _httpClient.GetAsync(uriBuilder.Uri));
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default;
                }
                response.EnsureSuccessStatusCode();
                return await JsonSerializer.DeserializeAsync<LatestResponse>(response.Content.ReadAsStream(), _options);
            });
        }

        public async Task<ConvertResponse?> Convert(ConvertRequest request)
        {
            var rate = await _memoryCache.GetOrCreateAsync($"{request.Date}_{request.FromCurrencyCode}_{request.ToCurrencyCode}", async entry =>
            {
                var queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams.Add("base", request.FromCurrencyCode);
                queryParams.Add("symbols", request.ToCurrencyCode);
                var uriBuilder = new UriBuilder(new Uri(_baseUri, "v1/" + request.Date!.Value.ToString(FrankfurterDateFormat)))
                {
                    Query = queryParams.ToString()
                };

                var response = await _resiliencePipeline.ExecuteAsync(async (_) => await _httpClient.GetAsync(uriBuilder.Uri));
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent)
                {
                    return default;
                }
                response.EnsureSuccessStatusCode();
                return await JsonSerializer.DeserializeAsync<LatestResponse>(response.Content.ReadAsStream(), _options);
            });

            if (rate is null)
            {
                return default;
            }

            var result = request.FromValue!.Value * rate.Rates![request.ToCurrencyCode!];
            return new(request, result);
        }

        public async Task<ListResponse?> List(ListRequest request)
        {
            var response = await _memoryCache.GetOrCreateAsync($"{request.CurrencyCode}_{request.From}_{request.To}", async entry =>
            {
                var queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams.Add("base", request.CurrencyCode);
                var uriBuilder = new UriBuilder(new Uri(_baseUri, "v1/" + request.From!.Value.ToString(FrankfurterDateFormat) + ".." + request.To!.Value.ToString(FrankfurterDateFormat)))
                {
                    Query = queryParams.ToString()
                };

                var response = await _resiliencePipeline.ExecuteAsync(async (_) => await _httpClient.GetAsync(uriBuilder.Uri));
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent)
                {
                    return default;
                }
                response.EnsureSuccessStatusCode();
                return await JsonSerializer.DeserializeAsync<FrankfurterListResponse>(response.Content.ReadAsStream(), _options);
            });
            if (response is null)
            {
                return default;
            }
            return new ListResponse(request, response.Rates!);
        }

        private class FrankfurterListResponse
        {
            [JsonPropertyName("start_date")]
            public DateOnly? StartDate { get; set; }
            [JsonPropertyName("end_date")]
            public DateOnly? EndDate { get; set; }
            public string? Base { get; set; }
            public Dictionary<DateOnly, Dictionary<string, decimal>>? Rates { get; set; }
        }
    }
}
