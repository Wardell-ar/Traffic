// Services/GaoDeTrafficService.cs
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace TrafficDataApi.Services
{
    public static class GaoDeTrafficService
    {
        private const string ApiKey = "ce0320168fb47e75555980d48eea5d65";
        private const string ApiUrl = "https://restapi.amap.com/v3/traffic/status/circle";

        public static async Task<string> GetTrafficDataAsync(string location, int radius = 1000, string output = "json", string extensions = "all")
        {
            using var client = new HttpClient();
            var url = $"{ApiUrl}?location={location}&radius={radius}&output={output}&extensions={extensions}&key={ApiKey}";
            
            try
            {
                var response = await client.GetStringAsync(url);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取交通数据失败: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
