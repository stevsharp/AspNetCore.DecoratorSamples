namespace AspNetCore.DecoratorSamples.Decorators;

using AspNetCore.DecoratorSamples.Interface;
using AspNetCore.DecoratorSamples.Model;

public sealed class LoggingInvoicesClientDecorator : IInvoicesClient
{
    private readonly IInvoicesClient _inner;
    private readonly ILogger<LoggingInvoicesClientDecorator> _log;

    public LoggingInvoicesClientDecorator(IInvoicesClient inner, ILogger<LoggingInvoicesClientDecorator> log)
    { _inner = inner; _log = log; }

    public async Task<PagedResultDtoOfInvoice> GetInvoicesAsync(
        string filter, string orderBy, string include,
        int? pageIndex, int? pageSize, bool? transactionCurrency, bool? skipRounding,
        CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            _log.LogInformation("Invoices request pageIndex={PageIndex} pageSize={PageSize}", pageIndex, pageSize);
            var res = await _inner.GetInvoicesAsync(filter, orderBy, include, pageIndex, pageSize, transactionCurrency, skipRounding, ct);
            _log.LogInformation("Invoices response count={Count} in {Elapsed} ms", res?.Data?.Count ?? 0, sw.ElapsedMilliseconds);
            return res;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Invoices failed in {Elapsed} ms", sw.ElapsedMilliseconds);
            throw;
        }
    }
}

