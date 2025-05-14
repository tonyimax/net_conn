namespace net_conn;
using System.Net;
using System.Net.WebSockets;
using System.Text;

public class WebSocketServer
{
    private HttpListener _listener = new HttpListener();
    private const string PREFIX = "http://192.168.119.130:5000/";
 
    public WebSocketServer()
    {
        _listener.Prefixes.Add(PREFIX);
    }
 
    public void StartListening()
    {
        _listener.Start();
        Console.WriteLine("Listening on " + PREFIX);
        Task.Run(() => ListenContinuouslyAsync());
    }
 
    private async Task ListenContinuouslyAsync()
    {
        try
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    await HandleWebSocketAsync(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Server exception: " + ex);
        }
        finally
        {
            _listener.Stop();
        }
    }
 
    private async Task HandleWebSocketAsync(HttpListenerContext context)
    {
        var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
        var webSocket = webSocketContext.WebSocket;
        byte[] buffer = new byte[1024];
        ArraySegment<byte> arraySegment = new ArraySegment<byte>(buffer);
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(arraySegment, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                Console.WriteLine("Received: " + Encoding.UTF8.GetString(buffer, 0, result.Count));
                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None); // Echo back the message
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                break; 
            }
        }
        Console.WriteLine("Connection closed");
    }
}