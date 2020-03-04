using System.IO;
using BankOfDotNet.API.Extentions;
using BankOfDotNet.API.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankOfDotNet.API
{
    public class Program
    {

        public static void Main(string[] args)
        {

            var build = BuildWebHost(args);



            build.MigrateDbContext<BankContext>((context, services) =>
            {


            });
            build.Run();
        }


        public static IWebHost BuildWebHost(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => options.AddServerHeader = false)//-
                                                                       //.UseHealthChecks("/hc")
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseStartup<Startup>()
               .ConfigureAppConfiguration((builderContext, config) =>//-
               {
                   config.AddEnvironmentVariables();
               })
               .ConfigureLogging((hostingContext, builder) =>//-
               {
                   builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                   builder.AddConsole();
                   builder.AddDebug();
               })

               //.UseApplicationInsights()
               .Build();




        //public static void Main(string[] args)
        //{
        //    CreateWebHostBuilder(args).Build().Run();
        //}

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>();
    }
}
