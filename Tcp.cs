using System.Net;
using System.Net.Sockets;
using System.Text;

namespace net_conn;

public class Tcp
{
    private String _host="127.0.0.1";
    private int _port=8000;
    private TcpClient c;
    private TcpListener l;
    private NetworkStream? _tcpStream=null;

    public delegate void ListenSuccess();

    public delegate void UpdateGridRowData(string ip,string port,string aparea);

    private ListenSuccess _callback;
    private UpdateGridRowData _callback_update_row;

    public NetworkStream? TcpStream
    {
        get => _tcpStream;
        set => _tcpStream = value ?? throw new ArgumentNullException(nameof(value));
    }

    private string Host
    {
        get => _host;
        set => _host = value ?? throw new ArgumentNullException(nameof(value));
    }

    private int Port
    {
        get => _port;
        set => _port = value;
    }

    public void NotifyListenSuccess(ListenSuccess success) 
    {
        _callback=success;
    }

    public void NotifyUpdateRow(UpdateGridRowData callback) 
    {
        _callback_update_row = callback;
    }

    public void Listen(String host ,int port)
    {
        TcpListener server = null;
        try
        {
            IPAddress localAddr = IPAddress.Parse(host);
            server = new TcpListener(localAddr,port);
            server.Start();
            Console.WriteLine($"作者:林宏权 博客: https://blog.csdn.net/fittec?type=blog  QQ:296863766");
            Console.WriteLine($"TCP服务已启动监听端口:[{port}]");
            if (null != _callback){
                _callback();
            }
            Byte[] bytes = new Byte[256];
            String data = null;
            while(true)
            {
                Console.Write("等待远程主机连接... ");
                using TcpClient client = server.AcceptTcpClient();
                client.SendTimeout = 5000;
                client.ReceiveTimeout = 5000;
                Console.WriteLine($"远程主机[{client.Client.RemoteEndPoint}]已连接");
                
                var http = new Http();
                var rip = client.Client.RemoteEndPoint.ToString().Split(":")[0];
                var rport = client.Client.RemoteEndPoint.ToString().Split(":")[1];
                var result =  http.CheckIp(rip);
                result.Wait();
                if (http.IsFromChina || rip == "10.1.8.8" || rip == "127.0.0.1")
                {
                    if (null != _callback_update_row) 
                    {
                        _callback_update_row(rip, rport, "中国");
                    }
                    data = null;
                    NetworkStream stream = client.GetStream();
                    int i;
                    try
                    {
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("<===[{0}]接收[{2}]: {1}", DateTime.Now, data, rip);
                            data = data.ToUpper();
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                            stream.Write(msg, 0, msg.Length);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("===>[{0}]发送[{2}]: {1}", DateTime.Now, data, rip);
                        }
                    }
                    catch (Exception e) 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                    }
                    
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("<===[{0}]非中国地区IP[{2}]不回应: {1}", DateTime.Now,data,rip);
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                
            }
        }
        catch(SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }

    public void Send(String data)
    {
        if (null!=TcpStream && TcpStream.CanWrite)
        {
            try
            {
                TcpStream.Write(Encoding.Default.GetBytes(data));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public int ReceiveOneByte()
    {
        int n = 0;
        byte[] buf = new byte[1];
        if (TcpStream.CanRead)
        {
            n = TcpStream.Read(buf,0,buf.Length);
        }
        return n;
    }

    public String Receive()
    {
        try
        {
            byte[] buf = new byte[1024];
            if (TcpStream.CanRead)
            {
                int n = TcpStream.Read(buf);
                if (n > 0)
                {
                    return Encoding.Default.GetString(buf);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return String.Empty;
    }

    public void Close()
    {
        if (TcpStream.CanRead||TcpStream.CanWrite)
        {
            TcpStream.Close();
        }
        if (c.Connected)
        {
            c.Close();
        }
    }

    public void Connect(String h, int p)
    {
        if (String.IsNullOrEmpty(h) || p == 0)
        {
            Console.WriteLine("===>Invalid host or port number");
            return ;
        }
        Host = h;
        Port = p;
        Console.WriteLine($"===>Connected to {Host}:{Port} ...");
        c = new TcpClient();
        try
        {
            c.Connect(h,p);
            if (c.Connected)
            { 
                TcpStream = c.GetStream();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            c.Dispose();
        }
    }
}