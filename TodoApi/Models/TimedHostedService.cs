using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TodoApi.Models
{
    public class TimedHostedService : IHostedService, IDisposable
    {

        private Timer _timer;


        public Task StartAsync(CancellationToken cancellationToken)
        {
            Models.Tools.guardarLog("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(Program.AppConfig.TimerSeconds));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            // Models.Tools.guardarLog("Timed Background Service is working.");
            // Just run if it's sending data to ABRP
            //if (Program.carState.isSending2ABRP)
            //{
            if (Program.carState.isOn || Program.currentTLM.is_charging)
            {
                Tools.SaveAndSendData(Tools.serializeReturnTLM(Program.currentTLM));
            }
            //}
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Tools.guardarLog("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
