

using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Websocket.Client;

namespace JwtDemo.WebSocket;

public class WebsocketHandlerMiddleware : IMiddleware
{
    private const string WebSocketName = $"Socket";

    private readonly Semaphore XSemaphore=new Semaphore(1, 1);

    private readonly ILogger<WebsocketHandlerMiddleware> logger;

    public WebsocketHandlerMiddleware(ILogger<WebsocketHandlerMiddleware> logger)
    {
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        
        if (context.Request.Path == "/ws")
        {
            //仅当网页执行new WebSocket("ws://localhost:5000/ws")时，后台会执行此逻辑
            if (context.WebSockets.IsWebSocketRequest)
            {
                XSemaphore.WaitOne();
                //后台成功接收到连接请求并建立连接后，前台的webSocket.onopen = function (event){}才执行
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                try
                {
                    await Handle(webSocket);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Echo websocket client err .");
                    await context.Response.WriteAsync("closed");
                }
                finally
                {
                    XSemaphore.Release();
                }
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }
        else
        {
            await next(context);
        }
    }
    
    private async Task Handle(System.Net.WebSockets.WebSocket webSocket)
    {
        WebsocketClientFactory.Add(WebSocketName,webSocket);
        logger.LogInformation($"Websocket client added.");
        
        WebSocketReceiveResult clientData = null;
        do
        {
            var buffer = new byte[1024 * 1];
            //客户端与服务器成功建立连接后，服务器会循环异步接收客户端发送的消息，收到消息后就会执行Handle(WebsocketClient websocketClient)中的do{}while;直到客户端断开连接
            //不同的客户端向服务器发送消息后台执行do{}while;时，websocketClient实参是不同的，它与客户端一一对应
            //同一个客户端向服务器多次发送消息后台执行do{}while;时，websocketClient实参是相同的
            clientData = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (clientData.MessageType == WebSocketMessageType.Text && !clientData.CloseStatus.HasValue)
            {
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely, result.CloseStatusDescription);
                }
                var text = Encoding.UTF8.GetString(buffer.AsSpan(0, result.Count));
                var sendStr = Encoding.UTF8.GetBytes($"服务端  : {text}  -{DateTime.Now}");
                await webSocket.SendAsync(sendStr, WebSocketMessageType.Text, true, CancellationToken.None);
                
            }
        } while (!clientData.CloseStatus.HasValue);
        //关掉使用WebSocket连接的网页/调用webSocket.close()后，与之对应的后台会跳出循环
        WebsocketClientFactory.Remove(WebSocketName);
        logger.LogInformation($"Websocket client closed.");
    }
    
}