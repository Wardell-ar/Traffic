
using Microsoft.AspNetCore.Mvc;


namespace TrafficDataApi.Models
{
    // 返回的响应数据模型
    public class RoadPredictionResponse
    {
        public List<RoadDetail> PastData { get; set; }
        public List<RoadDetail> FuturePredictions { get; set; }
    }
}