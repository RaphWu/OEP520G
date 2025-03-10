﻿using Imageproject.Contracts;
using Imageproject.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace Imageproject
{
    public class ImageprojectModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IImage, ImageClass>();
            //containerRegistry.RegisterForNavigation<Imagedisplay, ImagedisplayViewModels>();
        }
    }
}