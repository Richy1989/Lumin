using HeliosClockAPIStandard;
using HeliosClockAPIStandard.Controller;
using HeliosClockAPIStandard.GPIOListeners;
using HeliosClockCommon.Clients;
using HeliosClockCommon.Configurator;
using HeliosClockCommon.Defaults;
using HeliosClockCommon.Discorvery;
using HeliosClockCommon.Hubs;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HeliosService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //First create the configuration file and start the configuration service to read the configuration
            services.AddSingleton<ILuminConfiguration, LuminConfigs>();
            services.AddHostedService<ConfigureService>();

            //Start the discovery service to find server IP
            services.AddHostedService<DiscroveryServer>();

            //Create an LED Controller
            services.AddSingleton<ILedController, LedAPA102Controller>();

            //Create the Lumin Manager, manages the LED Connection
            services.AddSingleton<ILuminManager, LuminManager>();

            //Initialize the SignarR Server
            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
            
            //Start the GPIO watch server, to listen on physical button touch
            services.AddHostedService<GPIOService>();

            //Start the lumin client, which is the local LED Client, listening to SignaR commands
            services.AddHostedService<LuminClientService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<HeliosHub>(DefaultValues.BaseUrl);
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with SignalR");
                });
            });
        }
    }
}