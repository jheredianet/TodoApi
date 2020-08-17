using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;

namespace TodoApi.Models
{
    public interface IMqttClientService : IHostedService,
                                          IMqttClientConnectedHandler,
                                          IMqttClientDisconnectedHandler,
                                          IMqttApplicationMessageReceivedHandler
    {
    }

    public class MqttClientService : IMqttClientService
    {
        private IMqttClient mqttClient;
        private IMqttClientOptions options;

        public MqttClientService(IMqttClientOptions options)
        {
            this.options = options;
            mqttClient = new MqttFactory().CreateMqttClient();
            ConfigureMqttClient();
        }

        private void ConfigureMqttClient()
        {
            mqttClient.ConnectedHandler = this;
            mqttClient.DisconnectedHandler = this;
            mqttClient.ApplicationMessageReceivedHandler = this;
        }


        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var tlm = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
            if (Program.AppConfig.DebugMode)
            {
                Tools.guardarLog($"+ Topic = {eventArgs.ApplicationMessage.Topic}");
                Tools.guardarLog($"+ Payload = {tlm}");
                Tools.guardarLog($"+ QoS = {eventArgs.ApplicationMessage.QualityOfServiceLevel}");
                Tools.guardarLog($"+ Retain = {eventArgs.ApplicationMessage.Retain}");
            }

            Tools.SendData2ABRP(tlm);
                        
            await Task.CompletedTask;
        }


        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            Tools.guardarLog("MQTT Server Connected");
            await mqttClient.SubscribeAsync("abrp/data");
        }

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            Tools.guardarLog("MQTT Server Disconnected");
            await Task.Delay(TimeSpan.FromSeconds(5));

            while (!mqttClient.IsConnected)
            {
                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
                }
                catch
                {
                    Tools.guardarLog("MQTT Server could not reconnect");
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await mqttClient.ConnectAsync(options);
            if (!mqttClient.IsConnected)
            {
                await mqttClient.ReconnectAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var disconnectOption = new MqttClientDisconnectOptions
                {
                    ReasonCode = MqttClientDisconnectReason.NormalDisconnection,
                    ReasonString = "NormalDiconnection"
                };
                await mqttClient.DisconnectAsync(disconnectOption, cancellationToken);
            }
            await mqttClient.DisconnectAsync();
        }


    }
}
