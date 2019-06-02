using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AzureLogAnalyticsClient
{
    public class Log : ILog
    {
        private readonly string _apiVersion;
        private readonly string _workspaceId;
        private readonly string _sharedKey;

        public Log(string workspaceId, string sharedKey, string apiVersion = "2016-04-01")
        {
            _workspaceId = workspaceId;
            _sharedKey = sharedKey;
            _apiVersion = apiVersion;
        }

        public Log()
        {
            _workspaceId = ConfigurationManager.AppSettings.Get("WorkspaceId");
            _sharedKey = ConfigurationManager.AppSettings.Get("SharedKey");
            _apiVersion = ConfigurationManager.AppSettings.Get("ApiVersion") ?? "2016-04-01";
        }

        public async Task<bool> Info(object data) => await Post(data, "Info");
        public async Task<bool> Warning(object data) => await Post(data, "Warning");
        public async Task<bool> Error(object data) => await Post(data, "Error");
        public async Task<bool> Success(object data) => await Post(data, "Success");

        private async Task<bool> Post(object data, string logType)
        {
            var requestUriString = $"https://{_workspaceId}.ods.opinsights.azure.com/api/logs?api-version={_apiVersion}";
            var contentType = "application/json";
            var dateString = DateTime.UtcNow.ToString("r");

            var json = JsonConvert.SerializeObject(data);

            var signature = GetSignature("POST", json.Length, contentType, dateString, "/api/logs");

            var result = await requestUriString
                    .WithHeaders(new { content_type = contentType })
                    .WithHeader("Log-Type", logType)
                    .WithHeader("x-ms-date", dateString)
                    .WithHeader("Authorization", signature)
                    .PostStringAsync(json);

            return result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.Accepted;
        }

        private string GetSignature(string method, int contentLength, string contentType, string date, string resource)
        {
            string message = $"{method}\n{contentLength}\n{contentType}\nx-ms-date:{date}\n{resource}";
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            using (HMACSHA256 encryptor = new HMACSHA256(Convert.FromBase64String(_sharedKey)))
            {
                return $"SharedKey {_workspaceId}:{Convert.ToBase64String(encryptor.ComputeHash(bytes))}";
            }
        }
    }
}
