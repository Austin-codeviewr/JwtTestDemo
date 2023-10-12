using JwtDemo.service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemo.controller;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class JwtController : ControllerBase
{
    private readonly IJWTService _IJWTService;

    public JwtController(IJWTService ijwtService)
    {
        _IJWTService = ijwtService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetToken(string name, string password)
    {
        Console.WriteLine($"{name}--{password}");
        if ("zhanglei".Equals(name) & "123456".Equals(password))
        {
            //生成JWT
            string token = _IJWTService.GetToken(name);
            return new JsonResult(new
            {
                Result = true,
                Token = token,
            });
        }
        else
        {
            return new JsonResult(new
            {
                Result = true,
                Token = "不匹配"
            });
        }
 
        //return null;
    }
 
 
    [HttpGet]
    public string a()
    {
        return "a";
    }
    
}