using Esri.ArcGISRuntime.Mapping;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Windows.Input;

namespace ENCViewer.ViewModels
{
    public class RotateViewModel : BindableBase, IDialogAware
    {
        private MainWindowViewModel _mainWindowViewModel;

        
        private double _opacityValue;
        public double OpacityValue
        {
            get { return _opacityValue; }
            set { SetProperty(ref _opacityValue, value); }
        }

        private double _rotateValue;
        public double RotateValue
        {
            get { return _rotateValue; }
            set { SetProperty(ref _rotateValue, value); }
        }


        public ICommand ChangeOpacity { get; set; }
        public ICommand ChangeRotate { get; set; }

        //public DelegateCommand<object[]> ValueChanged { get; }


        public RotateViewModel(MainWindowViewModel mainWindowViewModel)
        {

            _mainWindowViewModel = mainWindowViewModel;

            ChangeOpacity = new DelegateCommand<object[]>(ChangeOpacityExecute);
            ChangeRotate = new DelegateCommand<object[]>(ChangeRotateExecute);
            //ValueChanged = new DelegateCommand<object[]>(ValueChangedExecute);


            RotateValue = _mainWindowViewModel.Viewpoint.Rotation;

            if (_mainWindowViewModel.Map.OperationalLayers.Count > 0)
            {
                OpacityValue = _mainWindowViewModel.Map.OperationalLayers.First().Opacity;
            } else
            {
                OpacityValue = 1;
            }

        }



        public string Title => "回転・透過";

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

        /*
        private void ValueChangedExecute(object[] items)
        {
            Console.WriteLine("Value Changed");
        }
        */

        private void ChangeOpacityExecute(object[] items)
        {
            if (_mainWindowViewModel.Map.OperationalLayers.Count > 0)
            {
                for (int i = 0; i < _mainWindowViewModel.Map.OperationalLayers.Count; i++)
                {
                    _mainWindowViewModel.Map.OperationalLayers[i].Opacity = OpacityValue;
                }
            }
        }

        private void ChangeRotateExecute(object[] items)
        {
            _mainWindowViewModel.Viewpoint = new Viewpoint(_mainWindowViewModel.Viewpoint.TargetGeometry, RotateValue);
        }

    }
}
