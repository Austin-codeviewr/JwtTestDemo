using System.Collections.Concurrent;
using System.Reflection.Metadata;
using Websocket.Client;

namespace JwtDemo.WebSocket;

public class WebsocketClientFactory 
{

    private readonly ILogger<WebsocketClientFactory> _logger;
    private static ConcurrentDictionary<string,System.Net.WebSockets.WebSocket> _clients = new ConcurrentDictionary<string,System.Net.WebSockets.WebSocket>();

    public WebsocketClientFactory(ILogger<WebsocketClientFactory> logger)
    {
        this._logger = logger;
    }
    
    public static void Add(String name,System.Net.WebSockets.WebSocket client)
    {
        _clients.TryAdd(name,client);
    }
 
    public static void Remove(string name)
    {
        _clients.Remove(name,out var _);
    }
 
    public static System.Net.WebSockets.WebSocket Get(string name,string clientId)
    {
        return _clients.FirstOrDefault(_ => _.Key.Equals(name)).Value;
    }
 
    public static IEnumerable<System.Net.WebSockets.WebSocket> GetAll()
    {
        return _clients.Values;
    }
    

}