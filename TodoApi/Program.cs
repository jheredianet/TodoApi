using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace TodoApi
{
    public class Program
    {
        public static Models.GlobalSettings AppConfig;
        public static void Main(string[] args)
        {
            // Read config from json file
            AppConfig = Models.Tools.getConfig();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }


}
