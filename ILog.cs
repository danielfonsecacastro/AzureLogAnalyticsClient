using System.Threading.Tasks;

namespace AzureLogAnalyticsClient
{
    public interface ILog
    {
        Task<bool> Error(object data);
        Task<bool> Info(object data);
        Task<bool> Success(object data);
        Task<bool> Warning(object data);
    }
}