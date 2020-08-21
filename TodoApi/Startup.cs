using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Models;

namespace TodoApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MapConfiguration();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Start MQTT client
            //services.AddMqttClientHostedService();
            //services.AddSingleton<ExternalService>();

            // Start Timer
            //services.AddHostedService<Models.TimedHostedService>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void MapConfiguration()
        {
            MapBrokerHostSettings();
            MapClientSettings();
        }

        private void MapBrokerHostSettings()
        {
            BrokerHostSettings brokerHostSettings = new BrokerHostSettings();
            brokerHostSettings.Host = Program.AppConfig.CLOUDMQTT_SERVER;
            brokerHostSettings.Port = Program.AppConfig.CLOUDMQTT_PORT;
            AppSettingsProvider.BrokerHostSettings = brokerHostSettings;
        }

        private void MapClientSettings()
        {
            ClientSettings clientSettings = new ClientSettings();
            clientSettings.Id = Environment.MachineName;
            clientSettings.Password = Program.AppConfig.CLOUDMQTT_PASSWORD;
            clientSettings.UserName = Program.AppConfig.CLOUDMQTT_USER;
            AppSettingsProvider.ClientSettings = clientSettings;
        }


    }
}
