using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrafficDataApi.Services;
using System.Linq;
using System.Collections.Generic;
using TrafficDataApi.Models;

namespace TrafficDataApi.Services
{
    public class TrafficDataService : IHostedService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TrafficDataService> _logger;
        private Timer _timer;

        public TrafficDataService(IHttpClientFactory httpClientFactory, ILogger<TrafficDataService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 设置定时任务，每个整点触发（例如：每小时的0分钟0秒）
            TimeSpan initialDelay = GetTimeUntilNextFullHour();
            _timer = new Timer(ExecuteTask, null, initialDelay, TimeSpan.FromHours(1));

            return Task.CompletedTask;
            // 设置定时任务，每分钟触发一次
            // _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));

            // return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void ExecuteTask(object state)
        {
            _logger.LogInformation("Executing traffic data task...");

            var districtLocations = new Dictionary<string, List<string>>
            {
                { "黄浦", new List<string> { "121.4905,31.2419", "121.4810,31.2381", "121.4903,31.2405" } },
                { "徐汇", new List<string> { "121.4375,31.1955", "121.4540,31.2049", "121.4425,31.1865" } },
                // { "长宁", new List<string> { "121.4226,31.2243", "121.4094,31.2135", "121.4103,31.2010" } },
                // { "静安", new List<string> { "121.4648,31.2291", "121.4551,31.2236", "121.4550,31.2410" } },
                // { "普陀", new List<string> { "121.4325,31.2478", "121.4178,31.2470", "121.4330,31.2530" } },
                // { "虹口", new List<string> { "121.4880,31.2640", "121.5000,31.2760", "121.5100,31.2700" } },
                // { "杨浦", new List<string> { "121.5228,31.3020", "121.5300,31.2700", "121.5200,31.2800" } },
                // { "闵行", new List<string> { "121.3800,31.1128", "121.3500,31.1600", "121.3600,31.2000" } },
                // { "宝山", new List<string> { "121.5000,31.3300", "121.4200,31.3000", "121.4800,31.4000" } },
                // { "嘉定", new List<string> { "121.2500,31.3800", "121.3200,31.2900", "121.1700,31.2900" } },
                // { "浦东", new List<string> { "121.5000,31.2400", "121.6000,31.2100", "121.5800,31.2500" } },
                // { "金山", new List<string> { "121.3300,30.7400", "121.1800,30.9000", "121.0500,30.8800" } },
                // { "松江", new List<string> { "121.2300,31.0300", "121.2000,31.1000", "121.3200,31.1200" } },
                // { "青浦", new List<string> { "121.0600,31.1200", "121.1200,31.1500", "121.2700,31.1800" } },
                // { "奉贤", new List<string> { "121.4600,30.9200", "121.5600,30.9000", "121.5000,30.9500" } },
                // { "崇明", new List<string> { "121.4000,31.6300", "121.5000,31.6800", "121.4800,31.7300" } },
            };

            string[] apiKeys = new[] { "c1e3477d2b7f99856e3b030010e7c9dc", "44db2db732d4f475404ee6a9ea7e7dc8",
                "14e4c3979bc1d9850fe9488d0b97ce55", "4e0688086ea33eca825b2eab1ae50e0e", "6a89009e649c9cc09a64a198e0e0b5ef",
                "74d62219c9bb01ee87907c231a9d82ac","0fad37385b91d721dbeb9d13bdc3fa6e","a13ef4f06db315d878fc0bfd5aed5594",
                "bd425a3d901606060abac7d8ae218040","1c504818545bdc963f325aae27c1f75d","09113358339aabf23fc6f31177654b86", "ce0320168fb47e75555980d48eea5d65", "f6a84ff59ce88262f6ea9484a30c0160" };
            int currentKeyIndex = 0;

            string GetApiKey()
            {
                var key = apiKeys[currentKeyIndex];
                currentKeyIndex = (currentKeyIndex + 1) % apiKeys.Length;
                return key;
            }

               string url = "https://restapi.amap.com/v3/traffic/status/circle";
               var districtTrafficIndexes = new Dictionary<string, object>();
               var globalUniqueRoads = new HashSet<string>();
               var allRoads = new List<Trafficroads>();  // 这里改变了变量类型，保存每个区域的完整道路数据

               foreach (var district in districtLocations)
               {
                   double totalIndex = 0;
                   int count = 0;
                   var uniqueRoads = new HashSet<string>();

                   foreach (var location in district.Value)
                   {
                       var queryParams = new[] 
                       {
                           $"key={GetApiKey()}",
                           $"level=6",
                           $"location={location}",
                           $"radius=1000",
                           $"extensions=all"
                       };
                       string requestUrl = $"{url}?{string.Join("&", queryParams)}";

                       try
                       {
                           HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                           response.EnsureSuccessStatusCode();

                           string responseBody = await response.Content.ReadAsStringAsync();
                           var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

                           if (result.TryGetProperty("trafficinfo", out var trafficInfo))
                           {
                               if (trafficInfo.TryGetProperty("evaluation", out var evaluation))
                               {
                                   double congestionIndex = CalculateCongestionIndex(evaluation);
                                   totalIndex += congestionIndex;
                                   count++;
                               }

                               if (trafficInfo.TryGetProperty("roads", out var roads) && roads.ValueKind == JsonValueKind.Array)
                               {
                                //    var processedRoads = new List<Trafficroads>();

                                   foreach (var road in roads.EnumerateArray())
                                   {
                                       if (road.TryGetProperty("name", out var name) &&
                                           road.TryGetProperty("direction", out var direction))
                                       {
                                           var roadKey = $"{name.GetString()}-{direction.GetString()}";

                                           if (uniqueRoads.Add(roadKey))
                                           {
                                               if (globalUniqueRoads.Add(roadKey))
                                               {
                                                   var fullRoadData = new Trafficroads
                                                   {
                                                       name = name.GetString(),
                                                       direction = direction.GetString(),
                                                       status = road.TryGetProperty("status", out var status) ? status.GetString() : null,
                                                       speed = road.TryGetProperty("speed", out var speed) ? speed.GetString() : null,
                                                       lcodes = road.TryGetProperty("status", out var lcodes) ? lcodes.GetString() : null
                                                   };
                                                    // processedRoads.Add(fullRoadData);
                                                     allRoads.Add(fullRoadData); // 将所有处理后的道路数据添加到 allRoads
                                               }
                                           }
                                       }
                                   }
                               }
                           }
                       }
                       catch (HttpRequestException ex)
                       {
                           _logger.LogError($"Error fetching data for location {location}: {ex.Message}");
                       }
                       await Task.Delay(125);
                   }

                   if (count > 0)
                   {
                       districtTrafficIndexes[district.Key] = Math.Round(totalIndex / count, 2);
                   }
                   else
                   {
                       districtTrafficIndexes[district.Key] = 0;
                   }
               }
                 // 将区域的完整道路数据一整条保存到 LeanCloud 中
                if (allRoads.Any())
                {
                    await LeanCloudService.InsertRoadTrafficDataToLeanCloud(allRoads);  // 确保是一整条数据保存
                }

            await LeanCloudService.InsertDistrictTrafficIndexesToLeanCloud(districtTrafficIndexes);

               var resultData = districtTrafficIndexes.Select(d => new { name = d.Key, d.Value }).ToList();
               _logger.LogInformation("Traffic data stored successfully.");
        }

        private double CalculateCongestionIndex(JsonElement evaluation)
        {
            double status = GetNumericValue(evaluation, "status");
            double congested = GetPercentage(evaluation, "congested");
            double blocked = GetPercentage(evaluation, "blocked");

            double congestionIndex = (status * 1) + (congested * 10) + (blocked * 20);
            return congestionIndex;
        }

        private double GetPercentage(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var propertyValue))
            {
                string valueString = propertyValue.GetString()?.TrimEnd('%');
                if (double.TryParse(valueString, out double value))
                {
                    return value / 100;
                }
            }
            return 0;
        }

        private double GetNumericValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var propertyValue))
            {
                if (double.TryParse(propertyValue.GetString(), out double value))
                {
                    return value;
                }
            }
            return 0;
        }      

        private TimeSpan GetTimeUntilNextFullHour()
        {
            var now = DateTime.Now;
            var nextFullHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
            return nextFullHour - now;
        }
    }
}
