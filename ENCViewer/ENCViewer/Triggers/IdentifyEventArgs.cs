using System;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;


namespace ENCViewer.Triggers
{
    public class IdentifyEventArgs : EventArgs
    {
        internal IdentifyEventArgs(MapPoint mapPoint, CalloutDefinition calloutDefinition)
        {
            MapPoint = mapPoint ?? new MapPoint(0,0);
            CalloutDefinition = calloutDefinition ?? new CalloutDefinition("");

        }
        public MapPoint MapPoint { get; private set; }
        public CalloutDefinition CalloutDefinition { get; private set; }

    }
}