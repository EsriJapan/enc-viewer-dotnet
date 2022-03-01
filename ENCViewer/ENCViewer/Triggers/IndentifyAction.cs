using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Microsoft.Xaml.Behaviors;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Hydrography;
using Prism.Events;
using ENCViewer.Services;

namespace ENCViewer.Triggers
{

    public class IndentifyAction : TriggerAction<MapView>
    {
        private static bool _doubleTapped = false;

        protected override async void Invoke(object parameter)
        {

            await Task.Delay(250);
            if (_doubleTapped)
            {
                _doubleTapped = false;
                return;
            }

            if (!(AssociatedObject is MapView mapView)) return;
            if (!(parameter is GeoViewInputEventArgs args)) return;


            if (Map != null)
            {

                Mouse.OverrideCursor = Cursors.Wait;

                IReadOnlyList<IdentifyLayerResult> results = await mapView.IdentifyLayersAsync(args.Position, 10, false);

                foreach (EncLayer layer in Map.OperationalLayers.OfType<EncLayer>())
                {
                    layer.ClearSelection();
                }

                // 該当するフィーチャが無い場合
                if (results.Count < 1)
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    Messenger.Instance.GetEvent<PubSubEvent<string>>().Publish("HideCallout");
                    return;
                }
            
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

                Mouse.OverrideCursor = Cursors.Arrow;

                // コールアウトをマップ上でクリックした場所に表示する
                Messenger.Instance.GetEvent<PubSubEvent<IdentifyEventArgs>>().Publish(new IdentifyEventArgs(args.Location, definition));


                return;
            }


        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (!(AssociatedObject is MapView mapView)) return;

            //GeoViewDoubleTappedはタップとダブルタップの両方で実行される
            mapView.GeoViewDoubleTapped += (s, e) => { _doubleTapped = true; };
        }


        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
            "Map", typeof(Map), typeof(IndentifyAction), new PropertyMetadata(default(Map)));

        public Map Map
        {
            get => (Map)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

    }
}