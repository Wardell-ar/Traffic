using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrafficInfoController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public TrafficInfoController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("GetTrafficInfo")]
        public async Task<IActionResult> GetTrafficInfo([FromQuery] int level, [FromQuery] string location, [FromQuery] int radius = 1000)
        {
            // 高德API地址
            string url = "https://restapi.amap.com/v3/traffic/status/circle";

            // 构造请求参数
            var queryParams = new[]
            {
            $"key={"46a22c068333e8bdcc4fa1fb063b8786"}",
            $"level={level}",
            $"location={location}",
            $"radius={radius}",
            $"extensions=base"         // 扩展信息，返回所有相关的流量信息
        };
            string requestUrl = $"{url}?{string.Join("&", queryParams)}";

            try
            {
                // 发送 GET 请求
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                // 解析响应内容
                string responseBody = await response.Content.ReadAsStringAsync();

                //Console.WriteLine(responseBody);  // 或者使用日志系统记录

                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

                // 提取 trafficinfo 部分
                if (result.TryGetProperty("trafficinfo", out var trafficInfo))
                {
                    return Ok(trafficInfo);
                }
                else
                {
                    return BadRequest(new { status = 0, info = "Traffic info not found in response" });
                }
            }
            catch (HttpRequestException ex)
            {
                // 捕获请求异常并返回错误信息
                return StatusCode(500, new
                {
                    status = 0,
                    info = "Request failed",
                    error = ex.Message
                });
            }
        }
    }
}
