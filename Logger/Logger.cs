using Microsoft.Extensions.Logging;

namespace MonitoringAgent.Logger
{
    public static partial class AppLogger
    {
        public static readonly EventId HttpClientCalled = new(1, nameof(HttpClientCalled));
        public static readonly EventId HttpClientError = new(2, nameof(HttpClientError));


        private static readonly Action<ILogger, string, string, long, Exception?> _supplierApiCalled = LoggerMessage.Define<string, string, long>(
                LogLevel.Information,
                HttpClientCalled,
                "send {method} request to {url} in {elapsed} ms.");

        private static readonly Action<ILogger, string, string, long, int, Exception?> _supplierApiError = LoggerMessage.Define<string, string, long, int>(
                LogLevel.Error,
                HttpClientError,
                "send {method} request to {url} in {elapsed} ms. status = {status}");



        public static void HttpClient_ApiCalled(ILogger logger, string method, string url, long elapsed) => _supplierApiCalled(logger, method.ToUpper(), url, elapsed, null);
        public static void HttpClient_ApiError(ILogger logger, string method, string url, long elapsed, int status, Exception ex) => _supplierApiError(logger, method, url, elapsed, status, ex);

    }
}
