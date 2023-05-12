namespace SOTFEdit.Infrastructure.Companion;

public record CustomPoi(
    int Id,
    string Title,
    string? Description,
    string? ScreenshotFile,
    float X,
    float Y,
    float Z,
    int AreaMask
);