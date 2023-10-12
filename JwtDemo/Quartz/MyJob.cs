using Quartz;

namespace JwtDemo.Quartz;

public class MyJob : IJob
{
    private string Url { get; set; }
    private string Corn { get; set; }
    
    public async Task Execute(IJobExecutionContext context)
    {
        await Task.Run(() =>
        {
            String url = "WWW.baidu.com";
            /*using HttpClient client = new HttpClient();
            var result = client.GetAsync(url).GetAwaiter().GetResult();*/
            
            Console.WriteLine("Start Quartz.NET");
            return Task.CompletedTask;
        });
    }
}