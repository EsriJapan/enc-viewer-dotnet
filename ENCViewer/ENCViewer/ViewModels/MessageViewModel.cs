using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace ENCViewer.ViewModels
{
    public class MessageViewModel : BindableBase, IDialogAware
    {

        private string _messageLabel;
        public string MessageLabel
        {
            get { return _messageLabel; }
            set { SetProperty(ref _messageLabel, value); }
        }

        public MessageViewModel()
        {

        }

        public string Title => "メッセージ";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            MessageLabel = parameters.GetValue<string>(nameof(MessageLabel));

        }
    }
}
