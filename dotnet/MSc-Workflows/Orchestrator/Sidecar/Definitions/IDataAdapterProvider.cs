using System.Threading.Tasks;
using Commons;
using k8s;
using Microsoft.Extensions.Configuration;
using TestGrpcService.Clients;

namespace TestGrpcService.Definitions
{
    public interface IDataAdapterProvider
    {
        Task<IDataSourceAdapter> GetSourceForName(string name);

        Task<IDataSinkAdapter> GetSinkForName(string name);
    }

    public class DataAdapterProvider : IDataAdapterProvider
    {
        public IGrpcChannelPool ChannelPool { get; }
        private readonly IConfiguration _config;
        private Kubernetes k8s;
        
        public DataAdapterProvider(IConfiguration config, IGrpcChannelPool channelPool)
        {
            ChannelPool = channelPool;
            _config = config;
            k8s = new Kubernetes(KubernetesClientConfiguration.InClusterConfig());
        }
        public async Task<IDataSourceAdapter> GetSourceForName(string name)
        {
            try
            {
                var daemonsets = await k8s.ListDaemonSetForAllNamespacesAsync(labelSelector: $"adapter={name}");

                var port = daemonsets.Items[0].Spec.Template.Spec.Containers[0].Ports[0].HostPort;

                if (port != null)
                {
                    return LocalFileSystemClient.CreateClientForOtherPort(_config, ChannelPool, port.Value);
                }
            }
            catch
            {
            }

            return null;
        }

        public async Task<IDataSinkAdapter> GetSinkForName(string name)
        {
            try
            {
                var daemonsets = await k8s.ListDaemonSetForAllNamespacesAsync(labelSelector: $"adapter={name}");

                var port = daemonsets.Items[0].Spec.Template.Spec.Containers[0].Ports[0].HostPort;

                if (port != null)
                {
                    return LocalFileSystemClient.CreateClientForOtherPort(_config, ChannelPool, port.Value);
                }
            }
            catch
            {
            }

            return null;
        }
    }
}