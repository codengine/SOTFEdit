using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Infrastructure;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Model.Events;

namespace SOTFEdit.ViewModel;

public partial class CompanionSetupWindowViewModel : ObservableObject
{
    private readonly CompanionConnectionManager _connectionManager;
    private readonly ICloseable _parent;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    private string? _address;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    private short? _connectTimeout;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    private short? _keepAliveInterval;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    private decimal? _mapPositionUpdateInterval;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    private int? _port;

    [ObservableProperty]
    private string _statusText = "";

    public CompanionSetupWindowViewModel(CompanionConnectionManager connectionManager, ICloseable parent)
    {
        _parent = parent;
        _connectionManager = connectionManager;
        OnCompanionConnectionStatusEvent(connectionManager.Status);

        SetupListeners();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<CompanionConnectionStatusEvent>(this,
            (_, message) => OnCompanionConnectionStatusEvent(message.Status));
    }

    private void OnCompanionConnectionStatusEvent(CompanionConnectionManager.ConnectionStatus status)
    {
        StatusText = CompanionConnectionManager.GetStatusText(status);
    }

    [RelayCommand]
    private static void OpenUrl(string url)
    {
        WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl(url));
    }

    private bool CanSave()
    {
        return !string.IsNullOrEmpty(Address) && Port > 0 && ConnectTimeout > 0 && KeepAliveInterval > 0 &&
               MapPositionUpdateInterval > 0;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        Settings.Default.CompanionAddress = Address;
        if (Port is { } port)
        {
            Settings.Default.CompanionPort = port;
        }

        if (ConnectTimeout is { } connectTimeout)
        {
            Settings.Default.CompanionConnectTimeout = connectTimeout;
        }

        if (KeepAliveInterval is { } keepAliveInterval)
        {
            Settings.Default.CompanionKeepAliveInterval = keepAliveInterval;
        }

        var mapPositionUpdateIntervalChanged = false;
        if (MapPositionUpdateInterval is { } mapPositionUpdateInterval)
        {
            if (Math.Abs(mapPositionUpdateInterval - Settings.Default.CompanionMapPositionUpdateInterval) > 0.01M)
            {
                mapPositionUpdateIntervalChanged = true;
            }

            Settings.Default.CompanionMapPositionUpdateInterval = mapPositionUpdateInterval;
        }

        Settings.Default.Save();

        if (mapPositionUpdateIntervalChanged && _connectionManager.IsConnected())
        {
            _connectionManager.SendAsync(new CompanionSettingsMessage
            {
                PositionUpdateFrequency = Settings.Default.CompanionMapPositionUpdateInterval
            });
        }

        _parent.Close();
    }
}