using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TodoApi.Models
{
    public class TimedHostedService : IHostedService, IDisposable
    {

        private Timer _timer;
        private bool _isSending2ABR;


        public Task StartAsync(CancellationToken cancellationToken)
        {
            Models.Tools.guardarLog("Timed Background Service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(Program.AppConfig.TimerSeconds));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            // Models.Tools.guardarLog("Timed Background Service is working.");

            // get Current state
            bool Estado = _isSending2ABR;

            if (Program.carState.IsOn || Program.carState.IsCharging)
            {
                _isSending2ABR = (Program.carState.ChargerCurrent > 0);
                Tools.SaveAndSendData(Tools.serializeReturnTLM(Program.currentTLM), _isSending2ABR);
            }
            else
            {
                _isSending2ABR = false;
            }

            if (Estado != _isSending2ABR)
            {
                // Call HomeAssistant only if Estado is different
                Tools.updateABRPSensorOnHA(_isSending2ABR);
            }
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
