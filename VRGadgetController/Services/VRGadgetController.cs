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
        private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);
        private bool _isReconnecting = false;

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

            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;

            await _mqttClient.ConnectAsync(_mqttOptions);
            Console.WriteLine("[Debug] Connected to MQTT Broker.");
        }

        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            if (_isReconnecting)
                return;

            Console.WriteLine("[Debug] MQTT Disconnected. Reconnecting...");
            _isReconnecting = true;

            try
            {
                await _connectionSemaphore.WaitAsync();
                
                // Wait before attempting to reconnect
                await Task.Delay(TimeSpan.FromSeconds(2));
                
                int retryCount = 0;
                const int maxRetries = 5;
                
                while (!_mqttClient.IsConnected && retryCount < maxRetries)
                {
                    try
                    {
                        Console.WriteLine($"[Debug] Reconnection attempt {retryCount + 1}/{maxRetries}");
                        await _mqttClient.ConnectAsync(_mqttOptions);
                        
                        if (_mqttClient.IsConnected)
                        {
                            Console.WriteLine("[Debug] Successfully reconnected to MQTT Broker.");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Reconnection attempt {retryCount + 1} failed: {ex.Message}");
                        retryCount++;
                        
                        if (retryCount < maxRetries)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(Math.Min(5 * retryCount, 30)));
                        }
                    }
                }
                
                if (!_mqttClient.IsConnected)
                {
                    Console.WriteLine("[Error] Failed to reconnect after maximum retry attempts.");
                }
            }
            finally
            {
                _isReconnecting = false;
                _connectionSemaphore.Release();
            }
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
            await _connectionSemaphore.WaitAsync();
            try
            {
                // Wait for reconnection if in progress
                int waitCount = 0;
                const int maxWaitTime = 30; // seconds
                
                while (_isReconnecting && waitCount < maxWaitTime)
                {
                    await Task.Delay(1000);
                    waitCount++;
                }
                
                if (!_mqttClient.IsConnected)
                {
                    Console.WriteLine("[Error] MQTT Client is not connected. Unable to publish.");
                    throw new InvalidOperationException("[Error] MQTT client is not connected");
                }

                var payloadJson = $"{{ \"data\": \"{command}\" }}";
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payloadJson)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(message);
                Console.WriteLine($"[Debug] Message published. Topic: {topic}, Payload: {payloadJson}");
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        public void Dispose()
        {
            _connectionSemaphore?.Dispose();
            _mqttClient?.Dispose();
        }
    }
}
