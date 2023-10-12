using System.Security.Claims;
using System.Text;
using JwtDemo.config;
using JwtDemo.Quartz;
using JwtDemo.service;
using JwtDemo.WebSocket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using WebSocketOptions = Microsoft.AspNetCore.Builder.WebSocketOptions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSwaggerGen(u =>
{
    u.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "Ver:1.0.0", //版本
        Title = "xxx后台管理系统", //标题
        Description = "xxx后台管理系统：包括人员信息、团队管理等。", //描述
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "西瓜程序猿",
            Email = "xxx@qq.com"
        }
    });
});
builder.Services.AddControllers();
builder.Services.Configure<JWTTokenOptions>(builder.Configuration.GetSection("JWTTokenOptions"));

JWTTokenOptions tokenOptions = new JWTTokenOptions();//初始化
builder.Configuration.Bind("JWTTokenOptions", tokenOptions);//实现调用

builder.Services.AddTransient<IJWTService, HSJWTService>();
//鉴权
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //JWT有一些默认的属性，就是给鉴权时就可以筛选了
            ValidateIssuer = true,//是否验证Issuer
            ValidateAudience = true,//是否验证Audience
            ValidateLifetime = false,//是否雅正失效时间
            ValidateIssuerSigningKey = true,//是否验证SecurityKey
            ValidAudience = tokenOptions.Audience,
            ValidIssuer = tokenOptions.Isuser,//Issuer,这两项和前面签发jwt的设置一致
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))
        };
    });

builder.Services.AddAuthorization();




var app = builder.Build();
//启用Swagger中间件
app.UseSwagger();
//配置SwaggerUI
app.UseSwaggerUI(u =>
{
    u.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI_v1");
});
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseMiddleware<WebsocketHandlerMiddleware>();

//中间件插入请求之前或者之后
/*app.Run(async context =>
{
    await context.Response.WriteAsync("Hello");
});*/

//中间件传递:执行完这个中间件后传递给后面的中间件
/*app.Use(async (context, next) =>
{
    await context.Response.WriteAsync("Hello");
    await next();
});*/

app.UseRouting();
//app.UseHttpsRedirection();
//鉴权
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(60),
    ReceiveBufferSize = 1 * 1024
});



//实例化调度器
IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

//开启调度器
scheduler.Start();

//创建一个作业
IJobDetail job1 = JobBuilder.Create<MyJob>()
    .WithIdentity("job1", "groupa")//名称，分组
    .Build();

//创建一个触发器
ITrigger trigger1 = TriggerBuilder.Create()
    .WithIdentity("trigger1", "groupa")//名称，分组
    .StartNow()//从启动的时候开始执行
    .WithSimpleSchedule(b =>
    {
        b.WithIntervalInSeconds(2)//2秒执行一次
            .WithRepeatCount(3);//重复执行3+1次
    })
    .Build();

//把作业，触发器加入调度器
scheduler.ScheduleJob(job1, trigger1);

app.Run();