using Prism.Ioc;
using Prism.Modularity;
using TcpipServer.Contracts;
using TcpipServer.Services;

namespace TcpipServer
{
    public class TcpipServerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ITcpipServer, TcpipServerService>();
        }
    }
}