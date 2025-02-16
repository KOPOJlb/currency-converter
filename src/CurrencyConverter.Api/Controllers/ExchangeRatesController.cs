using CurrencyConverter.ApplicationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
    [Authorize]
    [Route("/v1/exchange-rates")]
    public class ExchangeRatesController(ExchangeRateApplicationService exchangeRateApplicationService) : ControllerBase
    {
        private readonly ExchangeRateApplicationService _exchangeRateApplicationService = exchangeRateApplicationService;

        [Authorize(Roles = "latest")]
        [HttpGet("latest")]
        public async Task<LatestResponse> Latest([FromQuery] LatestRequest request)
        {
            return await _exchangeRateApplicationService.Latest(request);
        }

        [Authorize(Roles = "convert")]
        [HttpGet("convert")]
        public async Task<ConvertResponse> Convert([FromQuery] ConvertRequest request)
        {
            return await _exchangeRateApplicationService.Convert(request);
        }

        [Authorize(Roles = "list")]
        [HttpGet("list")]
        public async Task<ListResponse> List([FromQuery] ListRequest request)
        {
            return await _exchangeRateApplicationService.List(request);
        }
    }
}
