using System;
using System.Threading.Tasks;
using System.Threading;
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
        private MqttClientOptions? _mqttOptions;

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
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId(clientId)
                .WithCredentials(username, password)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .Build();

            await _mqttClient.ConnectAsync(_mqttOptions);
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

        private async Task PublishAsync(string topic, string command)
        {
            // If not connected, trigger reconnection and wait for it to complete
            if (!_mqttClient.IsConnected)
            {
                Console.WriteLine("[Debug] MQTT Client is not connected. Starting reconnection...");
                await TriggerReconnectionAsync();
                
                // Check if reconnection was successful
                if (!_mqttClient.IsConnected)
                {
                    Console.WriteLine("[Error] Failed to reconnect. Unable to publish message.");
                    throw new InvalidOperationException("[Error] MQTT client could not be reconnected");
                }
            }

            var payloadJson = $"{{ \"data\": \"{command}\" }}";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payloadJson)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            try
            {
                await _mqttClient.PublishAsync(message);
                Console.WriteLine($"[Debug] Message published. Topic: {topic}, Payload: {payloadJson}");
            }
            catch (Exception publishEx)
            {
                Console.WriteLine($"[Error] Failed to publish message: {publishEx.Message}");
                
                // Check if it's a connection-related error and try reconnection once more
                if (!_mqttClient.IsConnected)
                {
                    Console.WriteLine("[Debug] Connection lost during publish. Attempting reconnection...");
                    await TriggerReconnectionAsync();
                    
                    if (_mqttClient.IsConnected)
                    {
                        Console.WriteLine("[Debug] Reconnected successfully. Retrying publish...");
                        await _mqttClient.PublishAsync(message);
                        Console.WriteLine($"[Debug] Message published after reconnection. Topic: {topic}, Payload: {payloadJson}");
                    }
                    else
                    {
                        Console.WriteLine("[Error] Failed to reconnect after publish failure.");
                        throw;
                    }
                }
                else
                {
                    throw; // Re-throw non-connection related exceptions
                }
            }
        }

        private async Task TriggerReconnectionAsync()
        {
            Console.WriteLine("[Debug] Manual reconnection triggered due to publish failure.");
            
            // // Wait before attempting to reconnect
            // await Task.Delay(TimeSpan.FromSeconds(2));
            
            int retryCount = 0;
            const int maxRetries = 5;
            
            while (!_mqttClient.IsConnected && retryCount < maxRetries)
            {
                try
                {
                    Console.WriteLine($"[Debug] Manual reconnection attempt {retryCount + 1}/{maxRetries}");
                    await _mqttClient.ConnectAsync(_mqttOptions);
                    
                    if (_mqttClient.IsConnected)
                    {
                        Console.WriteLine("[Debug] Successfully reconnected to MQTT Broker after publish failure.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Manual reconnection attempt {retryCount + 1} failed: {ex.Message}");
                    retryCount++;
                    
                    if (retryCount < maxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Min(5 * retryCount, 30)));
                    }
                }
            }
            
            if (!_mqttClient.IsConnected)
            {
                Console.WriteLine("[Error] Failed to reconnect after maximum retry attempts (manual trigger).");
            }
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
        }
    }
}
