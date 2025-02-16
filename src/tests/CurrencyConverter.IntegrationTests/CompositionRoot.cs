using CurrencyConverter.Infrastructure;
using CurrencyConverter.Infrastructure.FrankfurterService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.IntegrationTests
{
    public static class CompositionRoot
    {
        private readonly static ServiceCollection _serviceCollection = new();
        public readonly static IServiceProvider ServiceProvider;
        static CompositionRoot()
        {
            var config = new ConfigurationBuilder().Build();
            _serviceCollection.AddFrankfurter(config);
            _serviceCollection.AddTransient<Frankfurter>();
            _serviceCollection.Configure<FrankfurterConfig>(x =>
            {
                x.Url = "https://api.frankfurter.dev/";
            });
            ServiceProvider = _serviceCollection.BuildServiceProvider();
        }
    }
}
