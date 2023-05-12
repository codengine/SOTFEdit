using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Model.Events;

namespace SOTFEdit.Model.Map;

public partial class CustomMapPoi : DefaultGenericInformationalPoi
{
    private const string IconFile = "star.png";

    private readonly string _screenshotDirectory;

    private CustomMapPoi(int id, Position teleport, string title, string? description, string screenshotDirectory,
        string? screenshot) : base(teleport.X, teleport.Z, teleport, title, description, screenshot, IconFile, null,
        teleport.Area.IsUnderground())
    {
        _screenshotDirectory = screenshotDirectory;
        Id = id;
    }

    public int Id { get; }

    public static BitmapImage CategoryIcon => LoadBaseIcon(IconFile, 24, 24);

    protected override string GetScreenshotPath(string screenshot)
    {
        return Path.Combine(_screenshotDirectory, screenshot);
    }

    [RelayCommand]
    private void Delete()
    {
        var evt = new DeleteCustomPoiEvent(Id);
        PoiMessenger.Instance.Send(evt);
        WeakReferenceMessenger.Default.Send(evt);
    }

    public static CustomMapPoi FromCustomPoi(CustomPoi customPoi, AreaMaskManager areaMaskManager)
    {
        var pos = new Position(customPoi.X, customPoi.Y, customPoi.Z)
        {
            Area = areaMaskManager.GetAreaForAreaMask(customPoi.AreaMask)
        };

        var poi = new CustomMapPoi(customPoi.Id, pos, customPoi.Title, customPoi.Description,
            CompanionPoiStorage.DirectoryPath, customPoi.ScreenshotFile);
        poi.SetEnabledNoRefresh(true);
        return poi;
    }

    public override void GetTeleportationOffset(out float xOffset, out float yOffset, out float zOffset)
    {
        xOffset = 0;
        yOffset = 0;
        zOffset = 0;
    }
}