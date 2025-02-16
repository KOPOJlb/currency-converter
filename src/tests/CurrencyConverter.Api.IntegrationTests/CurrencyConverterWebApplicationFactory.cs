using CurrencyConverter.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CurrencyConverter.Api.IntegrationTests
{
    public class CurrencyConverterWebApplicationFactory : WebApplicationFactory<Main>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var httpMock = new Mock<HttpClientHandler>();
                httpMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage
                {
                    Content = new StringContent(""),
                    StatusCode = System.Net.HttpStatusCode.OK
                });

                services
                    .AddHttpClient(Constants.HttpClientNames.FrankfurterHttpClient)
                    .ConfigurePrimaryHttpMessageHandler(_ => httpMock.Object);
            });
        }
    }

    public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Role, "latest"), new Claim(ClaimTypes.Role, "convert"), new Claim(ClaimTypes.Role, "list") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}
