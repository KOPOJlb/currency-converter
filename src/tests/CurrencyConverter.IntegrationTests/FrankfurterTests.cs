using CurrencyConverter.Infrastructure.FrankfurterService;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.IntegrationTests
{
    public class FrankfurterTests
    {
        [Test]
        public async Task ShouldReturnLatest()
        {
            var frankfurter = CompositionRoot.ServiceProvider.GetRequiredService<Frankfurter>();
            var response = await frankfurter.Latest("USD");
            response.Base.Should().Be("USD");
            response.Rates.Should().ContainKeys("BGN", "BRL");
        }

        [Test]
        public async Task ShouldConvert()
        {
            var frankfurter = CompositionRoot.ServiceProvider.GetRequiredService<Frankfurter>();
            var response = await frankfurter.Convert(new() { FromCurrencyCode = "USD", ToCurrencyCode = "EUR", Date = new DateOnly(2025, 2, 14), FromValue = 1 });
            response.FromCurrencyCode.Should().Be("USD");
            response.ToCurrencyCode.Should().Be("EUR");
            response.Date.Should().Be(new DateOnly(2025, 2, 14));
            response.FromValue.Should().Be(1);
            response.Result.Should().Be(0.95438M);
        }

        [Test]
        public async Task ShouldReturnList()
        {
            var frankfurter = CompositionRoot.ServiceProvider.GetRequiredService<Frankfurter>();
            var response = await frankfurter.List(new() { CurrencyCode = "USD", From = new DateOnly(2025, 2, 14), To = new DateOnly(2025, 2, 14)});
            response.Base.Should().Be("USD");
            response.Rates.Should().HaveCount(10);
        }
    }
}