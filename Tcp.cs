﻿using System.Net;
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

    public void Listen(String host ,int port)
    {
        TcpListener server = null;
        try
        {
            IPAddress localAddr = IPAddress.Parse(host);
            server = new TcpListener(localAddr,port);
            server.Start();
            Byte[] bytes = new Byte[256];
            String data = null;
            while(true)
            {
                Console.Write("Waiting for a connection... ");
                using TcpClient client = server.AcceptTcpClient();
                client.SendTimeout = 5000;
                client.ReceiveTimeout = 5000;
                Console.WriteLine("Connected!");
                data = null;
                NetworkStream stream = client.GetStream();
                int i;
                while((i = stream.Read(bytes, 0, bytes.Length))!=0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("<===[{0}]Received: {1}", DateTime.Now,data);
                    data = data.ToUpper();
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    stream.Write(msg, 0, msg.Length);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("===>[{0}]Sent: {1}",DateTime.Now,data);
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
            TcpStream.Write(Encoding.Default.GetBytes(data));
        }
    }

    public String Receive()
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