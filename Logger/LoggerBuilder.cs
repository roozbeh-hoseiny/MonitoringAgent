using Microsoft.Extensions.Configuration;
using Serilog;

internal sealed class LoggerBuilder
{
    public LoggerConfiguration Build(IConfiguration config)
    {
        var template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        var result = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                outputTemplate: template)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error);
        ;
        return result;
    }
}