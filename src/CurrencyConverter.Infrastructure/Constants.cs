namespace CurrencyConverter.Infrastructure
{
    public static class Constants
    {
        public static class ResiliencePolicies
        {
            public const string FrankfurterPolicy = nameof(FrankfurterPolicy);
        }
        public static class HttpClientNames
        {
            public const string FrankfurterHttpClient = nameof(FrankfurterHttpClient);
        }
        public static class RateLimitPolicies
        {
            public const string Fixed = nameof(Fixed);
        }
    }
}
