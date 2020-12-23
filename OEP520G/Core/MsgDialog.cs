using Prism.Services.Dialogs;

namespace OEP520G.Core
{
    public enum MsgDialogButtons
    {
        OK = 0x0,
        OKCancel = 0x1,
        AbortRetryIgnore = 0x2,
        YesNoCancel = 0x3,
        YesNo = 0x4,
        RetryCancel = 0x5
    }

    public enum MsgDialogIcon
    {
        None = 0,
        Hand = 0x10,
        Question = 0x20,
        Exclamation = 0x30,
        Asterisk = 0x40,
        Stop = Hand,
        Error = Hand,
        Warning = Exclamation,
        Information = Asterisk,
    }

    public enum MsgDialogDefaultButton
    {
        Button1 = 0x0,
        Button2 = 0x100,
        Button3 = 0x200
    }

    public class MsgDialog
    {
        public static ButtonResult Show(string text, string title, MsgDialogButtons buttons, MsgDialogIcon icon)
        {
            string para = $"Text={text}&Title={title}&Buttons={(int)buttons}&Icon={(int)icon}";

            ButtonResult result = ButtonResult.None;
            Common.DS.Show("MessageDialog", new DialogParameters(para), r => { result = r.Result; });

            return result;
        }

        public static ButtonResult ShowDialog(string text, string title, MsgDialogButtons buttons, MsgDialogIcon icon)
        {
            string para = $"Text={text}&Title={title}&Buttons={(int)buttons}&Icon={(int)icon}";

            ButtonResult result = ButtonResult.None;
            Common.DS.ShowDialog("MessageDialog", new DialogParameters(para), r => { result = r.Result; });

            return result;
        }
    }
}
