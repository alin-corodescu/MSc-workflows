using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DataMaster
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddGrpc();
            services.AddGrpcReflection();
            services.AddGrpcHttpApi();
            services.AddSingleton<IDataChunkLedger, DataChunkLedger>();
            
            services.AddSingleton(new ActivitySource("Workflows"));
            
            // This one creates a singleton of the type TracerProvider.
            services.AddOpenTelemetryTracing((builder) =>
            {
                builder
                    // Set the resource name for prettier printing.
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DataMaster"))
                    // Register the source which emits our own activities.
                    .AddSource("Worfklows")
                    // For incoming requests
                    .AddAspNetCoreInstrumentation()
                    // For outgoing requests
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Export everything to the console.
                    // .AddConsoleExporter();
                    .AddJaegerExporter();
            });
            
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            
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
                endpoints.MapGrpcReflectionService();
                endpoints.MapGrpcService<DataMasterService>();
            });
        }
    }
}