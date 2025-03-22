
using Microsoft.AspNetCore.Mvc;


namespace TrafficDataApi.Models
{

  public class Trafficroads{
    public string? name{ get; set; }
    public string? status { get; set; }
    public string? direction { get; set; }
    public string? angle { get; set; }
    public string? lcodes { get; set; }
    public string? speed { get; set; }
    public string? polyline { get; set; }
}
  public class TrafficEvaluation
    {
        public string? Expedite { get; set; }
        public string? Congested { get; set; }
        public string? Blocked { get; set; }
        public string? Unknown { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
    }

    public class TrafficInfo
    {
        public string? Description { get; set; }
        public TrafficEvaluation? Evaluation { get; set; }
         public Trafficroads? Roads { get; set; }
    }

    public class TrafficResponse
    {
        public string? Status { get; set; }
        public string? Info { get; set; }
        public string? Infocode { get; set; }
        public TrafficInfo? Trafficinfo { get; set; }
    }

        public class RoadTrafficData
    {
        public string RoadName { get; set; }
        public string Direction { get; set; }
        public double CongestionLevel { get; set; }  // 拥堵水平，例如 0-10
        public string RoadStatus { get; set; }  // 状态：例如"正常"、"拥堵"等
        public string Description { get; set; }  // 交通状况描述
        public string Evaluation { get; set; }  // 评价
        public DateTime CreatedAt { get; set; }
    }

    
        // 辅助扩展方法，将 double[][] 转换为 double[,]
    public static class ArrayExtensions
    {
        public static T[,] To2DArray<T>(this T[][] jaggedArray)
        {
            var length1 = jaggedArray.Length;
            var length2 = jaggedArray[0].Length;
            var result = new T[length1, length2];

            for (int i = 0; i < length1; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    result[i, j] = jaggedArray[i][j];
                }
            }

            return result;
        }
    }
    

// 返回数据的模型
    public class DistrictTrafficIndex
    {
        public string DistrictName { get; set; }
        public double TrafficIndex { get; set; }
    }

    public class DistrictTrafficIndexResponse
    {
        public List<DistrictTrafficIndex> DistrictTrafficIndexes { get; set; }
        public string CreatedAt { get; set; }
    }

    // 道路数据模型
    public class Road
    {
        public string Name { get; set; }
        public int Status { get; set; }
        public string Direction { get; set; }
        public int Speed { get; set; }

    }
    public class RoadDetail
    {

        public int Status { get; set; }
        public double Speed { get; set; }
        public double CongestionIndex { get; set; }
        public string CreatedAt { get; set; }
    }
}

  