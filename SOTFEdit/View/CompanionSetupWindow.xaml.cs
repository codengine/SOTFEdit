using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class CompanionSetupWindow : ICloseable
{
    public CompanionSetupWindow(Window parent)
    {
        DataContext = BuildViewModel();
        Owner = parent;
        InitializeComponent();
    }

    private CompanionSetupWindowViewModel BuildViewModel()
    {
        var connectionManager = Ioc.Default.GetRequiredService<CompanionConnectionManager>();
        var viewModel = new CompanionSetupWindowViewModel(connectionManager, this)
        {
            Address = Settings.Default.CompanionAddress,
            Port = Settings.Default.CompanionPort,
            ConnectTimeout = Settings.Default.CompanionConnectTimeout,
            KeepAliveInterval = Settings.Default.CompanionKeepAliveInterval,
            MapPositionUpdateInterval = Settings.Default.CompanionMapPositionUpdateInterval
        };
        return viewModel;
    }
}