using HeliosClockAPIStandard;
using HeliosClockAPIStandard.Controller;
using HeliosClockCommon.Clients;
using HeliosClockCommon.Configurator;
using HeliosClockCommon.Interfaces;
using HeliosClockCommon.LedCommon;
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
            //First start the configuration service
            ConfigureService configureService = new();
            services.AddSingleton(configureService);

            ILedController controller = new LedAPA102Controller();// { LedCount = 92 };
            ILuminManager heliosManager = new LuminManager(controller);

            services.AddSingleton(controller);
            services.AddSingleton(heliosManager);
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
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}