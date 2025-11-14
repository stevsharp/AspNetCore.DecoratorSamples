namespace AspNetCore.DecoratorSamples;

public sealed class LoggingHandler(ILogger<LoggingHandler> log) : DelegatingHandler
{
    /// <summary>
    /// 
    /// </summary>
    private readonly ILogger<LoggingHandler> _log = log;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await base.SendAsync(request, ct);
        _log.LogInformation("{Method} {Uri} -> {Status} in {Elapsed} ms",
            request.Method, request.RequestUri, (int)response.StatusCode, sw.ElapsedMilliseconds);
        return response;
    }
}

