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
            //First start the configuration service
            ConfigureService configureService = new();
            services.AddSingleton(configureService);

            ILedController controller = new LedAPA102Controller();
            ILuminManager heliosManager = new LuminManager(controller);

            services.AddHostedService<DiscroveryServer>();

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });
            services.AddSingleton(controller);
            services.AddSingleton(heliosManager);
            services.AddHostedService<GPIOService>();
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