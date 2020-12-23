using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace OEP520G.Core.ViewModels
{
    public class MessageDialogViewModel : BindableBase, IDialogAware
    {
        //IEventAggregator _ea;

        /// <summary>
        /// 建構函式
        /// </summary>
        //public MessageDialogViewModel(IEventAggregator ea)
        //{
        //    _ea = ea;
        //}

        /********************
         * Prism IDialogService Start
         *******************/
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand
            => _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        protected virtual void CloseDialog(string parameter)
        {
            // 按鍵判別
            ButtonResult result = parameter switch
            {
                "OK" => ButtonResult.OK,
                "Cancel" => ButtonResult.Cancel,
                "Abort" => ButtonResult.Abort,
                "Retry" => ButtonResult.Retry,
                "Ignore" => ButtonResult.Ignore,
                "Yes" => ButtonResult.Yes,
                "No" => ButtonResult.No,
                _ => ButtonResult.None
            };

            // Dialog結束
            RaiseRequestClose(new DialogResult(result));
        }

        // Dialog結束
        public event Action<IDialogResult> RequestClose;
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
            => RequestClose?.Invoke(dialogResult);

        public virtual bool CanCloseDialog() => true;

        public virtual void OnDialogClosed() { }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            MessageContent = parameters.GetValue<string>("Text");
            Title = parameters.GetValue<string>("Title");

            string getValue = parameters.GetValue<string>("Buttons");
            if (getValue != null)
            {
                MsgDialogButtons btns = (MsgDialogButtons)int.Parse(getValue);
                switch (btns)
                {
                    case MsgDialogButtons.OK:
                        OkVisibility = "Visible";
                        YesVisibility = "Collapsed";
                        NoVisibility = "Collapsed";
                        RetryVisibility = "Collapsed";
                        IgnoreVisibility = "Collapsed";
                        AbortVisibility = "Collapsed";
                        CancelVisibility = "Collapsed";
                        break;
                    case MsgDialogButtons.OKCancel:
                        OkVisibility = "Visible";
                        YesVisibility = "Collapsed";
                        NoVisibility = "Collapsed";
                        RetryVisibility = "Collapsed";
                        IgnoreVisibility = "Collapsed";
                        AbortVisibility = "Collapsed";
                        CancelVisibility = "Visible";
                        break;
                    case MsgDialogButtons.YesNo:
                        OkVisibility = "Collapsed";
                        YesVisibility = "Visible";
                        NoVisibility = "Visible";
                        RetryVisibility = "Collapsed";
                        IgnoreVisibility = "Collapsed";
                        AbortVisibility = "Collapsed";
                        CancelVisibility = "Collapsed";
                        break;
                    case MsgDialogButtons.YesNoCancel:
                        OkVisibility = "Collapsed";
                        YesVisibility = "Visible";
                        NoVisibility = "Visible";
                        RetryVisibility = "Collapsed";
                        IgnoreVisibility = "Collapsed";
                        AbortVisibility = "Collapsed";
                        CancelVisibility = "Visible";
                        break;
                    case MsgDialogButtons.AbortRetryIgnore:
                        OkVisibility = "Collapsed";
                        YesVisibility = "Collapsed";
                        NoVisibility = "Collapsed";
                        RetryVisibility = "Visible";
                        IgnoreVisibility = "Collapsed";
                        AbortVisibility = "Visible";
                        CancelVisibility = "Visible";
                        break;
                    case MsgDialogButtons.RetryCancel:
                        OkVisibility = "Collapsed";
                        YesVisibility = "Collapsed";
                        NoVisibility = "Collapsed";
                        RetryVisibility = "Visible";
                        IgnoreVisibility = "Collapsed";
                        AbortVisibility = "Collapsed";
                        CancelVisibility = "Visible";
                        break;
                }
            }

            getValue = parameters.GetValue<string>("Icon");
            if (getValue != null)
            {
                MsgDialogIcon icon = (MsgDialogIcon)int.Parse(getValue);
                switch (icon)
                {
                    case MsgDialogIcon.None:
                        ImageVisibility = false;
                        ImageSource = "";
                        break;
                    case MsgDialogIcon.Hand:
                        ImageVisibility = true;
                        ImageSource = @"Images/MsgDialog_Hand.png";
                        break;
                    case MsgDialogIcon.Question:
                        ImageVisibility = true;
                        ImageSource = @"Images/MsgDialog_Question.png";
                        break;
                    case MsgDialogIcon.Exclamation:
                        ImageVisibility = true;
                        ImageSource = @"Images/MsgDialog_Exclamation.png";
                        break;
                    case MsgDialogIcon.Asterisk:
                        ImageVisibility = true;
                        ImageSource = @"Images/MsgDialog_Asterisk.png";
                        break;
                }
            }
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

        // Message
        private string _messageContent;
        public string MessageContent
        {
            get { return _messageContent; }
            set { SetProperty(ref _messageContent, value); }
        }

        // Image
        private string _imageSource;
        public string ImageSource
        {
            get { return _imageSource; }
            set { SetProperty(ref _imageSource, value); }
        }

        // Visibility
        private bool _imageVisibility;
        public bool ImageVisibility
        {
            get { return _imageVisibility; }
            set { SetProperty(ref _imageVisibility, value); }
        }

        private string _okVisibility;
        public string OkVisibility
        {
            get { return _okVisibility; }
            set { SetProperty(ref _okVisibility, value); }
        }

        private string _yesVisibility;
        public string YesVisibility
        {
            get { return _yesVisibility; }
            set { SetProperty(ref _yesVisibility, value); }
        }

        private string _noVisibility;
        public string NoVisibility
        {
            get { return _noVisibility; }
            set { SetProperty(ref _noVisibility, value); }
        }

        private string _retryVisibility;
        public string RetryVisibility
        {
            get { return _retryVisibility; }
            set { SetProperty(ref _retryVisibility, value); }
        }

        private string _ignoreVisibility;
        public string IgnoreVisibility
        {
            get { return _ignoreVisibility; }
            set { SetProperty(ref _ignoreVisibility, value); }
        }

        private string _abortVisibility;
        public string AbortVisibility
        {
            get { return _abortVisibility; }
            set { SetProperty(ref _abortVisibility, value); }
        }

        private string _cancelVisibility;
        public string CancelVisibility
        {
            get { return _cancelVisibility; }
            set { SetProperty(ref _cancelVisibility, value); }
        }
    }
}
