using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TrafficDataApi.Models;

namespace TrafficDataApi.Services
{
    public class TrafficDataBackgroundService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TrafficDataBackgroundService> _logger;
        private Timer _timer;

        public TrafficDataBackgroundService(IHttpClientFactory httpClientFactory, ILogger<TrafficDataBackgroundService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // 在 ExecuteAsync 中实现定时任务
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // // 计算到下一个整点的时间差
            // TimeSpan initialDelay = GetTimeUntilNextFullHour();

            // // 初次延迟，等待下一个整点触发
            // await Task.Delay(initialDelay, stoppingToken);

            // // 创建定时器，每小时执行一次任务
            // _timer = new Timer(ExecuteTask, stoppingToken, TimeSpan.Zero, TimeSpan.FromHours(1));
             
             // 创建定时器，每两分钟执行一次任务
            _timer = new Timer(ExecuteTask, stoppingToken, TimeSpan.Zero, TimeSpan.FromHours(1));


            // 保持服务运行直到被停止
            await Task.CompletedTask;
        }

        // 定时执行的任务逻辑
        private async void ExecuteTask(object state)
        {
            var cancellationToken = (CancellationToken)state;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogInformation("Executing traffic data task...");

            var districtLocations = new Dictionary<string, List<string>>
            {
            { "黄浦", new List<string> { "121.4905,31.2419", "121.4810,31.2381", "121.4903,31.2405" } },
            { "徐汇", new List<string> { "121.4375,31.1955", "121.4540,31.2049", "121.4425,31.1865" } },
            { "长宁", new List<string> { "121.4226,31.2243", "121.4094,31.2135", "121.4103,31.2010" } },
            { "静安", new List<string> { "121.4648,31.2291", "121.4551,31.2236", "121.4550,31.2410" } },
            { "普陀", new List<string> { "121.4325,31.2478", "121.4178,31.2470", "121.4330,31.2530" } },
            { "虹口", new List<string> { "121.4880,31.2640", "121.5000,31.2760", "121.5100,31.2700" } },
            { "杨浦", new List<string> { "121.5228,31.3020", "121.5300,31.2700", "121.5200,31.2800" } },
            { "闵行", new List<string> { "121.3800,31.1128", "121.3500,31.1600", "121.3600,31.2000" } },
            { "宝山", new List<string> { "121.5000,31.3300", "121.4200,31.3000", "121.4800,31.4000" } },
            { "嘉定", new List<string> { "121.2500,31.3800", "121.3200,31.2900", "121.1700,31.2900" } },
            { "浦东", new List<string> { "121.5000,31.2400", "121.6000,31.2100", "121.5800,31.2500" } },
            { "金山", new List<string> { "121.3300,30.7400", "121.1800,30.9000", "121.0500,30.8800" } },
            { "松江", new List<string> { "121.2300,31.0300", "121.2000,31.1000", "121.3200,31.1200" } },
            { "青浦", new List<string> { "121.0600,31.1200", "121.1200,31.1500", "121.2700,31.1800" } },
            };
// "c1e3477d2b7f99856e3b030010e7c9dc", "44db2db732d4f475404ee6a9ea7e7dc8",
//                 "14e4c3979bc1d9850fe9488d0b97ce55", "4e0688086ea33eca825b2eab1ae50e0e", "6a89009e649c9cc09a64a198e0e0b5ef",
//                 "74d62219c9bb01ee87907c231a9d82ac","0fad37385b91d721dbeb9d13bdc3fa6e","a13ef4f06db315d878fc0bfd5aed5594",
//                 "bd425a3d901606060abac7d8ae218040","1c504818545bdc963f325aae27c1f75d","09113358339aabf23fc6f31177654b86", "ce0320168fb47e75555980d48eea5d65", "f6a84ff59ce88262f6ea9484a30c0160","79d94304c2ee63effd8f35c2ec918b7a","19df7a012b1e29d609d16fa92f789771","3ae027c5cf5a06a6b4b73b2e4af51cb8","5caf6d7ff0e3ec5a47835ac5bbb57573","d0d64a476ffa90e369541340fddd2c0e","a2467850a65255909149ac519149daa4" 
            string[] apiKeys = new[] {  "c1e3477d2b7f99856e3b030010e7c9dc", "44db2db732d4f475404ee6a9ea7e7dc8", "14e4c3979bc1d9850fe9488d0b97ce55",//吴泓霖 
                "74d62219c9bb01ee87907c231a9d82ac","0fad37385b91d721dbeb9d13bdc3fa6e","a13ef4f06db315d878fc0bfd5aed5594",//pzx
                "09113358339aabf23fc6f31177654b86", "ce0320168fb47e75555980d48eea5d65", "f6a84ff59ce88262f6ea9484a30c0160",//ly
                "167c9c7782ce3e2108642ccb770a1630","1c036a92bf5125756bdf09ee4bdbb78f","129d2339c8103d6db4ab88994f8d4a57",//syc
                "be29d5228cc82cc0bb2fc6580a4cbb13","db209a2a865ae87804ed0b6785c80cd0","49cd85419df5c47782fcaacda7645036",//lmz
                 "a057327c53c4f8b45df4aa080772f1c6","fd3c78a0ce83b5cab630db35e6b8407c","50990c0ea6ebb3bd64e3b77eb4ab7e0d",//xyy
                 "937de1931ea4c2c741ff4315cb0eaf72","e226d0a18f47ef9455fc69b7b5a42b6e","eaf452b70c43678d7e7cf559306c6834",//lty
                 "79d94304c2ee63effd8f35c2ec918b7a","19df7a012b1e29d609d16fa92f789771","3ae027c5cf5a06a6b4b73b2e4af51cb8",//未知1
                 "5caf6d7ff0e3ec5a47835ac5bbb57573","d0d64a476ffa90e369541340fddd2c0e","a2467850a65255909149ac519149daa4",//未知2
                 "df40ff62cc870faf8fa961e3e62be080","51441f78937e0062dfda1fc419441e24","cac452fa5d34753cfb5a9d9cef06e09f",//hmy
                 "ed5cd9892d998bb9646dc3fbc908f621","31b09d99451ac78a58ee81ae8e580fd8","f2d216356df69cf6894ccfeea9254f45",//xjh
                 "8de4518f1d517be53a2329cd268786c3","379591b835c8ca11e51b563909ed1489","69b87b632176273eba719f24b45c52a7",//lbr
                 "e47b5804f6741191c0db5c5a74d7bb74","22a04459734bdb3d1d49361e08ff9aed","edc9a0afe93a825dc5fd62c2dcc3d698",//lyx
                  "a4677993bf383d4a18e8a041587d1d82","9b7f9af85987e5062431326976a7a1cb","9ead2fdd3cbd6da15f894eae63a29aea",//hyc
            };
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
                        HttpResponseMessage response = await _httpClientFactory.CreateClient().GetAsync(requestUrl);
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

              Console.WriteLine("已获取的路况数据:");
                foreach (var detail in allRoads)
                {
                    Console.WriteLine($"名字: {detail.name}");
                }

            // 将区域的完整道路数据一整条保存到 LeanCloud 中
            if (allRoads.Any())
            {
                await LeanCloudService.InsertRoadTrafficDataToLeanCloud(allRoads);  // 确保是一整条数据保存
            }

            // 存储结果到 LeanCloud
            if (districtTrafficIndexes.Count > 0)
            {
                await LeanCloudService.InsertDistrictTrafficIndexesToLeanCloud(districtTrafficIndexes);
            }

            _logger.LogInformation("Traffic data task executed successfully.");
        }

        // 计算拥堵指数
        private double CalculateCongestionIndex(JsonElement evaluation)
        {   
            Console.WriteLine($"{evaluation.ToString()}");
            //double status = GetNumericValue(evaluation, "status");
            double status = GetStatusAsDouble(evaluation);
            if (status < 0)
                return -1;
            double congested = GetPercentage(evaluation, "congested");
            double blocked = GetPercentage(evaluation, "blocked");

            // 拥堵系数计算公式：
            // CI = (status * 2) + (congested * 0.5) + (blocked * 0.8)
            double congestionIndex = (status * 1) + (congested * 2.5) + (blocked * 5);


            return congestionIndex;
        }

        private double GetPercentage(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var propertyValue))
            {
                string valueString = propertyValue.GetString()?.TrimEnd('%');
                if (double.TryParse(valueString, out double value))
                {
                    return value / 100; // 转换为小数形式
                }
            }
            return 0;
        }

        public static double GetStatusAsDouble(JsonElement evaluation)
        {
            // 检查是否存在"status"字段
            if (evaluation.TryGetProperty("status", out JsonElement statusElement))
            {
                switch (statusElement.ValueKind)
                {
                    case JsonValueKind.String:
                        // 尝试解析为字符串
                        if (double.TryParse(statusElement.GetString(), out double statusFromString))
                        {
                            return statusFromString;
                        }
                        break;

                    case JsonValueKind.Number:
                        // 如果是数值类型，直接获取
                        return statusElement.GetDouble();

                    case JsonValueKind.Array:
                        // 如果是数组，直接返回 -1
                        if (statusElement.GetArrayLength() == 0)
                        {
                            return -1;
                        }
                        break;
                }

                // 如果无法解析，抛出异常
                throw new FormatException("The status value is not in a valid format.");
            }
            else
            {
                throw new KeyNotFoundException("The status field is not found in the provided JsonElement.");
            }
        }
    }
}
