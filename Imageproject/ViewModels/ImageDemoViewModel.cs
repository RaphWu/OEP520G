using Imageproject.Models;
using Imageproject.Services;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using TcpipServer.Contracts;

namespace Imageproject.ViewModels
{
    public class ImageDemoViewModel : BindableBase
    {
        private readonly IEventAggregator _ea;
        private readonly ITcpipServer _tcpipServer;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ImageDemoViewModel(IEventAggregator ea, ITcpipServer tcpipServer)
        {
            _ea = ea;
            _tcpipServer = tcpipServer;

            _tcpipServer.ConnectServer();
            _ea.GetEvent<RequestUpdateDemo>().Subscribe(UpdateListView);
        }

        // window close
        private ICommand _unloadedCommand;
        public ICommand UnloadedCommand
            => _unloadedCommand ??= new DelegateCommand(OnUnloaded);
        private void OnUnloaded()
        {
            _tcpipServer.CloseServer();
        }

        private void UpdateListView(string msg)
        {
            //ImageSampleList = new ObservableCollection<ListViewData>();
            //foreach (var item in ImageParameters.ReceiveFrame)
            //{
            //    var objectImage = ImageParameters.ImageList.Find(x => x.ObjectId == item.ObjectId);
            //    ImageSampleList.Add(new ListViewData()
            //    {
            //        ObjectId = (int)objectImage.ObjectId,
            //        Title = objectImage.Title,
            //        imgByte = null,
            //        X = objectImage.X,
            //        Y = objectImage.Y,
            //        A = objectImage.A
            //    });
            //}
        }

        /********************
         * Data Binding
         ********************/
        private ObservableCollection<ListViewData> _ImageSampleList;

        public ObservableCollection<ListViewData> ImageSampleList
        {
            get => _ImageSampleList;
            set => SetProperty(ref _ImageSampleList, value);
        }

    }
}
