using System.Net;

namespace MonitoringAgent.HttpClient.DelegatingHandlers;

public sealed class HttpClientDefaultDelegatingHandler : HttpClientHandler
{
    public HttpClientDefaultDelegatingHandler() => AutomaticDecompression = DecompressionMethods.All;
}
