using Esri.ArcGISRuntime.Hydrography;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ENCViewer.ViewModels
{
    public class EncSettingViewModel : BindableBase, IDialogAware
    {

        private Converter.item _encColorSchemeValue;
        public Converter.item EncColorSchemeValue
        {
            get { return _encColorSchemeValue; }
            set
            {
                _encColorSchemeValue = value;
                //OnPropertyChanged("EncColorSchemeValue");
            }
        }

        private Converter.item _encAreaValue;
        public Converter.item EncAreaValue
        {
            get { return _encAreaValue; }
            set
            {
                _encAreaValue = value;
            }
        }

        private Converter.item _encPointValue;
        public Converter.item EncPointValue
        {
            get { return _encPointValue; }
            set
            {
                _encPointValue = value;
            }
        }


        public DelegateCommand SetDisplaySettingButton { get; }


        public EncSettingViewModel()
        {
            SetDisplaySettingButton = new DelegateCommand(SetDisplaySettingButtonExecute);

        }

        public string Title => "ENC表示設定";

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

        private void SetDisplaySettingButtonExecute()
        {
            
            // アプリケーションにENC表示設定を適用させる
            EncDisplaySettings globalDisplaySettings = EncEnvironmentSettings.Default.DisplaySettings;
            EncMarinerSettings globalMarinerSettings = globalDisplaySettings.MarinerSettings;

            // カラー設定
            if (EncColorSchemeValue == Converter.item.EncColorDay) { globalMarinerSettings.ColorScheme = EncColorScheme.Day; }
            else if (EncColorSchemeValue == Converter.item.EncColorDusk) { globalMarinerSettings.ColorScheme = EncColorScheme.Dusk; }
            else if (EncColorSchemeValue == Converter.item.EncColorNight) { globalMarinerSettings.ColorScheme = EncColorScheme.Night; }

            // エリア表示設定
            if (EncAreaValue == Converter.item.EncAreaPlain) { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain; }
            else if (EncAreaValue == Converter.item.EncAreaSymbol) { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized; }

            // ポイント表示設定
            if (EncPointValue == Converter.item.EncPointPaper) { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart; }
            else if (EncPointValue == Converter.item.EncPointSimple) { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified; }
            
        }
    }






}
