using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CurrencyConverter.Infrastructure
{
    public static class SerializerSettings
    {
        public static readonly JsonSerializerOptions Default = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public static readonly Action<JsonOptions> DefaultAction = o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = Default.PropertyNamingPolicy;
        };
    }
}
