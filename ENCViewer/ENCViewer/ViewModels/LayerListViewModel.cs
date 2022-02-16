using Esri.ArcGISRuntime.Mapping;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ENCViewer.ViewModels
{
    public class LayerListViewModel : BindableBase, IDialogAware
    {
        private MainWindowViewModel _mainWindowViewModel;


        private ObservableCollection<Layer> _layerListBox = new ObservableCollection<Layer>();
        public ObservableCollection<Layer> LayerListBox
        {
            get { return _layerListBox; }
            set { SetProperty(ref _layerListBox, value); }
        }

        public LayerListViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            LayerListBox.AddRange(_mainWindowViewModel.Map.OperationalLayers.Reverse().ToArray());
        }


        public string Title => "レイヤー一覧";

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
    }
}
