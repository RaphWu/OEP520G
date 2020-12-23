using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace OEP520G.Core.ViewModels
{
    public class CrudDialogViewModel : BindableBase, IDialogAware
    {
        IEventAggregator _ea;

        /// <summary>
        /// 建構函式
        /// </summary>
        public CrudDialogViewModel(IEventAggregator ea)
        {
            _ea = ea;
        }

        /********************
         * Prism IDialogService Start
         *******************/
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand
            => _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        protected virtual void CloseDialog(string parameter)
        {
            // 按鍵判別
            ButtonResult result = ButtonResult.None;
            if (parameter == "OK")
                result = ButtonResult.OK;
            else if (parameter == "Cancel")
                result = ButtonResult.Cancel;

            // 使用聚合器傳回結果
            if (result == ButtonResult.OK)
                _ea.GetEvent<CrudDialogReceiver>().Publish(new CrudDialogData()
                {
                    Result = result,
                    Field1 = Field1,
                    Field2 = Field2,
                    Field3 = Field3,
                    Field4 = Field4
                });

            // Dialog結束
            RaiseRequestClose(new DialogResult(result));
        }

        // Dialog結束
        public event Action<IDialogResult> RequestClose;
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public virtual bool CanCloseDialog() => true;

        /// <summary>
        /// Dialog關閉時處理
        /// </summary>
        public virtual void OnDialogClosed() { }

        /// <summary>
        /// Dialog開啟時處理
        /// </summary>
        /// <param name="parameters">傳入參數</param>
        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("Title");

            Field1 = parameters.GetValue<string>("Field1");
            Field1Label = parameters.GetValue<string>("Field1Label");
            Field1Visibility = parameters.GetValue<string>("Field1Visibility");
            Field1Enabled = parameters.GetValue<string>("Field1Enabled");

            Field2 = parameters.GetValue<string>("Field2");
            Field2Label = parameters.GetValue<string>("Field2Label");
            Field2Visibility = parameters.GetValue<string>("Field2Visibility");
            Field2Enabled = parameters.GetValue<string>("Field2Enabled");

            Field3 = parameters.GetValue<string>("Field3");
            Field3Label = parameters.GetValue<string>("Field3Label");
            Field3Visibility = parameters.GetValue<string>("Field3Visibility");
            Field3Enabled = parameters.GetValue<string>("Field3Enabled");

            Field4 = parameters.GetValue<string>("Field4");
            Field4Label = parameters.GetValue<string>("Field4Label");
            Field4Visibility = parameters.GetValue<string>("Field4Visibility");
            Field4Enabled = parameters.GetValue<string>("Field4Enabled");
        }
        /********************
         * Prism IDialogService End
         *******************/

        /********************
         * 繫結
         *******************/
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        /******** 1 ********/
        private string _field1;
        public string Field1
        {
            get { return _field1; }
            set { SetProperty(ref _field1, value); }
        }

        private string _field1Label;
        public string Field1Label
        {
            get { return _field1Label; }
            set { SetProperty(ref _field1Label, value); }
        }

        private string _field1Visibility;
        public string Field1Visibility
        {
            get { return _field1Visibility; }
            set { SetProperty(ref _field1Visibility, value); }
        }

        private string _field1Enabled;
        public string Field1Enabled
        {
            get { return _field1Enabled; }
            set { SetProperty(ref _field1Enabled, value); }
        }

        /******** 2 ********/
        private string _field2;
        public string Field2
        {
            get { return _field2; }
            set { SetProperty(ref _field2, value); }
        }

        private string _field2Label;
        public string Field2Label
        {
            get { return _field2Label; }
            set { SetProperty(ref _field2Label, value); }
        }

        private string _field2Visibility;
        public string Field2Visibility
        {
            get { return _field2Visibility; }
            set { SetProperty(ref _field2Visibility, value); }
        }

        private string _field2Enabled;
        public string Field2Enabled
        {
            get { return _field2Enabled; }
            set { SetProperty(ref _field2Enabled, value); }
        }

        /******** 3 ********/
        private string _field3;
        public string Field3
        {
            get { return _field3; }
            set { SetProperty(ref _field3, value); }
        }

        private string _field3Label;
        public string Field3Label
        {
            get { return _field3Label; }
            set { SetProperty(ref _field3Label, value); }
        }

        private string _field3Visibility;
        public string Field3Visibility
        {
            get { return _field3Visibility; }
            set { SetProperty(ref _field3Visibility, value); }
        }

        private string _field3Enabled;
        public string Field3Enabled
        {
            get { return _field3Enabled; }
            set { SetProperty(ref _field3Enabled, value); }
        }

        /******** 4 ********/
        private string _field4;
        public string Field4
        {
            get { return _field4; }
            set { SetProperty(ref _field4, value); }
        }

        private string _field4Label;
        public string Field4Label
        {
            get { return _field4Label; }
            set { SetProperty(ref _field4Label, value); }
        }

        private string _field4Visibility;
        public string Field4Visibility
        {
            get { return _field4Visibility; }
            set { SetProperty(ref _field4Visibility, value); }
        }

        private string _field4Enabled;

        public string Field4Enabled
        {
            get { return _field4Enabled; }
            set { SetProperty(ref _field4Enabled, value); }
        }
    }
}
