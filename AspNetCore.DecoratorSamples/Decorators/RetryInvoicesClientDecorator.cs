namespace AspNetCore.DecoratorSamples.Decorators;

using AspNetCore.DecoratorSamples.Interface;
using AspNetCore.DecoratorSamples.Model;

using Polly;

using System.Net.Sockets;


public sealed class RetryInvoicesClientDecorator(IInvoicesClient inner) : IInvoicesClient
{
    private readonly IInvoicesClient _inner = inner;
    private readonly IAsyncPolicy _retry = Policy
            .Handle<HttpRequestException>()
            .Or<IOException>()
            .Or<SocketException>()
            .Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            .WaitAndRetryAsync(3,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) +
                           TimeSpan.FromMilliseconds(Random.Shared.Next(0, 750)));
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <param name="include"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="transactionCurrency"></param>
    /// <param name="skipRounding"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<PagedResultDtoOfInvoice> GetInvoicesAsync(
        string filter, string orderBy, string include,
        int? pageIndex, int? pageSize, bool? transactionCurrency, bool? skipRounding,
        CancellationToken ct = default)
    =>
        _retry.ExecuteAsync(t =>
            _inner.GetInvoicesAsync(filter, orderBy, include, pageIndex, pageSize, transactionCurrency, skipRounding, t),
            ct);
}

