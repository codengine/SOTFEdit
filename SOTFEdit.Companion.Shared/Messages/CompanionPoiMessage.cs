using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionPoiMessage : ICompanionMessage
{
    public CompanionPoiMessage(string title, string? description, float x, float y, float z, int areaMask,
        string? screenshotPath)
    {
        Title = title;
        Description = description;
        X = x;
        Y = y;
        Z = z;
        AreaMask = areaMask;
        ScreenshotPath = screenshotPath;
    }

    [Key(0)]
    public string Title { get; init; }

    [Key(1)]
    public string? Description { get; init; }

    [Key(2)]
    public float X { get; init; }

    [Key(3)]
    public float Y { get; init; }

    [Key(4)]
    public float Z { get; init; }

    [Key(5)]
    public int AreaMask { get; init; }

    [Key(6)]
    public string? ScreenshotPath { get; init; }
}