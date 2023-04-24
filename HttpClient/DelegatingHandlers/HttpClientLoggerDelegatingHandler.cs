using Microsoft.Extensions.Logging;
using MonitoringAgent.Logger;

namespace MonitoringAgent.HttpClient.DelegatingHandlers;
public sealed class HttpClientLoggerDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientLoggerDelegatingHandler> _logger;

    public HttpClientLoggerDelegatingHandler(ILogger<HttpClientLoggerDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        HttpResponseMessage? responseMessage = null;
        try
        {
            responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            stopwatch.Stop();
            AppLogger.HttpClient_ApiCalled(
                _logger,
                request.Method.ToString(),
                request.RequestUri!.ToString(),
                stopwatch.ElapsedMilliseconds
            );
            return responseMessage;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            AppLogger.HttpClient_ApiError(
                _logger,
                request.Method.ToString(),
                request.RequestUri!.ToString(),
                stopwatch.ElapsedMilliseconds,
                responseMessage?.StatusCode.GetHashCode() ?? 0,
                ex
            );
            throw;
        }
    }
}
