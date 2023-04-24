namespace MonitoringAgent.Models
{
    internal record AppListApiResponse(string AppName);
    internal record UpdateAppStatusRequest(string AppName, string Status);
    internal record UpdateAppStatusResponse(bool Done);

}
