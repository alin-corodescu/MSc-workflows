using System;
using System.Diagnostics;
using Commons;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TestGrpcService.Clients;
using TestGrpcService.Definitions;
using TestGrpcService.Transports;

namespace TestGrpcService
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

            services.AddSingleton<IComputeStep, ComputeStepGrpcClient>();
            services.AddSingleton<IDataSourceAdapter, LocalFileSystemClient>();
            services.AddSingleton<IDataSinkAdapter, LocalFileSystemClient>();
            services.AddSingleton<IOrchestratorServiceClient, OrchestratorServiceClient>();
            services.AddSingleton<IGrpcChannelPool, GrpcChannelPool>();
            services.AddSingleton<ISidecar, Sidecar>();
            
            services.AddSingleton(new ActivitySource("Workflows"));
            
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
                    .AddJaegerExporter();
            });
            
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            
            Console.WriteLine($"Config: {Configuration["Sidecar:InputPath"]}");
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
                endpoints.MapGrpcService<GrpcSidecarTransport>();
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