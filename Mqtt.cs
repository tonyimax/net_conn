using MQTTnet;
using System.Text;

namespace net_conn;

public class Mqtt
{
    private static string ip = "43.136.40.107";
    public static async Task Subscribe()
    {
        var mqttFactory = new MqttClientFactory();
        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithCredentials("ubuntu_dev_192_168_119_130","6e94a638d6b35f43de8a2f0cd1644089cb004e19e8667cfd0e427a7437032e75")
                .WithTcpServer(ip).Build();
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine($"Recv===>:clientid:{e.ClientId}," +
                                  $"topic:{e.ApplicationMessage.Topic}," +
                                  $"message:{Encoding.UTF8.GetString(e.ApplicationMessage.Payload.First.Span)}");
                return Task.CompletedTask;
            };
            var mccResult = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            if (mccResult.ResultCode != MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("===>[{0}] Publish->ConnectAsync Result:{1},see MqttClientConnectResultCode",DateTime.Now,mccResult.ResultCode);
                return;
            }
            var mqttSubscribeOptions = mqttFactory.
                CreateSubscribeOptionsBuilder()
                .WithTopicFilter("test/topic")
                .Build();
            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            Console.WriteLine("subscribe success : test/topic ......");
            Console.WriteLine("press any key to exit......");
            Console.ReadLine();
            
            var mqttUnSubscribeOptions = mqttFactory.
                CreateUnsubscribeOptionsBuilder()
                .WithTopicFilter("test/topic")
                .Build();
            await mqttClient.UnsubscribeAsync(mqttUnSubscribeOptions);
            
            Console.ReadLine();
        }
    }

    public static async Task Publish()
    {
        var mqttFactory = new MqttClientFactory();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId("dotnet-client")
            .WithCredentials("ubuntu_dev_192_168_119_130","6e94a638d6b35f43de8a2f0cd1644089cb004e19e8667cfd0e427a7437032e75")
            .WithTcpServer(ip, 1883)
            .Build();
 
        var mqttClient = mqttFactory.CreateMqttClient();
        var mccResult = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        if (mccResult.ResultCode != MqttClientConnectResultCode.Success)
        {
            Console.WriteLine("===>[{0}] Publish->ConnectAsync Result:{1},see MqttClientConnectResultCode",DateTime.Now,mccResult.ResultCode);
            return;
        }
        Console.WriteLine("Connected to MQTT broker");

        while (true)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("test/topic")
                .WithPayload(String.Format("[{0}] this message publish use C#",DateTime.Now))
                .Build();
            var result = await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            Console.WriteLine("publish message to mqtt server Success -> IsSuccess:{0}",result.IsSuccess);
            await Task.Delay(1000);
        }
    }

}
