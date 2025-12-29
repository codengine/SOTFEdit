using MessagePack;

namespace SOTFEdit.Companion.Shared.Messages;

[MessagePackObject]
public class CompanionPoiMessage(
    string title, string? description, float x, float y, float z, int areaMask,
    string? screenshotPath)
    : ICompanionMessage
{
    [Key(0)] public string Title { get; init; } = title;

    [Key(1)] public string? Description { get; init; } = description;

    [Key(2)] public float X { get; init; } = x;

    [Key(3)] public float Y { get; init; } = y;

    [Key(4)] public float Z { get; init; } = z;

    [Key(5)] public int AreaMask { get; init; } = areaMask;

    [Key(6)] public string? ScreenshotPath { get; init; } = screenshotPath;
}