using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TrafficDataApi.Services;
using TrafficDataApi.Models;
using static LeanCloud.Realtime.AVRealtime;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetDataController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public GetDataController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("GetData")]
        public async Task<IActionResult> GetRoadMapData()
        {
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
                //{ "奉贤", new List<string> { "121.508178,30.925192", "121.5600,30.9000", "121.540433,30.922355" } },
                //{ "崇明", new List<string> { "121.427031,31.626536", "121.662207,31.5856", "121.841421,31.472934" } },
                // 可根据实际需求添加更多区和地点
            };

            string[] apiKeys = new[] {  "c1e3477d2b7f99856e3b030010e7c9dc", "44db2db732d4f475404ee6a9ea7e7dc8", 
               "14e4c3979bc1d9850fe9488d0b97ce55", "4e0688086ea33eca825b2eab1ae50e0e", "6a89009e649c9cc09a64a198e0e0b5ef",
                "74d62219c9bb01ee87907c231a9d82ac","0fad37385b91d721dbeb9d13bdc3fa6e","a13ef4f06db315d878fc0bfd5aed5594","bd425a3d901606060abac7d8ae218040","1c504818545bdc963f325aae27c1f75d",
                "09113358339aabf23fc6f31177654b86", "ce0320168fb47e75555980d48eea5d65", "f6a84ff59ce88262f6ea9484a30c0160",
                "167c9c7782ce3e2108642ccb770a1630","1c036a92bf5125756bdf09ee4bdbb78f","129d2339c8103d6db4ab88994f8d4a57","af7da54013758624d74b7be9c3cee2d9","c1b53da873d1ee7ee29d2ac1044b9c27",
                "be29d5228cc82cc0bb2fc6580a4cbb13","db209a2a865ae87804ed0b6785c80cd0","49cd85419df5c47782fcaacda7645036","974a440ba711e5d93b431796e7fc0476","8f2d503538f5403070c6339f17a3f16b"};
            int currentKeyIndex = 0;

            string GetApiKey()
            {
                var key = apiKeys[currentKeyIndex];
                currentKeyIndex = (currentKeyIndex + 1) % apiKeys.Length;
                return key;
            }

            string url = "https://restapi.amap.com/v3/traffic/status/circle";
            var districtTrafficIndexes = new Dictionary<string, object>();

            // 全局去重的 HashSet，用来存储所有区域所有经纬度查询到的唯一道路
            var globalUniqueRoads = new HashSet<string>(); // 用于全局去重（基于道路名称和方向）
            var allRoads = new List<Trafficroads>();

            foreach (var district in districtLocations)
            {
                double totalIndex = 0;
                int count = 0;
   
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

                        //Console.WriteLine(result.ToString());

                        if (result.TryGetProperty("trafficinfo", out var trafficInfo))
                        {
                            if (trafficInfo.TryGetProperty("evaluation", out var evaluation))
                            {

                                // 解析并计算拥堵系数
                                double congestionIndex = CalculateCongestionIndex(evaluation);
                                if (congestionIndex > 0)
                                {
                                    totalIndex += congestionIndex;
                                    count++;
                                }
                            }

                            if (trafficInfo.TryGetProperty("roads", out var roads) && roads.ValueKind == JsonValueKind.Array)
                            {
                                ProcessRoads(roads, globalUniqueRoads, allRoads);
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"Error fetching data for location {location}: {ex.Message}");
                    }
                    await Task.Delay(125);
                }

                // 计算平均拥堵系数
                if (count > 0)
                {
                    districtTrafficIndexes[district.Key] = Math.Round(totalIndex / count, 2);
                }
                else
                {
                    districtTrafficIndexes[district.Key] = 0; // 若无有效数据，设为0
                }
            }
            if (allRoads.Any())
            {
            // 插入去重后的道路交通数据到 LeanCloud
                await LeanCloudService.InsertRoadTrafficDataToLeanCloud(allRoads);
            }
            // 将区域交通指数数据插入到 LeanCloud
            await LeanCloudService.InsertDistrictTrafficIndexesToLeanCloud(districtTrafficIndexes);

            var resultData = districtTrafficIndexes.Select(d => new { name = d.Key, d.Value }).ToList();

            return Ok(resultData);
        }
        private void ProcessRoads(JsonElement roads, HashSet<string> globalUniqueRoads, List<Trafficroads> allRoads)
        {
            foreach (var road in roads.EnumerateArray())
            {
                // 提取道路相关属性
                if (road.TryGetProperty("name", out var name) &&
                    road.TryGetProperty("direction", out var direction))
                {
                    // 使用道路的名称和方向作为唯一标识
                    var roadKey = $"{name.GetString()}-{direction.GetString()}";

                    // 全局去重：如果该道路在全局集合中已经存在，则不再添加
                    if (globalUniqueRoads.Add(roadKey)) // 全局去重
                    {
                        // 将去重后的道路数据加入到结果中
                        var fullRoadData = new Trafficroads
                        {
                            name = name.GetString(),
                            direction = direction.GetString(),
                            status = road.TryGetProperty("status", out var status) ? status.GetString() : (string?)null,
                            speed = road.TryGetProperty("speed", out var speed) ? speed.GetString() : (string?)null,
                            lcodes = road.TryGetProperty("status", out var lcodes) ? lcodes.GetString() : (string?)null,
                        };
                        allRoads.Add(fullRoadData); // 保存去重后的道路数据
                    }
                }
            }
        }
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
