using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using Semver;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;

namespace SOTFEdit;

public class UpdateChecker
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private volatile bool _isLoading;
    private readonly string _latestTagUrl;
    private readonly HttpClient _client;
    private readonly SemVersion _assemblyVersion;

    public UpdateChecker(GameData gameData)
    {
        _latestTagUrl = gameData.Config.LatestTagUrl;
        var assemblyInfo = Assembly.GetExecutingAssembly()
            .GetName();
        _assemblyVersion = SemVersion.FromVersion(assemblyInfo.Version);

        _client = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", $"{assemblyInfo.Name}/{_assemblyVersion}" }
            }
        };
    }

    public void CheckForUpdates(bool notifyOnSameVersion, bool notifyOnError)
    {
        Task.Run(() => DoCheckForUpdates(notifyOnSameVersion, notifyOnError));
    }

    private async Task DoCheckForUpdates(bool notifyOnSameVersion, bool notifyOnError)
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, _latestTagUrl);

            using var result = await _client.SendAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                Logger.Warn($"Got a failure response: {result.StatusCode}");
                NotifyOnError(notifyOnError);
                return;
            }

            var json = await result.Content.ReadAsStringAsync();

            var doc = JToken.Parse(json);
            if (doc["tag_name"]?.ToObject<string>() is not { } tagName)
            {
                Logger.Warn("Tag version not found in response");
                NotifyOnError(notifyOnError);
                return;
            }

            var latestTagVersion = SemVersion.Parse(tagName, SemVersionStyles.AllowV);
            switch (latestTagVersion.ComparePrecedenceTo(_assemblyVersion))
            {
                case 1:
                    var link = doc["html_url"]?.ToObject<string>();
                    NotifyOnNewerVersion(latestTagVersion, link);
                    break;
                case 0 when notifyOnSameVersion:
                    NotifyOnSameVersion(latestTagVersion);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error while checking for updates");
            NotifyOnError(notifyOnError);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static void NotifyOnSameVersion(SemVersion latestTagVersion)
    {
        Logger.Info("No update found");
        SendResult(new VersionCheckResultEvent(latestTagVersion, false));
    }

    private static void NotifyOnNewerVersion(SemVersion latestTagVersion, string? link)
    {
        Logger.Info($"Newer version found ({latestTagVersion})");
        SendResult(new VersionCheckResultEvent(latestTagVersion, true)
        {
            Link = link
        });
    }

    private static void NotifyOnError(bool notifyOnError)
    {
        if (notifyOnError)
        {
            SendResult(new VersionCheckResultEvent(null, false, true));
        }
    }

    private static void SendResult(VersionCheckResultEvent checkResult)
    {
        _ = Task.Run(() => WeakReferenceMessenger.Default.Send(checkResult));
    }
}