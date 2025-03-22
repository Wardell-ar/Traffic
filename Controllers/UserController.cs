using Microsoft.AspNetCore.Mvc;
using TrafficDataApi.Models;
using TrafficDataApi.Services;
namespace TrafficDataApi.Controllers;


[ApiController]
[Route("api/[controller]")]
// 注册用户控制器
public class UserController : ControllerBase
{
    // 用户注册
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        var result = await LeanCloudService.RegisterUserAsync(user);
        if (result.Contains("成功"))
            return Ok(result);
        return BadRequest(result);
    }

    // 用户登录
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await LeanCloudService.LoginUserAsync(loginRequest.Username, loginRequest.Password);
        if (result.Contains("成功"))
            return Ok(result);
        return Unauthorized(result);
    }
}
