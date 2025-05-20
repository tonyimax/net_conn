using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;

namespace net_conn;

public class Udp
{
    private String _host = "127.0.0.1";
    private int _port = 8850;

    public Udp()
    {

    }
    public Udp(String host)
    {
        _host = host;
    }
    public Udp(int port)
    {
        _port = port;
    }

    public Udp(string host, int port)
    {
        _host = host;
        _port = port;
    }

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

    public void sendFrequency(UdpClient uc1)
    {
        new Thread(() =>
        {
            byte[] buf = new byte[8];
            buf[0] = 0xdd;
            buf[1] = 0xdd;
            buf[2] = 0xfa;
            buf[3] = 0x03;
            buf[4] = 0x03;
            buf[5] = 0x00;
            buf[6] = 0x01;
            int sum = 0;
            for (int i = 0; i < 7; i++){
                sum += buf[i];
            }
            buf[7] = (byte)(sum & 0xff);
            uc1.Send(buf,8);
            Console.WriteLine("===>Send对频数据: {0}",buf[4]);
        }).Start();
    }

    //for udp server
    public void Server()
    {
        UdpClient uc1 = new UdpClient(8811);
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            bool isFreq=false;
            Byte[] rBuf = uc1.Receive(ref RemoteIpEndPoint);
            if (rBuf?.Length > 0 && rBuf[0] == 0xdd && rBuf[1] == 0xdd && rBuf[2] == 0xfa && rBuf[4] == 0x03)
            {
                isFreq = true;
                Console.WriteLine("===>接收到客户端的对频数据:{0}",rBuf[4]);
            }

            JsonObject? obj = null;
            string data = Encoding.Default.GetString(rBuf);
            Console.WriteLine("<===[{0}][{1}:{2}] {3}",DateTime.Now,RemoteIpEndPoint.Address,RemoteIpEndPoint.Port,data);
            lock (data)
            {
                HttpClient client = new HttpClient();
                var r = client.GetFromJsonAsync(
                    "https://qifu-api.baidubce.com/ip/geo/v1/district?ip="+RemoteIpEndPoint.Address,
                    new JsonObject().GetType(),
                    CancellationToken.None);
                r.Wait();
                obj = (JsonObject?)r.Result;
            }
            if (obj!= null)
            {
                Console.WriteLine(obj);
                var code =obj["code"]?.ToString();
                if (code == "Success")
                {
                    var areaData = (JsonObject?)obj["data"];
                    var ip = obj["ip"]?.ToString();
                    var country =  areaData?["country"];
                    if (country!=null && country.GetValue<String>().Equals("中国")||areaData["continent"].GetValue<String>().Equals("保留IP"))
                    {
                        if (isFreq)
                        {
                            byte[] buf = new byte[8];
                            buf[0] = 0xdd;
                            buf[1] = 0xdd;
                            buf[2] = 0xfa;
                            buf[3] = 0x03;
                            buf[4] = 0x01;
                            buf[5] = 0x00;
                            buf[6] = 0x01;
                            int sum = 0;
                            for (int i = 0; i < 7; i++){
                                sum += buf[i];
                            }
                            buf[7] = (byte)(sum & 0xff);
                            uc1.Send(buf, RemoteIpEndPoint);
                            Console.WriteLine("===>回复对频数据: {0}",buf[4]);
                        }
                        else
                        {
                            Byte[] buf = Encoding.Default.GetBytes(  String.Format("[{0}] Hello Client ,UDP DATA FROM C# Server", DateTime.Now));
                            uc1.Send(buf, RemoteIpEndPoint);
                            Console.WriteLine("===>{0}",Encoding.Default.GetString(buf));
                        }
                    }
                }
            }
        }
    }
    
    //for udp client
    public void Client()
    {
        UdpClient uc1 = new UdpClient();
        uc1.Connect(Host,Port);
        
        new Thread(() =>
        {
            while (true)
            {
                try
                {
                    bool isFreq = false;
                    Byte[] buf = Encoding.Default.GetBytes( String.Format("[{0}] Hello Server UDP DATA FROM C# Client",DateTime.Now));
                    uc1.Send(buf, buf.Length);
                    Console.WriteLine("===>{0}",Encoding.Default.GetString(buf));
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    Byte[] rBuf = uc1.Receive(ref RemoteIpEndPoint);
                    if (rBuf?.Length > 0 && rBuf[0]==0xdd &&  rBuf[1]==0xdd && rBuf[2]==0xfa)
                    {
                        isFreq = true;
                        switch (rBuf[4])
                        {
                            case 0:
                                Console.WriteLine("===>对频失败");
                                Thread.Sleep(2000);
                                sendFrequency(uc1);
                                break;
                            case 1:
                                Console.WriteLine("===>对频成功");
                                break;
                            case 2:
                                Console.WriteLine("===>对频中");
                                break;
                            case 3:
                                Console.WriteLine("===>开始对频");
                                break;
                        }
                    }

                    if (isFreq)
                    {
                        Console.WriteLine("<===对频结果:{0}",rBuf[4]);
                    }
                    else
                    {
                        string data = Encoding.Default.GetString(rBuf);
                        Console.WriteLine("<==={0}",data);
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }   
        }).Start();
        
        Thread.Sleep(1000);
        sendFrequency(uc1);
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