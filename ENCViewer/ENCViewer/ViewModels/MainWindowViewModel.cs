using ENCViewer.Views;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ENCViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;

        private string _title = "ENC Viewer";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private Map _map;
        public Map Map
        {
            get { return _map; }
            set { SetProperty(ref _map, value); }

        }

        private string _sketchButtonText = "作図開始";
        public string SketchButtonText
        {
            get { return _sketchButtonText; }
            set { SetProperty(ref _sketchButtonText, value); }

        }

        private SketchEditor _sketchEditor;
        public SketchEditor SketchEditor
        {
            get { return _sketchEditor; }
            set { SetProperty(ref _sketchEditor, value); }
        }

        private Viewpoint _viewpoint;
        public Viewpoint Viewpoint
        {
            get { return _viewpoint; }
            set { SetProperty(ref _viewpoint, value); }
        }

        private Viewpoint _updatedViewpoint;
        public Viewpoint UpdatedViewpoint
        {
            get { return _updatedViewpoint; }
            set { SetProperty(ref _updatedViewpoint, value); }
        }

        private double _rotate;
        public double Rotate
        {
            get { return _rotate; }
            set { SetProperty(ref _rotate, value); }
        }

        public ICommand ZoomCommand { get; private set; }
        public ICommand UpdateViewpointCommand { get; private set; }
        public DelegateCommand ShowViewChangeBasemapButton { get; }
        public DelegateCommand ShowViewLayerListButton { get; }
        public DelegateCommand ShowViewEncSettingButton { get; }
        public DelegateCommand ShowViewRotateButton { get; }
        public DelegateCommand AddEncFileButton { get; }
        public DelegateCommand DeleteEncFileButton { get; }
        public DelegateCommand StartSketchButton { get; }
        public DelegateCommand PrintButton { get; }


        public MainWindowViewModel(IDialogService dialogService, IEventAggregator eventAggregator)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;

            SketchEditor = new SketchEditor();

            ShowViewChangeBasemapButton = new DelegateCommand(ShowViewChangeBasemapButtonExecute);
            ShowViewLayerListButton = new DelegateCommand(ShowViewLayerListButtonExecute);
            ShowViewRotateButton = new DelegateCommand(ShowViewRotateButtonExecute);
            ShowViewEncSettingButton = new DelegateCommand(ShowViewEncSettingButtonExecute);
            AddEncFileButton = new DelegateCommand(AddEncFileButtonExecuteAsync);
            DeleteEncFileButton = new DelegateCommand(DeleteEncFileButtonExecuteAsync);
            StartSketchButton = new DelegateCommand(StartSketchButtonExecuteAsync);
            ZoomCommand = new DelegateCommand<Viewpoint>(ZoomAction);
            UpdateViewpointCommand = new DelegateCommand<Viewpoint>(UpdateViewpointAction);

            SetupMap();
        }

        private void SetupMap()
        {
            ArcGISRuntimeEnvironment.ApiKey = "APIキーを入力";
            Map = new Map(BasemapStyle.ArcGISStreets);
            //Map = new Map(Basemap.CreateStreetsVector());
        }



        private void ZoomAction(Viewpoint vp)
        {
            //Viewpoint = new Viewpoint(4, -74, 5000000);
            Viewpoint = vp;
        }

        private void UpdateViewpointAction(Viewpoint vp)
        {
            var projectedVp = new Viewpoint(GeometryEngine.Project(vp.TargetGeometry, SpatialReferences.Wgs84), vp.Camera);
            if (UpdatedViewpoint == null || projectedVp.ToJson() != UpdatedViewpoint.ToJson())
            {
                UpdatedViewpoint = projectedVp;
            }
        }

        private void ShowViewChangeBasemapButtonExecute()
        {
            //var p = new DialogParameters();
            //p.Add(nameof(ChangeBasemapViewModel.Title), this.Title);
            _dialogService.ShowDialog(nameof(ChangeBasemap), null, null);
        }

        private void ShowViewLayerListButtonExecute()
        {
            _dialogService.ShowDialog(nameof(LayerList), null, null);
        }

        private void ShowViewEncSettingButtonExecute()
        {
            _dialogService.ShowDialog(nameof(EncSetting), null, null);
        }

        private void ShowViewRotateButtonExecute()
        {
            _dialogService.ShowDialog(nameof(Rotate), null, null);
        }



        private async void AddEncFileButtonExecuteAsync()
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
                            Map.OperationalLayers.Add(myEncLayer);

                            // マップの表示位置をレイヤーの全体領域に設定
                            ZoomAction(new Viewpoint(myEncLayer.FullExtent));

                            // レイヤーリストを更新
                            //_legend.ItemsSource = Map.OperationalLayers.Reverse();
                        }
                    }

                }

                MessageBox.Show("ファイルの読み込みが完了しました。");

            }
        }

        private void DeleteEncFileButtonExecuteAsync()
        {
            // 既存のENCレイヤーを削除
            var encLayers = Map.OperationalLayers.OfType<EncLayer>().ToList();
            foreach (var l in encLayers)
            {
                Map.OperationalLayers.Remove(l);
            }

            // レイヤーリストを更新
            //_legend.ItemsSource = Map.OperationalLayers.Reverse();
        }


        private async void StartSketchButtonExecuteAsync()
        {
            // フリーハンドでの作図（スケッチ モード）を開始する
            if (SketchButtonText.ToString() == "作図開始")
            {
                SketchButtonText = "作図停止";

                SketchCreationMode creationMode = SketchCreationMode.FreehandLine;
                try
                {
                    SketchEditor = new SketchEditor();
                    await SketchEditor.StartAsync(creationMode, true);
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

            }
            else
            {
                // スケッチ モードを停止する
                SketchButtonText = "作図開始";
                if (SketchEditor.CancelCommand.CanExecute(null))
                {
                    SketchEditor.CancelCommand.Execute(null);
                }
            }

        }
    


        public CalloutDefinition HandleTap(IReadOnlyList<IdentifyLayerResult> results)
        {

            // 既にフィーチャが選択状態の場合は、選択を解除する
            foreach (EncLayer layer in Map.OperationalLayers.OfType<EncLayer>())
            {
                layer.ClearSelection();
            }

            // 該当するフィーチャが無い場合
            if (results.Count < 1) { return null; }

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
            return definition;
        }



        public void PrintImage(ImageSource image)
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


    }
}
