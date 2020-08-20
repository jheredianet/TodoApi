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
            var Value = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
            if (Program.AppConfig.DebugMode)
            {
                Tools.guardarLog($"+ Topic = {eventArgs.ApplicationMessage.Topic}");
                Tools.guardarLog($"+ Payload = {Value}");
                Tools.guardarLog($"+ QoS = {eventArgs.ApplicationMessage.QualityOfServiceLevel}");
                Tools.guardarLog($"+ Retain = {eventArgs.ApplicationMessage.Retain}");
            }

            // Now we will receive all data an need to parse
            Program.currentTLM.setData(eventArgs.ApplicationMessage.Topic, Value);
            await Task.CompletedTask;
        }


        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            Tools.guardarLog("MQTT Server Connected");

            // Subscribe Topics
            //await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/#");
            await mqttClient.SubscribeAsync("abrp/status");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/e/on");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/c/charging");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/p/latitude");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/p/longitude");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/p/altitude");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/b/soc");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/b/soh");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/p/speed");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/e/temp");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/b/temp");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/b/voltage");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/b/current");
            await mqttClient.SubscribeAsync("ovms/jchm/KonaEV/metric/v/b/power");
        }

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            Tools.guardarLog("MQTT Server Disconnected");
            int i = 0;
            while (!mqttClient.IsConnected)
            {
                i++;
                await Task.Delay(TimeSpan.FromSeconds(5 * i));
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
