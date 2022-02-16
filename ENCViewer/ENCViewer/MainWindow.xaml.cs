using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Hydrography;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Threading.Tasks;

namespace ENCViewer
{


    public partial class MainWindow : Window
    {

        private GraphicsOverlay _sketchOverlay;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();

        }


        private void Initialize()
        {

            ArcGISRuntimeEnvironment.ApiKey = "APIキーを入力";
            
            _mainMapView.Map = new Map(BasemapStyle.ArcGISStreets);

            // マップ上のクリックイベントの登録
            _mainMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            _sketchOverlay = new GraphicsOverlay();
            _mainMapView.GraphicsOverlays.Add(_sketchOverlay);

        }


        #region レイヤーの透過・回転設定
        private void RotateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // スライダー値に応じてマップを回転
            _mainMapView.SetViewpointRotationAsync(e.NewValue);
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // マップ上に追加しているENCレイヤーの透過設定
            if (_mainMapView.Map.OperationalLayers.Count > 0)
            {
                for (int i = 0; i < _mainMapView.Map.OperationalLayers.Count; i++)
                {
                    _mainMapView.Map.OperationalLayers[i].Opacity = 1 - e.NewValue;
                }
            }
        }
        #endregion


        #region パネルの表示・非表示
        private void _showRotateButton_Click(object sender, RoutedEventArgs e)
        {
            // 回転設定パネルを表示
            this._rotatePanel.Visibility = this._basemapPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void _closeRotatePanelButton_Click(object sender, RoutedEventArgs e)
        {
            // 回転設定パネルを非表示
            this._rotatePanel.Visibility = Visibility.Collapsed;
        }

        private void _showSettingDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            // 表示設定パネルを表示
            this._displayPanel.Visibility = this._basemapPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void _closeDisplayPanelButton_Click(object sender, RoutedEventArgs e)
        {
            // 表示設定パネルを非表示
            this._displayPanel.Visibility = Visibility.Collapsed;
        }

        private void _changeBaseMapButton_Click(object sender, RoutedEventArgs e)
        {
            // 背景地図パネルを表示
            this._basemapPanel.Visibility = this._basemapPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void _closeBaseMapPanelButton_Click(object sender, RoutedEventArgs e)
        {
            // 背景地図パネルを非表示
            this._basemapPanel.Visibility = Visibility.Collapsed;
        }

        private void _showLayerListButton_Click(object sender, RoutedEventArgs e)
        {
            // レイヤー一覧パネルを表示
            this._layerListPanel.Visibility = this._layerListPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            if (this._layerListPanel.Visibility == Visibility.Visible)
            {
                // レイヤーリストを更新
                _legend.ItemsSource = _mainMapView.Map.OperationalLayers.Reverse();
            }
        }

        private void _closeLayerListPanelButton_Click(object sender, RoutedEventArgs e)
        {
            // レイヤー一覧パネルを閉じる
            this._layerListPanel.Visibility = Visibility.Collapsed;
        }
        #endregion



        #region ENC レイヤーの表示設定
        private void _settingDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            // ENCレイヤー表示設定の適用
            UpdateDisplaySettings();
        }

        private void UpdateDisplaySettings()
        {
            // アプリケーションにENC表示設定を適用させる
            EncDisplaySettings globalDisplaySettings = EncEnvironmentSettings.Default.DisplaySettings;
            EncMarinerSettings globalMarinerSettings = globalDisplaySettings.MarinerSettings;

            // カラー設定
            if ((bool)radDay.IsChecked) { globalMarinerSettings.ColorScheme = EncColorScheme.Day; }
            else if ((bool)radDusk.IsChecked) { globalMarinerSettings.ColorScheme = EncColorScheme.Dusk; }
            else if ((bool)radNight.IsChecked) { globalMarinerSettings.ColorScheme = EncColorScheme.Night; }

            // エリア表示設定
            if ((bool)radAreaPlain.IsChecked) { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain; }
            else { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized; }

            // ポイント表示設定
            if ((bool)radPointPaper.IsChecked) { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart; }
            else { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified; }
        }
        #endregion


        #region 個別属性表示
        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // 既にフィーチャが選択状態の場合は、選択を解除する
            foreach (EncLayer layer in _mainMapView.Map.OperationalLayers.OfType<EncLayer>())
            {
                layer.ClearSelection();
            }

            // コールアウトを非表示にする
            _mainMapView.DismissCallout();

            // マップ上をクリックした地点にあるフィーチャを取得する
            IReadOnlyList<IdentifyLayerResult> results = await _mainMapView.IdentifyLayersAsync(e.Position, 10, false);
            // 該当するフィーチャが無い場合
            if (results.Count < 1) { return; }

            // 取得されたフィーチャが含まれるレイヤーのリストが返されるので、ENCレイヤーのみを取得する
            IEnumerable<IdentifyLayerResult> encResults = results.Where(result => result.LayerContent is EncLayer);
            IEnumerable<IdentifyLayerResult> encResultsWithFeatures = encResults.Where(result => result.GeoElements.Count > 0);

            // ENCレイヤーのリストから最上位のレイヤーを取得する
            IdentifyLayerResult firstResult = encResultsWithFeatures.First();
            EncLayer containingLayer = firstResult.LayerContent as EncLayer;

            // レイヤーに含まれるフィーチャ リストから最後のフィーチャを取得する
            EncFeature firstFeature = firstResult.GeoElements.Last() as EncFeature;

            // 取得したフィーチャを選択（ハイライト表示）する
            containingLayer.SelectFeature(firstFeature);

            // フィーチャの関連情報を取得を文字列に変換する
            var attributes = new System.Text.StringBuilder();
            attributes.AppendLine(firstFeature.Description);

            if (firstFeature.Attributes.Count > 0)
            {
                // フィーチャの属性（key:属性のフィールド名、value:属性のフィールド値のペア）を取得する
                foreach (var attribute in firstFeature.Attributes)
                {
                    var fieldName = attribute.Key;
                    var fieldValue = attribute.Value;
                    attributes.AppendLine(fieldName + ": " + fieldValue);
                }
                attributes.AppendLine();

            }

            // ENCフィーチャの頭文字と関連情報を指定してコールアウトを作成する
            CalloutDefinition definition = new CalloutDefinition(firstFeature.Acronym, attributes.ToString());
            // コールアウトをマップ上でクリックした場所に表示する
            _mainMapView.ShowCalloutAt(e.Location, definition);
        }
        #endregion



        #region ENC レイヤー追加
        private async void _addEncButton_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {

                foreach (string strFilePath in dialog.FileNames)
                {
                    // ファイルパスからファイル名を取得
                    string strFileName = System.IO.Path.GetFileName(strFilePath);

                    EncLayer myEncLayer = new EncLayer(new EncCell(strFilePath));
                    myEncLayer.Name = strFileName;

                    try
                    {
                        await myEncLayer.LoadAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                    finally
                    {
                        if (myEncLayer.LoadStatus == LoadStatus.FailedToLoad)
                        {
                            MessageBox.Show("ファイルの形式が無効です:" + strFileName);
                        }
                        else if (myEncLayer.LoadStatus == LoadStatus.Loaded)
                        {
                            // マップにレイヤーを追加
                            _mainMapView.Map.OperationalLayers.Add(myEncLayer);

                            // マップの表示位置をレイヤーの全体領域に設定
                            _mainMapView.SetViewpoint(new Viewpoint(myEncLayer.FullExtent));

                            // レイヤーリストを更新
                            _legend.ItemsSource = _mainMapView.Map.OperationalLayers.Reverse();
                        }
                    }

                }

                MessageBox.Show("ファイルの読み込みが完了しました。");

            }


        }
        #endregion


        #region 追加レイヤーの削除
        private void _allClearButton_Click(object sender, RoutedEventArgs e)
        {
            // 既存のENCレイヤーを削除
            var encLayers = _mainMapView.Map.OperationalLayers.OfType<EncLayer>().ToList();
            foreach (var l in encLayers)
            {
                _mainMapView.Map.OperationalLayers.Remove(l);
            }

            // レイヤーリストを更新
            _legend.ItemsSource = _mainMapView.Map.OperationalLayers.Reverse();
        }
        #endregion


        #region 背景地図変更
        private void _changeBaseMapRadioButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 現在の背景地図を削除
                _mainMapView.Map.Basemap.BaseLayers.RemoveAt(0);


                var radioButton = sender as RadioButton;
                if (radioButton.Tag.ToString() == "imagery")
                {
                    _mainMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISImagery);

                }
                else if (radioButton.Tag.ToString() == "streets")
                {
                    _mainMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISStreets);

                }
                else if (radioButton.Tag.ToString() == "topographic")
                {
                    _mainMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISTopographic);

                }
                else if (radioButton.Tag.ToString() == "oceans")
                {
                    _mainMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISOceans);

                }
                else if (radioButton.Tag.ToString() == "osm")
                {
                    _mainMapView.Map.Basemap = new Basemap(BasemapStyle.OSMStandard);

                }
                else if (radioButton.Tag.ToString() == "gsi")
                {
                    var webTiledLayer = new WebTiledLayer() { TemplateUri = "https://cyberjapandata.gsi.go.jp/xyz/std/{level}/{col}/{row}.png" };
                    _mainMapView.Map.Basemap.BaseLayers.Insert(0, webTiledLayer);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("背景地図の変更に失敗しました: " + ex.Message);
            }
        }
        #endregion


        #region 地図の画像出力
        private async void _printButton_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new SaveFileDialog();
            dialog.Title = "JPEGファイルの保存";
            dialog.AddExtension = true;
            dialog.Filter = "jpg画像|*.jpg;*.jpeg";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // マップの現在の状態をJPEG画像として出力して、指定のパスに保存する
                    ImageSource image = await RuntimeImageExtensions.ToImageSourceAsync(await _mainMapView.ExportImageAsync());
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image));
                    FileStream stream = new FileStream(dialog.FileName, FileMode.Create);
                    encoder.Save(stream);
                    stream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
        #endregion

        #region 作図の開始・停止
        private async void _startSketchButton_Click(object sender, RoutedEventArgs e)
        {

            // フリーハンドでの作図（スケッチ モード）を開始する
            if (_startSketchButton.Content.ToString() == "作図開始") {
                _startSketchButton.Content = "作図停止";

                SketchCreationMode creationMode = SketchCreationMode.FreehandLine;
                try
                {
                    await _mainMapView.SketchEditor.StartAsync(creationMode, true);
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {

                }

            } else
            {
                // スケッチ モードを停止する
                _startSketchButton.Content = "作図開始";
                if (_mainMapView.SketchEditor.CancelCommand.CanExecute(null))
                {
                    _mainMapView.SketchEditor.CancelCommand.Execute(null);   
                }
            }

        }
        #endregion
    }
}