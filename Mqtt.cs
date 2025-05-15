using MQTTnet;
using System.Text;

namespace net_conn;

public class Mqtt
{
    public static async Task Subscribe()
    {
        var mqttFactory = new MqttClientFactory();
        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("192.168.119.130").Build();
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine($"Recv===>:clientid:{e.ClientId}," +
                                  $"topic:{e.ApplicationMessage.Topic}," +
                                  $"message:{Encoding.UTF8.GetString(e.ApplicationMessage.Payload.First.Span)}");
                return Task.CompletedTask;
            };
            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            
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
            .WithTcpServer("192.168.119.130", 1883)
            .Build();
 
        var mqttClient = mqttFactory.CreateMqttClient();
        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
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
