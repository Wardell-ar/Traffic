using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public class OptionsRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public OptionsRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method.ToUpper() == "OPTIONS")
            {
                // 设置CORS头部
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                context.Response.Headers.Add("Access-Control-Max-Age", "3600"); // 缓存预检请求1小时

                // 设置状态码为200
                context.Response.StatusCode = 200;

                // 确保不写入内容体
                await context.Response.WriteAsync(""); 

                return; // 直接返回响应，跳过后续中间件
            }

            // 如果不是OPTIONS请求，继续传递请求
            await _next.Invoke(context);
        }
    }

    /// <summary>
    /// 扩展中间件
    /// </summary>
    public static class OptionsRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseOptionsRequest(this IApplicationBuilder app)
        {
            return app.UseMiddleware<OptionsRequestMiddleware>();
        }
    }
}
