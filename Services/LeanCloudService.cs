// Services/LeanCloudService.cs
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TrafficDataApi.Models;
using System;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace TrafficDataApi.Services
{

    public static class LeanCloudService
    {
        private const string AppId = "ybBc9PfAWgYTZXYlMUHy40SM-gzGzoHsz";
        private const string AppKey = "EScVA1jLLBDUQITg4NhYP8YM";
        private const string LeanCloudApiUrl = "https://api.leancloud.cn/1.1/classes/heatmap";
        private const string RoadTrafficApiUrl = "https://api.leancloud.cn/1.1/classes/roadtrafficDataList";
        public static async Task<string> RegisterUserAsync(User user)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", AppKey);

            var userData = new
            {
                username = user.Username,
                password = user.Password,
                phone = user.Phone,
                createdAt = new
                {
                    __type = "Date",
                    iso = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }
            };

            var jsonContent = JsonConvert.SerializeObject(userData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api.leancloud.cn/1.1/classes/User", content);
                if (response.IsSuccessStatusCode)
                {
                    return "注册成功";
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return $"注册失败: {response.StatusCode} - {response.ReasonPhrase}, 错误详情: {errorResponse}";
                }
            }
            catch (Exception ex)
            {
                return $"注册失败: {ex.Message}";
            }
        }

        public static async Task<string> LoginUserAsync(string username, string password)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", AppKey);

            var queryUrl = $"https://api.leancloud.cn/1.1/classes/User?where={{\"username\":\"{username}\",\"password\":\"{password}\"}}";

            try
            {
                var response = await client.GetStringAsync(queryUrl);
                var data = JsonConvert.DeserializeObject<dynamic>(response);

                if (data.results.Count > 0)
                    return "登录成功";
                else
                    return "用户名或密码错误";
            }
            catch (Exception ex)
            {
                return $"登录失败: {ex.Message}";
            }
        }


        // 新增：存储道路信息
        public static async Task InsertRoadTrafficDataToLeanCloud(List<Trafficroads> roadTrafficData)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", AppKey);
            // 获取上海本地时间
            var shanghaiTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "China Standard Time");
            // 将道路交通数据转换为符合LeanCloud要求的格式
            var trafficData = new
            {
                roads = roadTrafficData.Select(road => new
                {
                    name = road.name,
                    status = road.status,
                    direction = road.direction,
                    angle = road.angle,
                    lcodes = road.lcodes,
                    speed = road.speed,
                    polyline = road.polyline
                }).ToList(),
                createdAt = new
                {
                    __type = "Date",
                    iso = shanghaiTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)
                }
            };

            // 将数据转换为 JSON 格式
            var jsonContent = JsonConvert.SerializeObject(trafficData);

            // 输出转换后的 JSON 数据
            // Console.WriteLine("转换后的 JSON 数据:");
            // Console.WriteLine(jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // 发送 POST 请求到 LeanCloud
                var response = await client.PostAsync(RoadTrafficApiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("道路交通数据成功存入 LeanCloud");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"错误: {response.StatusCode} - {response.ReasonPhrase}");
                    Console.WriteLine($"详细错误信息: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插入道路交通数据失败: {ex.Message}");
            }
        }
        //插入每个区及其拥堵系数
        public static async Task InsertDistrictTrafficIndexesToLeanCloud(Dictionary<string, object> districtTrafficIndexes)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", AppKey);
            var shanghaiTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "China Standard Time");
            // 构造要存储的数据
            var trafficData = new
            {
                districtTrafficIndexes = districtTrafficIndexes.Select(district => new
                {
                    districtName = district.Key,
                    trafficIndex = district.Value
                }).ToList(),
                createdAt = new
                {
                    __type = "Date",
                    iso = shanghaiTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)
                }
            };

            // 将数据转换为 JSON 格式
            var jsonContent = JsonConvert.SerializeObject(trafficData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // 发送 POST 请求到 LeanCloud
                var response = await client.PostAsync(LeanCloudApiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("区域交通指数数据成功存入 LeanCloud");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"错误: {response.StatusCode} - {response.ReasonPhrase}");
                    Console.WriteLine($"详细错误信息: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插入区域交通指数数据失败: {ex.Message}");
            }
        }
        // 新增：从 LeanCloud 获取最新热力图数据
        public static async Task<DistrictTrafficIndexResponse> GetLatestDistrictTrafficData(int limit = 10)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", AppKey);

            // 构建查询 URL，按 createdAt 降序排序
            var queryUrl = $"{LeanCloudApiUrl}?limit={1}&order=-createdAt";

            try
            {
                var response = await client.GetStringAsync(queryUrl);
                var data = JsonConvert.DeserializeObject<dynamic>(response);

                // 获取最新的结果（最新的 `results`）
                var latestResult = data.results[0];

                // 从最新的 result 中提取 districtTrafficIndexes 和 createdAt
                var districtTrafficIndexes = new List<DistrictTrafficIndex>();
                foreach (var item in latestResult.districtTrafficIndexes)
                {
                    districtTrafficIndexes.Add(new DistrictTrafficIndex
                    {
                        DistrictName = item.districtName,
                        TrafficIndex = item.trafficIndex
                    });
                }

                return new DistrictTrafficIndexResponse
                {
                    DistrictTrafficIndexes = districtTrafficIndexes,
                    CreatedAt = latestResult.createdAt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取数据失败: {ex.Message}");
                return null;
            }
        }
        //获取最拥堵的n道路数据
        public static async Task<List<Road>> GetSortedRoadData(int limit = 50)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", AppKey);

            // 构建查询 URL
            var queryUrl = $"{RoadTrafficApiUrl}?limit={1}&order=-createdAt";

            try
            {
                var response = await client.GetStringAsync(queryUrl);
                var data = JsonConvert.DeserializeObject<dynamic>(response);
               
                // 从数据库中提取 roads 数据
                var roads = new List<Road>();
                foreach (var item in data.results[0].roads)
                {
                    // 获取 speed 并判断是否大于 0
                    int speed = item.speed != null ? int.Parse((string)item.speed) : -1;// 处理 null 为 -1

                    if (speed > 0) // 只添加 speed 大于 0 的道路
                    {
                        roads.Add(new Road
                        {
                            Name = item.name ?? "Unknown", // 如果 name 为空，使用默认值
                            Status = item.status != null ? int.Parse((string)item.status) : -1, // 处理 null 为 -1
                            Direction = item.direction ?? "Unknown", // 如果 direction 为空，使用默认值
                            Speed = speed
                        });
                    }
                }

                // 排序并限制条数
                return roads
                    .OrderByDescending(r => r.Status) // 按 status 降序
                    .ThenBy(r => r.Speed)            // 按 speed 升序
                    .Take(limit)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取数据失败: {ex.Message}");
                return null;
            }
        }
       //获取某条道路的历史数据
        public static async Task<List<RoadDetail>> GetRoadDetails(string roadName, string direction, int limit = 24)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", AppKey);

            // 构建查询 URL，限制结果数量
            var queryUrl = $"{RoadTrafficApiUrl}?limit={limit}&order=-createdAt";

            try
            {
                var response = await client.GetStringAsync(queryUrl);
                var data = JsonConvert.DeserializeObject<dynamic>(response);

                // 获取最新的 createdAt 日期
                var latestDate = DateTime.Parse((string)data.results[0].createdAt).Date;

                var roadDetails = new List<RoadDetail>();
                int maxSpeed = -1;

                // 筛选出今天的数据，并找到最大速度作为基准速度
                foreach (var result in data.results)
                {
                    var recordDate = DateTime.Parse((string)result.createdAt).Date;

                    // 确保数据是同一天
                    if (recordDate != latestDate) continue;

                    foreach (var item in result.roads)
                    {
                        var currentSpeed = item.speed != null ? int.Parse((string)item.speed) : -1;
                        // 匹配道路名称和方向
                        if ((string)item.name == roadName && (string)item.direction == direction)
                        {
                            // 更新今天的最大速度
                            if (currentSpeed > maxSpeed)
                            {
                                maxSpeed = currentSpeed;
                            }
                        }
                    }
                }

                // 遍历结果中的多个记录
                for (int i = 0; i <= limit && i < data.results.Count; i++)
                {
                    if (limit <= 24)
                    {
                        var recordDate = DateTime.Parse((string)data.results[i].createdAt).Date;

                        // 确保数据是同一天
                        if (recordDate != latestDate) continue;
                    }

                    foreach (var item in data.results[i].roads)
                    {
                        // 匹配道路名称和方向
                        if ((string)item.name == roadName && (string)item.direction == direction)
                        {
                            var status = item.status != null ? int.Parse((string)item.status) : -1;
                            var currentSpeed = item.speed != null ? int.Parse((string)item.speed) : -1;
                            var congestionIndex = 1.0;
                            if (maxSpeed > 0 && currentSpeed > 0 && status > 0)
                            {
                                var ratio = (double)maxSpeed / currentSpeed;
                                congestionIndex = ratio / 2 + (double)status / 2;
                            }
                                                      

                            roadDetails.Add(new RoadDetail
                            {
                                Status = status,                               
                                Speed = currentSpeed,
                                CongestionIndex = Math.Round(congestionIndex, 2),
                                CreatedAt = data.results[i].createdAt,
                            });
                        }
                    }
                }

                return roadDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取数据失败: {ex.Message}");
                return null;
            }
        }


            public static List<RoadDetail> PredictFutureSpeed(List<RoadDetail> realroadDetails)
            {
            // 如果数据为空，则不进行预测
            if (realroadDetails == null || realroadDetails.Count < 5)
            {
                Console.WriteLine("没有足够的数据进行预测");
                return new List<RoadDetail>();
            }

            // Parse CreatedAt to DateTime for processing
            DateTime baseDate = DateTime.Parse(realroadDetails.First().CreatedAt);
            baseDate = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, baseDate.Hour, 0, 0);
           

            var parsedData = realroadDetails
                .Select(data => new
                {
                    Hour = DateTime.Parse(data.CreatedAt).Hour,
                    data.CongestionIndex,
                    data.Speed
                })
                .ToList();

            // Group historical data by hour to calculate average congestion index and speed for each hour
            var groupedData = parsedData
                .GroupBy(data => data.Hour)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        AvgCongestionIndex = g.Average(data => data.CongestionIndex),
                    });
            // Print groupedData for debugging
            Console.WriteLine("Grouped Data:");
            foreach (var entry in groupedData)
            {
                Console.WriteLine($"Hour: {entry.Key}, AvgCongestionIndex: {entry.Value.AvgCongestionIndex:F2}");
            }
            // Smooth data with weighted moving average to emphasize peaks and troughs
            var smoothedData = new List<RoadDetail>();
            for (int offset = 0; offset < 24; offset++)
            {
                int currentHour = (baseDate.Hour + offset) % 24;
                if (!groupedData.ContainsKey(currentHour))
                {
                    // Skip if current hour data is missing
                    continue;
                }
                double prevCongestion = groupedData.ContainsKey((currentHour - 1 + 24) % 24) ? groupedData[(currentHour - 1 + 24) % 24].AvgCongestionIndex : 0;
                double currentCongestion = groupedData[currentHour].AvgCongestionIndex ;
                double nextCongestion = groupedData.ContainsKey((currentHour + 1) % 24) ? groupedData[(currentHour + 1) % 24].AvgCongestionIndex : 0;


                double totalWeight = 1.0;
                double weightedSum = currentCongestion;

                if (prevCongestion > 0)
                {
                    weightedSum += prevCongestion * 0.1;
                    totalWeight += 0.1;
                }


                if (nextCongestion > 0)
                {
                    weightedSum += nextCongestion * 0.1;
                    totalWeight += 0.1;
                }

                if (totalWeight > 0)
                {
                    double weightedCongestion = weightedSum / totalWeight;
                    smoothedData.Add(new RoadDetail
                    {
                        CreatedAt = baseDate.AddHours(offset).ToString("yyyy-MM-dd HH:mm:ss"),
                        CongestionIndex = Math.Round(weightedCongestion, 2),
                    });
                }
            }



            return smoothedData;
        }

       
    }
}

