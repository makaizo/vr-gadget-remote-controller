using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace VRGadgetController.Services
{
    public class VRGadgetController : IVRGadgetController
    {
        private IMqttClient _mqttClient;
        private readonly string _host = "mqtt.beebotte.com";
        private readonly int _port = 1883;
        private readonly string _clientId = "vr-gadget-controller";

        // Beebotte token is hardcoded to avoid Unity's complex file system setup. 
        // Also, it costs nothing to use Beebotte because it has a free tier.
        private readonly string _beebotteToken = "token_eV0DHwv6YCml6X8l";

        private readonly string _vrGadgetTopic = "VRGadget/command";

        public VRGadgetController()
        {
            _mqttClient = new MqttFactory().CreateMqttClient();
        }

        public async Task InitializeAsync()
        {
            try
            {
                await ConnectAsync(_host, _port, _clientId, _beebotteToken, _beebotteToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Debug] Initialize Error: {ex.Message}");
                throw;
            }
        }

        // Control features implementation
        public async Task StartHeatingAsync()
        {
            await SendCommandAsync("start_heating");
        }

        public async Task FinishHeatingAsync()
        {
            await SendCommandAsync("finish_heating");
        }

        public async Task StartCoolingAsync()
        {
            await SendCommandAsync("start_cooling");
        }

        public async Task FinishCoolingAsync()
        {
            await SendCommandAsync("finish_cooling");
        }

        public async Task StartSplashAsync()
        {
            await SendCommandAsync("start_splash");
        }

        public async Task FinishSplashAsync()
        {
            await SendCommandAsync("finish_splash");
        }
        private async Task ConnectAsync(string host, int port, string clientId, string username, string password)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId(clientId)
                .WithCredentials(username, password)
                .Build();

            _mqttClient.DisconnectedAsync += async e =>
            {
                Console.WriteLine("[Debug] MQTT Disconnected. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await _mqttClient.ConnectAsync(options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Reconnection failed: {ex.Message}");
                }
            };

            await _mqttClient.ConnectAsync(options);
            Console.WriteLine("[Debug] Connected to MQTT Broker.");
        }

        private async Task SendCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("[Error] Command cannot be null or empty");
            }

            try
            {
                await PublishAsync(_vrGadgetTopic, command);
                Console.WriteLine($"[Debug] Command sent: {command}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error sending command '{command}': {ex.Message}");
                throw;
            }
        }

        private async Task PublishAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            if (_mqttClient.IsConnected)
            {
                await _mqttClient.PublishAsync(message);
                Console.WriteLine($"[Debug] Message published. Topic: {topic}, Payload: {payload}");
            }
            else
            {
                Console.WriteLine("[Error] MQTT Client is not connected. Unable to publish.");
                throw new InvalidOperationException("[Error] MQTT client is not connected");
            }
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
        }
    }
}
