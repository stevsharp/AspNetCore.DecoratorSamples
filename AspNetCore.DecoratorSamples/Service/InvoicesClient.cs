using AspNetCore.DecoratorSamples.Interface;
using AspNetCore.DecoratorSamples.Model;

using System.Net.Http.Headers;
using System.Text.Json;

namespace AspNetCore.DecoratorSamples.Service;

public sealed class InvoicesClient(HttpClient http, string baseUrl, Func<string> getToken) : IInvoicesClient
{
    private readonly HttpClient _http = http;
    private readonly string _baseUrl = baseUrl.TrimEnd('/');
    private readonly Func<string> _getToken = getToken;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public async Task<PagedResultDtoOfInvoice> GetInvoicesAsync(
        string filter, string orderBy, string include,
        int? pageIndex, int? pageSize, bool? transactionCurrency, bool? skipRounding,
        CancellationToken ct = default)
    {
        var url = BuildUrl(_baseUrl, filter, orderBy, include, pageIndex, pageSize, transactionCurrency, skipRounding);

        using var req = new HttpRequestMessage(HttpMethod.Get, url)
        {
#if NET5_0_OR_GREATER
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
#endif
        };

        var token = _getToken?.Invoke();
        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<PagedResultDtoOfInvoice>(stream, _json, ct).ConfigureAwait(false)
               ?? new PagedResultDtoOfInvoice();
    }

    private static string BuildUrl(
        string baseUrl, string filter, string orderBy, string include,
        int? pageIndex, int? pageSize, bool? transactionCurrency, bool? skipRounding)
    {
        var q = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrWhiteSpace(filter)) 
            q["filter"] = filter;
        if (!string.IsNullOrWhiteSpace(orderBy)) 
            q["orderBy"] = orderBy;
        if (!string.IsNullOrWhiteSpace(include)) 
            q["include"] = include;
        if (pageIndex.HasValue) q["pageIndex"] = 
                pageIndex.Value.ToString();
        if (pageSize.HasValue) q["pageSize"] = 
                pageSize.Value.ToString();
        if (transactionCurrency.HasValue) 
            q["transactionCurrency"] = transactionCurrency.Value ? "true" : "false";
        if (skipRounding.HasValue) q["skipRounding"] = skipRounding.Value ? "true" : "false";
        return $"{baseUrl}/invoices?{q}";
    }
}

