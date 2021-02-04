using OEP520G.EPRegulator.Contracts;
using OEP520G.EPRegulator.Services;
using OEP520G.EPRegulator.ViewModels;
using OEP520G.EPRegulator.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace OEP520G.EPRegulator
{
    public class EPRegulatorModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IEpr, EprService>();

            //containerRegistry.RegisterForNavigation<EprCorrect, EprCorrectViewModel>();
        }
    }
}