using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficDataApi.Services;
using TrafficDataApi.Models;
using System.Threading.Tasks;

namespace TrafficDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeatMapController : ControllerBase
    {
        
        // 获取最新交通数据
        [HttpGet("GetHeatMap")]
        public async Task<IActionResult> GetLatestDistrictTrafficData(int limit = 10)
        {
            var response = await LeanCloudService.GetLatestDistrictTrafficData(limit);

            if (response == null || response.DistrictTrafficIndexes.Count == 0)
            {
                return NotFound("没有找到相关数据");
            }

            return Ok(response);
        }
    }
}
