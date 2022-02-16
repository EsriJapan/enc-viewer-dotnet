using ENCViewer.ViewModels;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ENCViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;

        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            _mainWindowViewModel = mainWindowViewModel;

        }

        public async void MainMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                MainMapView.DismissCallout();
                IReadOnlyList<IdentifyLayerResult> results = await MainMapView.IdentifyLayersAsync(e.Position, 10, false);
                MainWindowViewModel viewmodel = this.Resources["MainWindowViewModel"] as MainWindowViewModel;
                CalloutDefinition definition = _mainWindowViewModel.HandleTap(results);
                MainMapView.ShowCalloutAt(e.Location, definition);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
        }

        private async void _printButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // マップの現在の状態をJPEG画像として出力して、指定のパスに保存する
                ImageSource image = await RuntimeImageExtensions.ToImageSourceAsync(await MainMapView.ExportImageAsync());
                MainWindowViewModel viewmodel = this.Resources["MainWindowViewModel"] as MainWindowViewModel;
                _mainWindowViewModel.PrintImage(image);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
        }
    }
}
