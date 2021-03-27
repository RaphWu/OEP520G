//using OEP520G.Core.Constants;
using OEP520G.Dispensing.Contracts;
using OEP520G.Dispensing.Services;
using OEP520G.Dispensing.ViewModels;
using OEP520G.Dispensing.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace OEP520G.Dispensing
{
    public class DispensingModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDispensing, DispensingService>();

            //containerRegistry.RegisterForNavigation<DispensingSetting, DispensingSettingViewModel>(PageKeys.DispensingSetting);
        }
    }
}