using Prism.Events;

namespace ENCViewer.Services
{
    class Messenger : EventAggregator
    {
        public static Messenger Instance { get; } = new Messenger();
    }
}
