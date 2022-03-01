using ENCViewer.Services;
using ENCViewer.Views;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ENCViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;
        private ICommonDialogService _commonDialogService;
        private DialogParameters _dialogParameters;

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

        public ICommand ZoomCommand { get; private set; }
        public ICommand UpdateViewpointCommand { get; private set; }
        public DelegateCommand ShowViewChangeBasemapButton { get; }
        public DelegateCommand ShowViewLayerListButton { get; }
        public DelegateCommand ShowViewEncSettingButton { get; }
        public DelegateCommand ShowViewRotateButton { get; }
        public DelegateCommand AddEncFileButton { get; }
        public DelegateCommand DeleteEncFileButton { get; }
        public DelegateCommand StartSketchButton { get; }
        public DelegateCommand PrintMapViewButton { get; }


        public MainWindowViewModel(IDialogService dialogService, IEventAggregator eventAggregator, ICommonDialogService commonDialogService)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            _commonDialogService = commonDialogService;

            ShowViewChangeBasemapButton = new DelegateCommand(ShowViewChangeBasemapButtonExecute);
            ShowViewLayerListButton = new DelegateCommand(ShowViewLayerListButtonExecute);
            ShowViewRotateButton = new DelegateCommand(ShowViewRotateButtonExecute);
            ShowViewEncSettingButton = new DelegateCommand(ShowViewEncSettingButtonExecute);
            AddEncFileButton = new DelegateCommand(AddEncFileButtonExecuteAsync);
            DeleteEncFileButton = new DelegateCommand(DeleteEncFileButtonExecuteAsync);
            StartSketchButton = new DelegateCommand(StartSketchButtonExecuteAsync);
            PrintMapViewButton = new DelegateCommand(PrintMapViewButtonExecuteAsync);

            ZoomCommand = new DelegateCommand<Viewpoint>(ZoomAction);
            UpdateViewpointCommand = new DelegateCommand<Viewpoint>(UpdateViewpointAction);


            SketchEditor = new SketchEditor();
            SetupMap();

        }


        private void SetupMap()
        {
            ArcGISRuntimeEnvironment.ApiKey = ConfigurationManager.AppSettings["APIKey"];
            Map = new Map(BasemapStyle.ArcGISStreets);
            //Map = new Map(Basemap.CreateStreetsVector());
            Map.Basemap.Name = "ArcGISStreets";
        }



        private void ZoomAction(Viewpoint vp)
        {
            Viewpoint = vp;
        }

        private void UpdateViewpointAction(Viewpoint vp)
        {
            // 初期表示位置
            if (Viewpoint == null) Viewpoint = new Viewpoint(35.6809591, 139.7673068, 100000000);

            var projectedVp = new Viewpoint(GeometryEngine.Project(vp.TargetGeometry, SpatialReferences.Wgs84), vp.Camera);
            if (UpdatedViewpoint == null || projectedVp.ToJson() != UpdatedViewpoint.ToJson())
            {
                UpdatedViewpoint = projectedVp;
            }
        }


        private void ShowViewChangeBasemapButtonExecute()
        {
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
            var dialogSettings = new OpenFileDialogSettings();
            dialogSettings.Multiselect = true;


            // ダイアログを表示する
            if (_commonDialogService.ShowDialog(dialogSettings) == true)
            {

                foreach (string strFilePath in dialogSettings.FileNames)
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
                        _dialogParameters = new DialogParameters();
                        _dialogParameters.Add(nameof(MessageViewModel.MessageLabel), ex.Message);
                        _dialogService.ShowDialog(nameof(Message), _dialogParameters, null);
                        return;
                    }
                    finally
                    {
                        if (myEncLayer.LoadStatus == LoadStatus.FailedToLoad)
                        {
                            _dialogParameters = new DialogParameters();
                            _dialogParameters.Add(nameof(MessageViewModel.MessageLabel), "ファイルの形式が無効です:" + strFileName);
                            _dialogService.ShowDialog(nameof(Message), _dialogParameters, null);
                        }
                        else if (myEncLayer.LoadStatus == LoadStatus.Loaded)
                        {
                            // マップにレイヤーを追加
                            Map.OperationalLayers.Add(myEncLayer);

                            // マップの表示位置をレイヤーの全体領域に設定
                            ZoomAction(new Viewpoint(myEncLayer.FullExtent));
                        }
                    }

                }

                _dialogParameters = new DialogParameters();
                _dialogParameters.Add(nameof(MessageViewModel.MessageLabel), "ファイルの読み込みが完了しました。");
                _dialogService.ShowDialog(nameof(Message), _dialogParameters, null);

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
                    _dialogParameters = new DialogParameters();
                    _dialogParameters.Add(nameof(MessageViewModel.MessageLabel), ex.Message);
                    _dialogService.ShowDialog(nameof(Message), _dialogParameters, null);
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

        private void PrintMapViewButtonExecuteAsync()
        {
            Messenger.Instance.GetEvent<PubSubEvent<string>>().Publish("DoPrint");
        }


        public void HandlePrint()
        {
            Console.WriteLine("HandlePrint");
        }


    }
}
