using System.Net;
using System.Net.Sockets;
using System.Text;

namespace net_conn;

public class Tcp
{
    private String host;
    private int port;
    private TcpClient c;
    private TcpListener l;
    private NetworkStream s;
    public string Host
    {
        get => host;
        set => host = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int Port
    {
        get => port;
        set => port = value;
    }

    public void Listen(int p)
    {
        l = new TcpListener(IPAddress.Any, p);
        l.Start(1);
        byte[] buffer = new byte[1024];

        while (true)
        {
            var c1 = l.AcceptTcpClient();
            var s1 = c1.GetStream();
            Console.WriteLine("===>{0} connected", c1.Client.RemoteEndPoint);
            while (s1.Read(buffer,0,1024)>0)
            {
                Console.WriteLine("<---[{0}]{1}",DateTime.Now,Encoding.Default.GetString(buffer));
            }
        }
    }

    public void Send(String data)
    {
        if (s.CanWrite)
        {
            s.Write(Encoding.Default.GetBytes(data));
        }
    }

    public String Receive()
    {
        byte[] buf = new byte[1024];
        if (s.CanRead)
        {
            int n = s.Read(buf);
            if (n > 0)
            {
                return Encoding.Default.GetString(buf);
            }
        }
        return String.Empty;
    }

    public void Close()
    {
        if (s.CanRead||s.CanWrite)
        {
            s.Close();
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
                s = c.GetStream();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            c.Dispose();
        }
    }
}