// Services/TrafficParser.cs
using Newtonsoft.Json;
using TrafficDataApi.Models;

namespace TrafficDataApi.Services
{
    public static class TrafficParser
    {
        public static TrafficInfo? ParseTrafficData(string jsonData)
        {
            try
            {
                var trafficResponse = JsonConvert.DeserializeObject<TrafficResponse>(jsonData);
                return trafficResponse?.Trafficinfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析交通数据失败: {ex.Message}");
                return null;
            }
        }
    }
}
