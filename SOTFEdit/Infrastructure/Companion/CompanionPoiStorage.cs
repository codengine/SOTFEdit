using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SOTFEdit.Companion.Shared.Messages;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;

namespace SOTFEdit.Infrastructure.Companion;

public class CompanionPoiStorage
{
    private readonly CustomPois _customPois;
    private readonly string _fullFilePath;

    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };

    private int _nextId = 1;

    public CompanionPoiStorage()
    {
        _fullFilePath = Path.Combine(DirectoryPath, "pois.json");
        if (File.Exists(_fullFilePath))
        {
            _customPois = JsonConverter.DeserializeFromFile<CustomPois>(_fullFilePath, _serializerSettings) ??
                          new CustomPois(new List<CustomPoi>());
            _nextId = FindNextId();
        }
        else
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            _customPois = new CustomPois(new List<CustomPoi>());
        }

        SetupListeners();
    }

    public static string DirectoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customPois");

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<DeleteCustomPoiEvent>(this, (_, message) => Delete(message.Id));
    }

    private int FindNextId()
    {
        if (_customPois.Pois.Count == 0)
        {
            return 1;
        }

        return _customPois.Pois.Select(poi => poi.Id).Max() + 1;
    }

    public IEnumerable<CustomPoi> GetAll()
    {
        return new List<CustomPoi>(_customPois.Pois);
    }

    public void Add(CompanionAddPoiMessage message)
    {
        var screenshotFile = SaveScreenshot(message.Title, message.Screenshot);
        var customPoi = new CustomPoi(_nextId++, message.Title, message.Description, screenshotFile, message.X,
            message.Y,
            message.Z, message.AreaMask);
        _customPois.Pois.Add(customPoi);
        Save();
        var evt = new CustomPoiAddedEvent(customPoi);
        PoiMessenger.Instance.Send(evt);
    }

    private void Save()
    {
        JsonConverter.Serialize(_fullFilePath, _customPois, _serializerSettings);
    }

    private static string SanitizeFileName(string fileName)
    {
        // Define the characters not allowed in a filename
        var invalidChars = Path.GetInvalidFileNameChars();

        // Remove invalid characters from the filename
        var sanitizedFileName = string.Join("", fileName.Split(invalidChars));

        // Convert the filename to lowercase
        sanitizedFileName = sanitizedFileName.ToLower();

        // Replace spaces with underscores
        sanitizedFileName = sanitizedFileName.Replace(' ', '_');

        return sanitizedFileName;
    }

    private static string? SaveScreenshot(string title, byte[] screenshotJpeg)
    {
        if (screenshotJpeg == null || screenshotJpeg.Length == 0)
        {
            return null;
        }

        var baseFilename = SanitizeFileName(title) + ".jpg";
        var path = Path.Combine(DirectoryPath, baseFilename);
        var thumbPath = Path.Combine(DirectoryPath, baseFilename.ExtendFilenameWith("_tn"));

        var fileNumber = 2;
        while (File.Exists(path) || File.Exists(thumbPath))
        {
            // If the file already exists, add the number to the filename
            var numberedFilename = $"{SanitizeFileName(title)}_{fileNumber}.jpg";
            path = Path.Combine(DirectoryPath, numberedFilename);
            thumbPath = Path.Combine(DirectoryPath, numberedFilename.ExtendFilenameWith("_tn"));
            fileNumber++;
        }

        File.WriteAllBytes(path, screenshotJpeg);

        using var imageResizing = new ImageResizing(screenshotJpeg);
        imageResizing.Quality(90)
            .Resize(300, 0)
            .Save(thumbPath);

        return Path.GetFileName(path);
    }

    private void Delete(int id)
    {
        for (var i = 0; i < _customPois.Pois.Count; i++)
        {
            var poi = _customPois.Pois[i];
            if (poi.Id != id)
            {
                continue;
            }

            DeleteScreenshots(poi);
            _customPois.Pois.RemoveAt(i);
            break;
        }

        Save();
        _nextId = FindNextId();
    }

    private static void DeleteScreenshots(CustomPoi poi)
    {
        if (poi.ScreenshotFile is not { } screenshotFile)
        {
            return;
        }

        var filePath = GetFullFilePath(screenshotFile);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var thumbPath = Path.Combine(DirectoryPath, screenshotFile.ExtendFilenameWith("_tn"));
        if (File.Exists(thumbPath))
        {
            File.Delete(thumbPath);
        }
    }

    public static string? GetFullFilePath(string? screenshotFile)
    {
        return screenshotFile == null ? null : Path.Combine(DirectoryPath, screenshotFile);
    }
}