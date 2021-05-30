using System.Collections.Generic;
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

        private Dictionary<string, IDataSourceAdapter> _sourceAdapters = new Dictionary<string, IDataSourceAdapter>();

        private Dictionary<string, IDataSinkAdapter> _sinkAdapters = new Dictionary<string, IDataSinkAdapter>();
        public DataAdapterProvider(IConfiguration config, IGrpcChannelPool channelPool)
        {
            ChannelPool = channelPool;
            _config = config;
            k8s = new Kubernetes(KubernetesClientConfiguration.InClusterConfig());
        }
        public async Task<IDataSourceAdapter> GetSourceForName(string name)
        {
            if (_sourceAdapters.ContainsKey(name))
            {
                return _sourceAdapters[name];
            }
            try
            {
                var daemonsets = await k8s.ListDaemonSetForAllNamespacesAsync(labelSelector: $"adapter={name}");

                var port = daemonsets.Items[0].Spec.Template.Spec.Containers[0].Ports[0].HostPort;

                if (port != null)
                {
                    
                    var client =  LocalFileSystemClient.CreateClientForOtherPort(_config, ChannelPool, port.Value);
                    _sourceAdapters[name] = client;
                    return client;
                }
            }
            catch
            {
            }

            return null;
        }

        public async Task<IDataSinkAdapter> GetSinkForName(string name)
        {
            if (_sinkAdapters.ContainsKey(name))
            {
                return _sinkAdapters[name];
            }
            try
            {
                var daemonsets = await k8s.ListDaemonSetForAllNamespacesAsync(labelSelector: $"adapter={name}");

                var port = daemonsets.Items[0].Spec.Template.Spec.Containers[0].Ports[0].HostPort;

                if (port != null)
                {
                    
                    var client =  LocalFileSystemClient.CreateClientForOtherPort(_config, ChannelPool, port.Value);
                    _sinkAdapters[name] = client;
                    return client;
                }
            }
            catch
            {
            }

            return null;
        }
    }
}