using Esri.ArcGISRuntime.Mapping;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ENCViewer.ViewModels
{
    public class ChangeBasemapViewModel : BindableBase, IDialogAware
    {

        private MainWindowViewModel _mainWindowViewModel;


        public DelegateCommand<object[]> ChangeBasemap { get; }

        public ChangeBasemapViewModel(MainWindowViewModel mainWindowViewModel)
        {

            _mainWindowViewModel = mainWindowViewModel;

            ChangeBasemap = new DelegateCommand<object[]>(ChangeBasemapExecute);

        }

        private Converter.item _basemapTypeValue;
        public Converter.item BasemapTypeValue
        {
            get { return _basemapTypeValue; }
            set
            {
                _basemapTypeValue = value;
                //OnPropertyChanged("BasemapTypeValue");
            }
        }


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


                if (BasemapTypeValue == Converter.item.BasemapImagery)
                {
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISImagery);

                }
                else if (BasemapTypeValue == Converter.item.BasemapStreets)
                {
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISStreets);

                }
                else if (BasemapTypeValue == Converter.item.BasemapTopographic)
                {
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISTopographic);

                }
                else if (BasemapTypeValue == Converter.item.BasemapOceans)
                {
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.ArcGISOceans);

                }
                else if (BasemapTypeValue == Converter.item.BasemapOsm)
                {
                    _mainWindowViewModel.Map.Basemap = new Basemap(BasemapStyle.OSMStandard);

                }
                else if (BasemapTypeValue == Converter.item.BasemapGsi)
                {
                    var webTiledLayer = new WebTiledLayer() { TemplateUri = "https://cyberjapandata.gsi.go.jp/xyz/std/{level}/{col}/{row}.png" };
                    _mainWindowViewModel.Map.Basemap.BaseLayers.Insert(0, webTiledLayer);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("背景地図の変更に失敗しました: " + ex.Message);
            }

        }
    }
}
