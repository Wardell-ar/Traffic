using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoadSearchController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public RoadSearchController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("SearchShanghaiRoads")]
        public async Task<IActionResult> SearchShanghaiRoads([FromQuery] string keywords = "道路", [FromQuery] int offset = 20, [FromQuery] int page = 1)
        {
            // 高德API地址
            string url = "https://restapi.amap.com/v3/place/text";

            // 构造请求参数
            var queryParams = new[]
            {
                $"key={"c1e3477d2b7f99856e3b030010e7c9dc"}",
                $"keywords={keywords}",
                $"types={"道路名"}",
                $"city={"上海"}",
                $"citylimit={true}",
                $"offset={offset}",
                $"page={page}",
                $"extensions=base"
            };
            string requestUrl = $"{url}?{string.Join("&", queryParams)}";

            try
            {
                // 发送 GET 请求
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                // 解析响应内容
                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseBody);  // 或者使用日志系统记录

                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

                // 提取 pois 部分
                if (result.TryGetProperty("pois", out var pois))
                {
                    return Ok(pois);
                }
                else
                {
                    return BadRequest(new { status = 0, info = "POIs not found in response" });
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
