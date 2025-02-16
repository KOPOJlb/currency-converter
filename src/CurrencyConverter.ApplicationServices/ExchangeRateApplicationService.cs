using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.ApplicationServices
{
    public enum CurrencyProviders
    {
        Default,
        Frankfurter
    }

    public class ExchangeRateApplicationService(
        [FromKeyedServices(CurrencyProviders.Default)] ICurrencyProvider currencyProvider, 
        IValidator<ConvertRequest> convertRequestValidator,
        IValidator<LatestRequest> latestRequestValidator,
        IValidator<ListRequest> listRequestValidator)
    {
        private readonly ICurrencyProvider _currencyProvider = currencyProvider;
        private readonly IValidator<ConvertRequest> _convertRequestValidator = convertRequestValidator;
        private readonly IValidator<LatestRequest> _latestRequestValidator = latestRequestValidator;
        private readonly IValidator<ListRequest> _listRequestValidator = listRequestValidator;

        public async Task<LatestResponse> Latest(LatestRequest request)
        {
            await _latestRequestValidator.ValidateAndThrowAsync(request);
            return await _currencyProvider.Latest(request.CurrencyCode!).ThrowIfNull(request.CurrencyCode);
        }

        public async Task<ConvertResponse> Convert(ConvertRequest request)
        {
            await _convertRequestValidator.ValidateAndThrowAsync(request);
            return await _currencyProvider.Convert(request).ThrowIfNull(request.FromCurrencyCode);
        }

        public async Task<ListResponse> List(ListRequest request)
        {
            await _listRequestValidator.ValidateAndThrowAsync(request);
            return await _currencyProvider.List(request).ThrowIfNull(request.From);
        }
    }

    public class LatestResponse
    {
        public string? Base { get; set; }
        public DateOnly? Date { get; set; }
        public Dictionary<string, decimal>? Rates { get; set; }
    }

    public class ListRequest
    {
        public string? CurrencyCode { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int GetPageNumber() => PageNumber ?? 1;
        public int GetPageSize() => PageSize ?? 10;
    }

    public class ListResponse
    {
        public ListResponse(ListRequest request, Dictionary<DateOnly, Dictionary<string, decimal>> rates)
        {
            StartDate = request.From!.Value;
            EndDate = request.To!.Value;
            Base = request.CurrencyCode!;
            PageNumber = request.GetPageNumber();
            PageSize = request.GetPageSize();
            Rates =  rates.SelectMany(d => d.Value.Select(inner => new RateResponse(d.Key, inner.Key, inner.Value)))
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CurrencyCode)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
        public DateOnly StartDate { get; }
        public DateOnly EndDate { get; }
        public string Base { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public IEnumerable<RateResponse> Rates { get; }
        public class RateResponse
        {
            public RateResponse(DateOnly date, string currencyCode, decimal rate)
            {
                Date = date;
                CurrencyCode = currencyCode;
                Rate = rate;
            }

            public DateOnly Date { get; }
            public string CurrencyCode { get; }
            public decimal Rate { get; }
        }
    }

    public class ListRequestValidator : AbstractValidator<ListRequest>
    {
        public ListRequestValidator()
        {
            RuleFor(x => x.CurrencyCode)
                .NotEmpty();

            RuleFor(x => x.From)
                .NotEmpty();

            RuleFor(x => x.To)
                .NotEmpty();
        }
    }

    public class LatestRequest
    {
        public string? CurrencyCode { get; set; }
    }
    public class LatestRequestValidator : AbstractValidator<LatestRequest>
    {
        public LatestRequestValidator()
        {
            RuleFor(x => x.CurrencyCode)
                .NotEmpty();
        }
    }

    public class ConvertRequest
    {
        public string? FromCurrencyCode { get; set; }
        public string? ToCurrencyCode { get; set; }
        public DateOnly? Date { get; set; }
        public decimal? FromValue { get; set; }
    }

    public class ConvertResponse
    {
        public ConvertResponse(ConvertRequest request, decimal result)
        {
            FromCurrencyCode = request.FromCurrencyCode;
            ToCurrencyCode = request.ToCurrencyCode;
            Date = request.Date;
            FromValue = request.FromValue;
            Result = result;
        }
        public string? FromCurrencyCode { get; set; }
        public string? ToCurrencyCode { get; set; }
        public DateOnly? Date { get; set; }
        public decimal? FromValue { get; set; }
        public decimal? Result { get; set; }
    }

    public class ConvertRequestValidator : AbstractValidator<ConvertRequest>
    {
        private static readonly HashSet<string> CurrencyCodeForExclude = ["TRY,", "PLN", "THB", "MXN"];
        public ConvertRequestValidator()
        {
            RuleFor(x => x.FromCurrencyCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(x => CurrencyCodeForExclude.Contains(x!) == false)
                .WithMessage("Currecy code is not allowed");

            RuleFor(x => x.ToCurrencyCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(x => CurrencyCodeForExclude.Contains(x!) == false)
                .WithMessage("Currecy code is not allowed");

            RuleFor(x => x.Date)
                .NotEmpty();
        }
    }
}
