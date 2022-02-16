using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ENCViewer.Behaviors
{
    public class SetMapViewRotateBehavior : Behavior<MapView>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty RotateProperty =
          DependencyProperty.Register(nameof(Rotate), typeof(double), typeof(SetMapViewRotateBehavior));

        /// <summary>
        /// 
        /// </summary>
        public double Rotate
        {
            get { return (double)GetValue(RotateProperty); }
            set { SetValue(RotateProperty, value); }
        }

        public SetMapViewRotateBehavior()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(Rotate))
            {
                SetRotate(this.Rotate);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotate"></param>
        private async void SetRotate(double rotate)
        {
            Debug.WriteLine("SetRotate");
            await this.AssociatedObject.SetViewpointRotationAsync(rotate);
        }

    }

}

