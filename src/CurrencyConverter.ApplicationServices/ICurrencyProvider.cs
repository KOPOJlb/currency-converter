namespace CurrencyConverter.ApplicationServices
{
    public interface ICurrencyProvider
    {
        Task<LatestResponse?> Latest(string currencyCode);
        Task<ConvertResponse?> Convert(ConvertRequest request);
        Task<ListResponse?> List(ListRequest request);
    }
}
