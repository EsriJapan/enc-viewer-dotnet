using ENCViewer.Views;
using Esri.ArcGISRuntime.Mapping;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace ENCViewer.ViewModels
{
    public class ChangeBasemapViewModel : BindableBase, IDialogAware
    {

        private MainWindowViewModel _mainWindowViewModel;
        private IDialogService _dialogService;
        private DialogParameters _dialogParameters;

        public ChangeBasemapViewModel(MainWindowViewModel mainWindowViewModel, IDialogService dialogService)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _dialogService = dialogService;

            ChangeBasemap = new DelegateCommand<object[]>(ChangeBasemapExecute);


            // 現在の背景地図を取得
            if (_mainWindowViewModel.Map.Basemap.Name == "ArcGISImagery")
            {

                BasemapTypeValue = Converter.item.BasemapImagery;

            }
            else if (_mainWindowViewModel.Map.Basemap.Name == "ArcGISStreets")
            {
                BasemapTypeValue = Converter.item.BasemapStreets;

            }
            else if (_mainWindowViewModel.Map.Basemap.Name == "ArcGISTopographic")
            {
                BasemapTypeValue = Converter.item.BasemapTopographic;

            }
            else if (_mainWindowViewModel.Map.Basemap.Name == "ArcGISOceans")
            {
                BasemapTypeValue = Converter.item.BasemapOceans;

            }
            else if (_mainWindowViewModel.Map.Basemap.Name == "OSMStandard")
            {
                BasemapTypeValue = Converter.item.BasemapOsm;

            }
            else if (_mainWindowViewModel.Map.Basemap.Name == "GSI")
            {
                BasemapTypeValue = Converter.item.BasemapGsi;

            }

        }

        private Converter.item _basemapTypeValue;
        public Converter.item BasemapTypeValue
        {
            get { return _basemapTypeValue; }
            set { _basemapTypeValue = value; }
        }

        public DelegateCommand<object[]> ChangeBasemap { get; }


        public string Title => "背景地図の変更";

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
        }

        private void ChangeBasemapExecute(object[] items)
        {

            try
            {
                // 現在の背景地図を削除
                _mainWindowViewModel.Map.Basemap.BaseLayers.RemoveAt(0);


                // 背景地図を設定
                if (BasemapTypeValue == Converter.item.BasemapImagery)
                {
                    // 衛星画像
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISImagery);
                    _mainWindowViewModel.Map.Basemap.Name = "ArcGISImagery";

                }
                else if (BasemapTypeValue == Converter.item.BasemapStreets)
                {
                    // 道路地図
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISStreets);
                    _mainWindowViewModel.Map.Basemap.Name = "ArcGISStreets";

                }
                else if (BasemapTypeValue == Converter.item.BasemapTopographic)
                {
                    // 地形図
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISTopographic);
                    _mainWindowViewModel.Map.Basemap.Name = "ArcGISTopographic";

                }
                else if (BasemapTypeValue == Converter.item.BasemapOceans)
                {
                    // 海洋図
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISOceans);
                    _mainWindowViewModel.Map.Basemap.Name = "ArcGISOceans";

                }
                else if (BasemapTypeValue == Converter.item.BasemapOsm)
                {
                    // OpenStreetMap
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.OSMStandard);
                    _mainWindowViewModel.Map.Basemap.Name = "OSMStandard";

                }
                else if (BasemapTypeValue == Converter.item.BasemapGsi)
                {
                    // 地理院地図
                    var webTiledLayer = new WebTiledLayer() { TemplateUri = "https://cyberjapandata.gsi.go.jp/xyz/std/{level}/{col}/{row}.png" };
                    _mainWindowViewModel.Map.Basemap.BaseLayers.Insert(0, webTiledLayer);
                    _mainWindowViewModel.Map.Basemap.Name = "GSI";
                }

            }
            catch (Exception ex)
            {
                _dialogParameters = new DialogParameters();
                _dialogParameters.Add(nameof(MessageViewModel.MessageLabel), "背景地図の変更に失敗しました: " + ex.Message);
                _dialogService.ShowDialog(nameof(Message), _dialogParameters, null);
            }

        }
    }
}
