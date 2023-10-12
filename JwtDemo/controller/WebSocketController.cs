using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemo.controller;


[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class WebSocketController : ControllerBase
{
    [HttpGet]
    public async void Connect(WebSocket.WebSocketOptions options)
    {
        using (var ws = new ClientWebSocket())
        {
            await ws.ConnectAsync(options.Uri, CancellationToken.None);
            var buffer = new byte[256];
            
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                }
                else
                {
                    Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, result.Count));
                }
            }
        }
    }
}