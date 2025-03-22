using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrafficDataApi.Models;
using TrafficDataApi.Services;
using System.Collections.Generic;

namespace TrafficDataApi.Controllers
{
   [Route("api/[controller]")]
    [ApiController]
    public class RoadTrafficPredictionController : ControllerBase
    {
        // 获取路段的历史数据并预测未来7天的速度
        [HttpGet("GetPredictedCongestionIndex")]
        public async Task<IActionResult> PredictRoadCongestionIndex([FromQuery] string name, [FromQuery] string direction)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(direction))
            {
                return BadRequest("路名和方向不能为空");
            }

            // 1. 获取过去七天的道路数据
            var pastData = await LeanCloudService.GetRoadDetails(name, direction, 144);

            if (pastData == null || pastData.Count == 0)
            {
                return NotFound("没有找到该路段的历史数据");
            }

            // 2. 使用多项式回归算法进行速度预测
            var predictions = LeanCloudService.PredictFutureSpeed(pastData);

            // 3. 将历史数据与预测数据一起返回
            var response = new RoadPredictionResponse
            {
                PastData = pastData,
                FuturePredictions = predictions
            };

            return Ok(response);
        }
    }
}