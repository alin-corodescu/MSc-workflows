using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using Definitions.Adapters;
using Definitions.Transports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StorageAdapters.Peers;
using Workflows.StorageAdapters.Definitions;

namespace StorageAdapters
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
            // services.AddGrpcReflection();
            // services.AddGrpcHttpApi();
            services.AddSingleton<IDataMasterClient, DataMasterClient>();
            services.AddSingleton<IPeerPool, PeerPool>();
            services.AddSingleton<IStorageAdapter, LocalFileSystemStorageAdapter>();

            services.AddSingleton(new ActivitySource("Workflows"));
            
            // these are the local files store.
            services.AddSingleton<IDictionary<string, int>, ConcurrentDictionary<string, int>>();
            
            // This one creates a singleton of the type TracerProvider.
            services.AddOpenTelemetryTracing((builder) =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DataAdapter"))
                    .AddSource("Workflows")
                    // For incoming requests
                    .AddAspNetCoreInstrumentation()
                    // For outgoing requests
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Export everything to the console.
                    // .AddConsoleExporter();
                    .AddJaegerExporter(o =>
                    {
                        o.AgentHost = Configuration["NODE_IP"];
                        o.MaxPayloadSizeInBytes = 65000;
                        // o.AgentPort = 9411;
                    });
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
                // endpoints.MapGrpcReflectionService();
                endpoints.MapGrpcService<GrpcStorageAdapter>();
                endpoints.MapGrpcService<DataPeerService>();
                endpoints.MapGrpcService<GrpcDataInjectionService>();
                
                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });
        }
    }
}