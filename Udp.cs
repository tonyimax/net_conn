using System.Net;
using System.Net.Sockets;
using System.Text;

namespace net_conn;

public class Udp
{
    private String _host = "127.0.0.1";
    private int _port = 8850;

    
    public string Host
    {
        get => _host;
        set => _host = value ?? throw new ArgumentNullException(nameof(value));
    }

    private int Port
    {
        get => _port;
        set => _port = value;
    }

    //for udp server
    public void Server()
    {
        UdpClient uc1 = new UdpClient(8850);
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Byte[] rBuf = uc1.Receive(ref RemoteIpEndPoint);
            string data = Encoding.Default.GetString(rBuf);
            Console.WriteLine("<===[{0}][{1}:{2}] {3}",DateTime.Now,RemoteIpEndPoint.Address,RemoteIpEndPoint.Port,data);

            Byte[] buf = Encoding.Default.GetBytes(  String.Format("[{0}] Hello Client ,UDP DATA FROM C# Server", DateTime.Now));
            uc1.Send(buf, RemoteIpEndPoint);
            Console.WriteLine("===>{0}",Encoding.Default.GetString(buf));
        }
    }
    
    //for udp client
    public void Client()
    {
        UdpClient uc1 = new UdpClient(Port+1);
        uc1.Connect(Host,Port);
        Byte[] buf = Encoding.Default.GetBytes("Hello Server");
        uc1.Send(buf, buf.Length);
        Console.WriteLine("===>{0}",Encoding.Default.GetString(buf));
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Byte[] rBuf = uc1.Receive(ref RemoteIpEndPoint);
        string data = Encoding.Default.GetString(rBuf);
        Console.WriteLine("<==={0}",data);
    }

    public void Send()
    {
        UdpClient uc1 = new UdpClient(Port);//for local udp
        try{
            uc1.Connect(Host, Port);//for remote udp
            Byte[] buf = Encoding.Default.GetBytes("UDP DATA HELLO");
            uc1.Send(buf, buf.Length);

            //UdpClient udpClientB = new UdpClient();
            //udpClientB.Send(buf, buf.Length, "WK_QT_DEV",Port);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] rBuf = uc1.Receive(ref RemoteIpEndPoint);
            string data = Encoding.Default.GetString(rBuf);
            
            Console.WriteLine("This is the message you received " +
                              data.ToString());
            Console.WriteLine("This message was sent from " +
                              RemoteIpEndPoint.Address.ToString() +
                              " on their port number " +
                              RemoteIpEndPoint.Port.ToString());

            uc1.Close();
            //uc2.Close();
        }
        catch (Exception e ) {
            Console.WriteLine(e.ToString());
        }
    }
}