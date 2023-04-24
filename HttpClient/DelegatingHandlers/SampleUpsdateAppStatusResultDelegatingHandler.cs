using Microsoft.Extensions.Logging;
using MonitoringAgent.Models;

namespace MonitoringAgent.HttpClient.DelegatingHandlers;

/*
    This is a sample delegating handler that returns a list of applications. You must remove this delegating handler from the HTTP client pipeline when using a real API to retrieve the list of applications.
 */
public sealed class SampleUpdateAppStatusResultDelegatingHandler : DelegatingHandler
{
    static string[] AppsList = new string[5] { "App1", "App2", "App3", "App4", "App5" };
    private readonly ILogger<SampleUpdateAppStatusResultDelegatingHandler> _logger;

    public SampleUpdateAppStatusResultDelegatingHandler(ILogger<SampleUpdateAppStatusResultDelegatingHandler> logger)
    {
        _logger = logger;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        this._logger.LogInformation("SampleUpdateAppStatusResultDelegatingHandler");
        return Task.FromResult(
            new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new UpdateAppStatusResponse(true)), System.Text.Encoding.UTF8, "application/json")
            }
        );
    }
}
