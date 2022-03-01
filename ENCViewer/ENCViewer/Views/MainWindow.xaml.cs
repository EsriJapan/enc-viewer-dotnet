using ENCViewer.ViewModels;
using ENCViewer.Services;
using ENCViewer.Triggers;
using Esri.ArcGISRuntime.UI;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Prism.Events;
using Prism.Services.Dialogs;

namespace ENCViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainWindowViewModel _mainWindowViewModel;
        private IDialogService _dialogService;
        private ICommonDialogService _commonDialogService;

        private DialogParameters _dialogParameters;


        public MainWindow(MainWindowViewModel mainWindowViewModel, IDialogService dialogService, ICommonDialogService commonDialogService)
        {
            InitializeComponent();

            _mainWindowViewModel = mainWindowViewModel;
            _dialogService = dialogService;
            _commonDialogService = commonDialogService;

            Messenger.Instance.GetEvent<PubSubEvent<IdentifyEventArgs>>().Subscribe(ShowCallout);
            Messenger.Instance.GetEvent<PubSubEvent<string>>().Subscribe(message => {
                if (message == "HideCallout") HideCallout();
            });

            Messenger.Instance.GetEvent<PubSubEvent<string>>().Subscribe(message => {
                if (message == "DoPrint") PrintMapView();
            });

        }

        private void ShowCallout(IdentifyEventArgs args)
        {
            MainMapView.ShowCalloutAt(args.MapPoint, args.CalloutDefinition);
        }

        private void HideCallout()
        {
            MainMapView.DismissCallout();
        }

        private async void PrintMapView()
        {
            _mainWindowViewModel.HandlePrint();

            // ダイアログのインスタンスを生成
            var dialogSettings = new SaveFileDialogSettings();
            dialogSettings.Title = "JPEGファイルの保存";
            dialogSettings.Filter = "jpg画像|*.jpg;*.jpeg";
            dialogSettings.AddExtension = true;

            // ダイアログを表示する
            if (_commonDialogService.ShowDialog(dialogSettings) == true)
            {
                try
                {
                    // マップの現在の状態をJPEG画像として出力して、指定のパスに保存する
                    ImageSource image = await RuntimeImageExtensions.ToImageSourceAsync(await MainMapView.ExportImageAsync());
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image));
                    FileStream stream = new FileStream(dialogSettings.FileName, FileMode.Create);
                    encoder.Save(stream);
                    stream.Close();
                }
                catch (Exception ex)
                {
                    _dialogParameters = new DialogParameters();
                    _dialogParameters.Add(nameof(MessageViewModel.MessageLabel), ex.Message);
                    _dialogService.ShowDialog(nameof(Message), _dialogParameters, null);

                    return;
                }
            }

        }

    }
}
