using AspNetCore.DecoratorSamples.Model;

namespace AspNetCore.DecoratorSamples.Interface;

public interface IInvoicesClient
{
    Task<PagedResultDtoOfInvoice> GetInvoicesAsync(
        string filter, string orderBy, string include,
        int? pageIndex, int? pageSize, bool? transactionCurrency, bool? skipRounding,
        CancellationToken ct = default);
}
