using HeliosClockAPIStandard;
using HeliosClockAPIStandard.Controller;
using LuminCommon.Clients;
using LuminCommon.Configurator;
using LuminCommon.Discorvery;
using LuminCommon.GlobalEvents;
using LuminCommon.Interfaces;
using LuminCommon.LedCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LuminClient
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //First create the configuration file and start the configuration service to read the configuration
            services.AddSingleton<ILuminConfiguration, LuminConfigs>();

            //Starting the configuration service
            services.AddHostedService<ConfigureService>();

            //Add the global event manager
            services.AddSingleton<IGlobalEventManager, GlobalEventManager>();

            //Starting discover factory, creates the unique UDP Client
            services.AddSingleton<DiscoverFactory>();

            //Create an LED Controller
            services.AddSingleton<ILedController, LedAPA102Controller>();

            //Create the Lumin Manager, manages the LED Connection
            services.AddSingleton<ILuminManager, LuminManager>();

            //Start the lumin client, which is the local LED Client, listening to SignarR commands
            services.AddHostedService<LuminClientService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            //Register Application Shutdown Listening
            var eventManager = app.ApplicationServices.GetService<IGlobalEventManager>();
            lifetime.ApplicationStopping.Register(() => eventManager.ThrowGlobalEvent(GlobalEvents.ApplicationShutDown));
        }
    }
}
