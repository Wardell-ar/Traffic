using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficDataApi.Services;

namespace TrafficDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoadDetailController : ControllerBase
    {
        private const string AppId = "Your_LeanCloud_AppId";
        private const string AppKey = "Your_LeanCloud_AppKey";
        private const string LeanCloudApiUrl = "Your_LeanCloud_API_Url";

        // 获取指定道路的详细信息
        [HttpGet("GetRoadDetails")]
        public async Task<IActionResult> GetRoadDetails(string roadName, string direction, int limit = 24)
        {
            var roadDetails = await LeanCloudService.GetRoadDetails(roadName, direction, limit);

            if (roadDetails == null || roadDetails.Count == 0)
            {
                return NotFound("没有找到相关道路信息");
            }

            return Ok(roadDetails);
        }
    }
}
