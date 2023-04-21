using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.SaveData.Storage.Module;
using SOTFEdit.Model.Storage;

namespace SOTFEdit.Model;

public class Item
{
    private string? _normalizedLowercaseName;
    public int Id { get; init; }
    public string Name => TranslationManager.Get("items." + Id);
    public string Type { get; init; }
    public FoodSpoilModuleDefinition? FoodSpoilModuleDefinition { get; init; }
    public SourceActorModuleDefinition? SourceActorModuleDefinition { get; init; }
    public bool IsInventoryItem { get; init; } = true;
    public bool IsEquippableArmor { get; init; } = false;
    public bool IsWearableCloth { get; init; } = false;
    public DefaultMinMax? Durability { get; init; }
    public StorageMax? StorageMax { get; init; }
    public string? Image { get; init; }

    public BitmapImage? ThumbnailMedium => GetThumbImage();

    public BitmapImage? ThumbnailBig => GetImage();

    public string NormalizedLowercaseName
    {
        get { return _normalizedLowercaseName ??= TranslationHelper.Normalize(Name).ToLower(); }
    }

    private BitmapImage? GetThumbImage()
    {
        if (Image == null)
        {
            return null;
        }

        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "items",
            $"{Path.GetFileNameWithoutExtension(Image)}_tn{Path.GetExtension(Image)}");
        if (!File.Exists(imagePath))
        {
            return GetImage(48, 48);
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(imagePath);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private BitmapImage? GetImage(int? width = null, int? height = null)
    {
        if (Image == null)
        {
            return null;
        }

        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "items", Image);
        if (!File.Exists(imagePath))
        {
            return null;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(imagePath);

        if (width is { } theWidth)
        {
            image.DecodePixelWidth = theWidth;
        }

        if (height is { } theHeight)
        {
            image.DecodePixelHeight = theHeight;
        }

        image.EndInit();
        image.Freeze();
        return image;
    }

    private bool Equals(Item other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Item)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public bool HasModules()
    {
        return FoodSpoilModuleDefinition != null;
    }
}

public record ItemModule(int ModuleId);

// ReSharper disable once ClassNeverInstantiated.Global
public record FoodSpoilModuleDefinition(int DefaultVariant, List<int> Variants) : ItemModule(3)
{
    public FoodSpoilStorageModule BuildNewModuleWithDefaults()
    {
        return new FoodSpoilStorageModule(ModuleId, DefaultVariant);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public record SourceActorModuleDefinition(int DefaultSourceActorType) : ItemModule(1)
{
    public SourceActorStorageModule BuildNewModuleWithDefaults()
    {
        return new SourceActorStorageModule(DefaultSourceActorType);
    }
}