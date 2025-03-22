using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using TrafficDataApi.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();//
// builder.Services.AddHostedService<TrafficDataService>();
//builder.Services.AddHostedService<TrafficDataBackgroundService>(); // 注册后台任务服务
// 禁用 HTTPS 重定向
builder.Services.AddControllersWithViews();
// 添加 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()   // 允许所有来源
               .AllowAnyMethod()   // 允许所有 HTTP 方法
               .AllowAnyHeader();  // 允许所有请求头
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();  // 仅在生产环境中启用 HTTPS 重定向
}
app.UseOptionsRequest();
// 配置请求管道
app.UseRouting();
// 确保启用 CORS 中间件，必须在 UseRouting 和 UseAuthorization 之间
app.UseCors("AllowAllOrigins");
app.UseAuthorization();
app.MapControllers();
app.Run();