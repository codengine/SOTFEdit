using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MessagePack;
using NLog;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Model.Events;
using WatsonWebsocket;

namespace SOTFEdit.Infrastructure.Companion;

public partial class CompanionConnectionManager : ObservableObject
{
    public enum ConnectionStatus
    {
        Connected,
        Connecting,
        Disconnected
    }

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly CompanionMessageHandler _messageHandler;
    private WatsonWsClient? _client;

    [ObservableProperty]
    private ConnectionStatus _status = ConnectionStatus.Disconnected;

    public CompanionConnectionManager(CompanionMessageHandler messageHandler)
    {
        _messageHandler = messageHandler;
    }

    partial void OnStatusChanged(ConnectionStatus value)
    {
        WeakReferenceMessenger.Default.Send(new CompanionConnectionStatusEvent(value));
    }

    public Task DisconnectAsync()
    {
        if (_client is not { } client || !IsConnected())
        {
            return Task.CompletedTask;
        }

        try
        {
            return client.StopAsync();
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
        finally
        {
            try
            {
                _client.Dispose();
            }
            finally
            {
                _client = null;
                RefreshConnectionStatus();
            }
        }
    }

    public bool IsConnected()
    {
        return _client?.Connected ?? false;
    }

    private void RefreshConnectionStatus()
    {
        Status = _client?.Connected ?? false ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
    }

    public async Task<bool> ConnectAsync()
    {
        if (IsConnected() || Status == ConnectionStatus.Connecting)
        {
            return true;
        }

        Status = ConnectionStatus.Connecting;

        try
        {
            _client?.Dispose();
            _client = BuildClient();
            return await _client.StartWithTimeoutAsync(Settings.Default.CompanionConnectTimeout).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occured while connecting to the companion plugin");
            throw;
        }
        finally
        {
            RefreshConnectionStatus();
        }
    }

    private WatsonWsClient BuildClient()
    {
        var client = new WatsonWsClient(Settings.Default.CompanionAddress, Settings.Default.CompanionPort, false);
        if (Debugger.IsAttached)
        {
            client.Logger = s => Logger.Info(s);
        }

        client.KeepAliveInterval = Settings.Default.CompanionKeepAliveInterval;
        client.MessageReceived += _messageHandler.Handle;
        client.ServerConnected += OnConnected;
        client.ServerDisconnected += OnDisconnected;
        return client;
    }

    private void OnDisconnected(object? sender, EventArgs e)
    {
        Status = ConnectionStatus.Disconnected;
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        Status = ConnectionStatus.Connected;
        SendInstrumentation();
    }

    private void SendInstrumentation()
    {
        var settings = new CompanionSettingsMessage
        {
            PositionUpdateFrequency = Settings.Default.CompanionMapPositionUpdateInterval
        };

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(5000);
                SendAsync(settings);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occured while sending instrumentation info");
            }
        });
    }

    public void SendAsync(ICompanionMessage companionMessage)
    {
        if (!IsConnected())
        {
            return;
        }

        try
        {
            _client?.SendAsync(MessagePackSerializer.Serialize(companionMessage)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"An error occured while sending a message of type {companionMessage.GetType()}");
            RefreshConnectionStatus();
        }
    }

    public static string GetStatusText(ConnectionStatus status)
    {
        return status switch
        {
            ConnectionStatus.Connected => TranslationManager.Get(
                "companion.status.connected"),
            ConnectionStatus.Connecting => TranslationManager.Get(
                "companion.status.connecting"),
            ConnectionStatus.Disconnected => TranslationManager.Get(
                "companion.status.disconnected"),
            _ => ""
        };
    }
}