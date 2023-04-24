using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitoringAgent.HttpClient.DelegatingHandlers;
using Serilog;
using System.Net.Http;
using System;
using System.Net.Http.Json;
using MonitoringAgent.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

var host = CreateHostBuilder(args).Build();
host.Run();
IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureLogging((hostingContext, logging) =>
        {
            Log.Logger = new LoggerBuilder().Build(hostingContext.Configuration!).CreateLogger();
        })
        .ConfigureServices((hostBuilderContext, services) =>
        {
            services
                .AddTransient<HttpClientLoggerDelegatingHandler>()
                
                // This is a sample delegating handler that returns a list of applications. You must remove this delegating handler from the HTTP client pipeline when using a real API to retrieve the list of applications.
                .AddTransient<SampleAppListResultDelegatingHandler>()

                .AddHttpClient(Constants.Application_List_Api_Name, httpClient => {
                    // set your api url for list of applications
                    httpClient.BaseAddress = new Uri("https://google.com");
                })
                .AddHttpMessageHandler<HttpClientLoggerDelegatingHandler>()

                // This is a sample delegating handler that returns a list of applications. You must remove this delegating handler from the HTTP client pipeline when using a real API to retrieve the list of applications.
                .AddHttpMessageHandler<SampleAppListResultDelegatingHandler>();

            services
                .AddTransient<HttpClientLoggerDelegatingHandler>()
                
                // This is a sample delegating handler that returns a list of applications. You must remove this delegating handler from the HTTP client pipeline when using a real API to retrieve the list of applications.
                .AddTransient<SampleUpdateAppStatusResultDelegatingHandler>()
                
                .AddHttpClient(Constants.Upadte_List_Api_Name, httpClient => {
                    // set your api url for updating application statuses
                    httpClient.BaseAddress = new Uri("https://google2.com");
                })
                .AddHttpMessageHandler<HttpClientLoggerDelegatingHandler>()

                // This is a sample delegating handler that returns a list of applications. You must remove this delegating handler from the HTTP client pipeline when using a real API to retrieve the list of applications.
                .AddHttpMessageHandler<SampleUpdateAppStatusResultDelegatingHandler>();

            services.AddHostedService<ServiceWorker>();
        });
}
internal sealed class ServiceWorker : BackgroundService
{
    private readonly PeriodicTimer _appListTimer;
    private readonly PeriodicTimer _appStatusUpdaterTimer;
    private readonly HttpClient _appListHttpClient;
    private readonly HttpClient _appStatusUpdaterHttpClient;
    private readonly ILogger<ServiceWorker> _logger;
    private static object appListLockObjec = new object();
    private IEnumerable<AppListApiResponse> _appList = Enumerable.Empty<AppListApiResponse>();

    public ServiceWorker(IHttpClientFactory httpClientFactory, ILogger<ServiceWorker> logger)
    {
        this._appListTimer = new PeriodicTimer(TimeSpan.FromSeconds(120));
        this._appStatusUpdaterTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        this._appListHttpClient = httpClientFactory.CreateClient(Constants.Application_List_Api_Name);
        this._appStatusUpdaterHttpClient = httpClientFactory.CreateClient(Constants.Upadte_List_Api_Name);
        this._logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await this.UpdateAppList(stoppingToken).ConfigureAwait(false);
        await this.UpdateAppStatus(stoppingToken).ConfigureAwait(false);
        _ = Task.Run(() => AppListUpdater(stoppingToken), stoppingToken);
        _ = Task.Run(() => AppStatusUpdater(stoppingToken), stoppingToken);
    }

    private async Task AppListUpdater(CancellationToken stoppingToken)
    {
        while (await _appListTimer.WaitForNextTickAsync())
        {
            await this.UpdateAppList(stoppingToken);
        }
    }
    private async Task AppStatusUpdater(CancellationToken stoppingToken)
    {
        while (await _appStatusUpdaterTimer.WaitForNextTickAsync())
        {
            await this.UpdateAppStatus(stoppingToken);
        }
    }
    private async Task UpdateAppList(CancellationToken stoppingToken)
    {
        this._logger.LogInformation("Updating app list ....");
        var appList = await this._appListHttpClient.GetFromJsonAsync<IEnumerable<AppListApiResponse>>("/applist", stoppingToken).ConfigureAwait(false);
        if (appList?.Any() ?? false)
        {
            lock (appListLockObjec)
            {
                _appList = appList;
            }
        }
        this._logger.LogInformation($"{(appList?.Count() ?? 0)} app list read ...");
    }
    private IEnumerable<UpdateAppStatusRequest> GetServiceStatus()
    {
        lock (appListLockObjec)
        {
            if (!this._appList.Any()) return Enumerable.Empty<UpdateAppStatusRequest>();

            return this._appList.Select(s => new UpdateAppStatusRequest(s.AppName, "up and run"));
        }
    }
    private async Task UpdateAppStatus(CancellationToken stoppingToken)
    {
        var appStatuses = this.GetServiceStatus();
        if (!appStatuses.Any()) return;
        this._logger.LogInformation("Updating app status ....");
        var responseMessage = await this._appStatusUpdaterHttpClient.PostAsJsonAsync("/updatestatus", appStatuses, stoppingToken).ConfigureAwait(false);
        var responseBody = await responseMessage.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateAppStatusResponse>(responseBody);
        this._logger.LogInformation($"update result = {result}");
    }
}
