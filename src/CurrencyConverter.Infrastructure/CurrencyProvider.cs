using CurrencyConverter.ApplicationServices;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Infrastructure
{
    public class CurrencyProvider(IServiceProvider serviceProvider) : ICurrencyProvider
    {
        public async Task<ConvertResponse> Convert(ConvertRequest request)
        {
            return await GetImplementation(CurrencyProviders.Frankfurter).Convert(request);
        }

        public async Task<LatestResponse> Latest(string currencyCode)
        {
            return await GetImplementation(CurrencyProviders.Frankfurter).Latest(currencyCode);
        }

        public async Task<ListResponse> List(ListRequest request)
        {
            return await GetImplementation(CurrencyProviders.Frankfurter).List(request);
        }

        public ICurrencyProvider GetImplementation(CurrencyProviders currencyProvider)
        {
            return currencyProvider switch 
            { 
                CurrencyProviders.Frankfurter => serviceProvider.GetRequiredKeyedService<ICurrencyProvider>(CurrencyProviders.Frankfurter),
                _ => throw new ArgumentOutOfRangeException(nameof(currencyProvider), currencyProvider, "GetImplementation is not implemented for value")
            };
        }
    }
}
