using CurrencyConverter.Api.IntegrationTests.TestData;
using CurrencyConverter.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace CurrencyConverter.Api.IntegrationTests
{
    public class ExchangeRatesControllerTests
    {
        private readonly HttpClient _httpClient;
        public ExchangeRatesControllerTests()
        {
            _httpClient = new CurrencyConverterWebApplicationFactory()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddAuthentication(defaultScheme: "TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                                "TestScheme", options => { });
                    });
                })
                .CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task LatestShouldReturnBadRequestWithoutCurrency()
        {
            var response = await _httpClient.GetAsync("v1/exchange-rates/latest");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task LatestShouldReturnNotFoundIncaseExternalProviderRetursNotFound()
        {
            var response = await GetClient(HttpStatusCode.NotFound, "").GetAsync("v1/exchange-rates/latest?currencyCode=USD");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task LatestShouldReturnSuccessResponse()
        {
            var response = await GetClient(HttpStatusCode.OK, Encoding.UTF8.GetString(FrankfurterResponses.LatestSuccessResponse)).GetAsync("v1/exchange-rates/latest?currencyCode=EUR");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().BeEquivalentTo("{\"base\":\"EUR\",\"date\":\"2025-02-14\",\"rates\":{\"AUD\":1.6514,\"BGN\":1.9558,\"BRL\":6.0127,\"CAD\":1.4856,\"CHF\":0.9442,\"CNY\":7.6141,\"CZK\":25.043,\"DKK\":7.459,\"GBP\":0.83215,\"HKD\":8.1554,\"HUF\":402.95,\"IDR\":16980,\"ILS\":3.7341,\"INR\":90.81,\"ISK\":147.3,\"JPY\":160.09,\"KRW\":1509.5,\"MXN\":21.315,\"MYR\":4.647,\"NOK\":11.6515,\"NZD\":1.8352,\"PHP\":60.487,\"PLN\":4.1653,\"RON\":4.977,\"SEK\":11.2445,\"SGD\":1.4052,\"THB\":35.238,\"TRY\":37.949,\"USD\":1.0478,\"ZAR\":19.2555}}");
        }

        [Test]
        public async Task ConvertShouldReturnSuccessResponse()
        {
            var fromCurrencyCode = "EUR";
            var toCurrencyCode = "USD";
            var date = "2025-02-14";
            var fromValue = 1;
            var response = await GetClient(HttpStatusCode.OK, Encoding.UTF8.GetString(FrankfurterResponses.LatestSuccessResponse)).GetAsync($"v1/exchange-rates/convert?{nameof(fromCurrencyCode)}={fromCurrencyCode}&{nameof(toCurrencyCode)}={toCurrencyCode}&{nameof(date)}={date}&{nameof(fromValue)}={fromValue}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().BeEquivalentTo("{\"fromCurrencyCode\":\"EUR\",\"toCurrencyCode\":\"USD\",\"date\":\"2025-02-14\",\"fromValue\":1,\"result\":1.0478}");
        }

        [Test]
        public async Task ListShouldReturnSuccessResponse()
        {
            var response = await GetClient(HttpStatusCode.OK, Encoding.UTF8.GetString(FrankfurterResponses.ListSuccessResponse)).GetAsync("v1/exchange-rates/list?currencyCode=EUR&from=2025-02-14&to=2025-02-14");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().BeEquivalentTo("{\"startDate\":\"2025-02-14\",\"endDate\":\"2025-02-14\",\"base\":\"EUR\",\"pageNumber\":1,\"pageSize\":10,\"rates\":[{\"date\":\"2025-02-14\",\"currencyCode\":\"ZAR\",\"rate\":18.3771},{\"date\":\"2025-02-14\",\"currencyCode\":\"TRY\",\"rate\":36.218},{\"date\":\"2025-02-14\",\"currencyCode\":\"THB\",\"rate\":33.63},{\"date\":\"2025-02-14\",\"currencyCode\":\"SGD\",\"rate\":1.3411},{\"date\":\"2025-02-14\",\"currencyCode\":\"SEK\",\"rate\":10.7315},{\"date\":\"2025-02-14\",\"currencyCode\":\"RON\",\"rate\":4.75},{\"date\":\"2025-02-14\",\"currencyCode\":\"PLN\",\"rate\":3.9753},{\"date\":\"2025-02-14\",\"currencyCode\":\"PHP\",\"rate\":57.728},{\"date\":\"2025-02-14\",\"currencyCode\":\"NZD\",\"rate\":1.7515},{\"date\":\"2025-02-14\",\"currencyCode\":\"NOK\",\"rate\":11.12}]}");
        }

        private static HttpClient GetClient(HttpStatusCode statusCode, string content)
        {
            return new CurrencyConverterWebApplicationFactory()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddAuthentication(defaultScheme: "TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                                "TestScheme", options => { });

                        SetMockResponse(services, statusCode, content);
                    });                    
                })
                .CreateClient();
        }

        private static void SetMockResponse(IServiceCollection services, HttpStatusCode statusCode, string content)
        {
            var httpMock = new Mock<HttpClientHandler>();
            httpMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage
                {
                    Content = new StringContent(content),
                    StatusCode = statusCode
                });

            services
                .AddHttpClient(Constants.HttpClientNames.FrankfurterHttpClient)
                .ConfigurePrimaryHttpMessageHandler(_ => httpMock.Object);
        }
    }
}