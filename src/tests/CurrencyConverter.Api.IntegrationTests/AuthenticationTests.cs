using FluentAssertions;

namespace CurrencyConverter.Api.IntegrationTests
{
    public class AuthenticationTests
    {
        public class ExchangeRatesControllerTests
        {
            private readonly HttpClient _httpClient;
            public ExchangeRatesControllerTests()
            {
                _httpClient = new CurrencyConverterWebApplicationFactory()
                    .CreateClient();
            }

            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                _httpClient.Dispose();
            }

            [Test]
            public async Task LatestShouldReturnUnauthorizedWithoutToken()
            {
                var response = await _httpClient.GetAsync("v1/exchange-rates/latest");
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
            }

            [Test]
            public async Task ConvertShouldReturnUnauthorizedWithoutToken()
            {
                var response = await _httpClient.GetAsync("v1/exchange-rates/convert");
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
            }

            [Test]
            public async Task ListShouldReturnUnauthorizedWithoutToken()
            {
                var response = await _httpClient.GetAsync("v1/exchange-rates/list");
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
            }
        }
    }
}
