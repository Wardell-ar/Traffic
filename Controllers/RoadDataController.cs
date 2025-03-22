using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficDataApi.Services;

namespace TrafficDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoadDataController : ControllerBase
    {
        private const string AppId = "Your_LeanCloud_AppId";
        private const string AppKey = "Your_LeanCloud_AppKey";
        private const string LeanCloudApiUrl = "Your_LeanCloud_API_Url";

        // 获取按 status 降序、speed 升序排列的道路数据
        [HttpGet("GetRoads")]
        public async Task<IActionResult> GetSortedRoadData(int limit = 50)
        {
            var roads = await LeanCloudService.GetSortedRoadData(limit);

            if (roads == null || roads.Count == 0)
            {
                return NotFound("没有找到相关道路数据");
            }

            return Ok(roads);
        }
    }
}
