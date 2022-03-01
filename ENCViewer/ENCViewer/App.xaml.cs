using ENCViewer.Services;
using ENCViewer.ViewModels;
using ENCViewer.Views;
using Prism.Ioc;
using System.Windows;

namespace ENCViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            containerRegistry.RegisterDialog<ChangeBasemap, ChangeBasemapViewModel>();
            containerRegistry.RegisterDialog<LayerList, LayerListViewModel>();
            containerRegistry.RegisterDialog<EncSetting, EncSettingViewModel>();
            containerRegistry.RegisterDialog<Rotate, RotateViewModel>();
            containerRegistry.RegisterDialog<Message, MessageViewModel>();
            containerRegistry.RegisterSingleton<ICommonDialogService, CommonDialogService>();
        }


    }
}
