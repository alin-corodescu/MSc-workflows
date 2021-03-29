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
using OrchestratorService.Definitions;
using OrchestratorService.Proximity;
using OrchestratorService.RequestQueueing;
using OrchestratorService.Transports;
using OrchestratorService.WorkflowSpec;
using OrchestratorService.WorkTracking;

namespace OrchestratorService
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
            services.AddSingleton<IWorkflowRegistry, WorkflowRegistry>();
            services.AddSingleton<IOrchestratorImplementation, OrchestratorImplementation>();
            services.AddSingleton<IClusterStateProvider, KubernetesClusterStateProvider>();
            services.AddSingleton<IPodSelector, KubernetesPodSelector>();
            services.AddSingleton<IGrpcChannelPool, GrpcChannelPool>();
            services.AddSingleton<IRequestRouter, RequestRouter>();
            services.AddSingleton<IWorkTracker, WorkTracker>();
            services.AddSingleton<IProximityTable, ProximityTable>();

            // Request queueing stuff.
            services.AddSingleton<IOrchestrationQueue, OrchestrationQueue>();
            services.AddHostedService<ExecutorService>();
            
            
            services.AddSingleton(new ActivitySource("Workflows"));
            
            // This one creates a singleton of the type TracerProvider.
            services.AddOpenTelemetryTracing((builder) =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Orchestrator"))
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
                endpoints.MapGrpcReflectionService();
                endpoints.MapGrpcService<GrpcOrchestratorTransport>();
                endpoints.MapGrpcService<WorkflowRegistrationService>();
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