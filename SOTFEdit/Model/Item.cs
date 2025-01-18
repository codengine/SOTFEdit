using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.SaveData.Storage.Module;
using SOTFEdit.Model.Storage;

namespace SOTFEdit.Model;

public class Item : ICloneable
{
    private string? _normalizedLowercaseName;
    private string? _normalizedLowercaseType;
    public int Id { get; init; }
    public string Name => TranslationManager.Get("items." + Id);
    public string Type { get; init; }
    public FoodSpoilModuleDefinition? FoodSpoilModuleDefinition { get; init; }
    public SourceActorModuleDefinition? SourceActorModuleDefinition { get; init; }
    public bool IsInventoryItem { get; init; } = true;
    public bool IsEquippableArmor { get; init; } = false;
    public bool IsWearableCloth { get; init; } = false;
    public bool IsPlatable { get; init; } = false;
    public DefaultMinMax? Durability { get; init; }
    public StorageMax? StorageMax { get; set; }
    public HashSet<int>? ModHashes { get; init; }
    public string? Image { get; init; }
    public string? Wiki { get; init; }

    public BitmapImage? ThumbnailMedium => GetThumbImage();

    public BitmapImage? ThumbnailBig => GetImage();

    public string NormalizedLowercaseName
    {
        get { return _normalizedLowercaseName ??= TranslationHelper.Normalize(Name).ToLower(); }
    }

    private string NormalizedLowercaseType
    {
        get { return _normalizedLowercaseType ??= TranslationHelper.Normalize(Type).ToLower(); }
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    private BitmapImage? GetThumbImage()
    {
        return GetThumbPath()?.LoadAppLocalImage();
    }

    private string? GetThumbPath()
    {
        return Image == null ? null : "/images/items/" + Image.ExtendFilenameWith("_tn");
    }

    private BitmapImage? GetImage()
    {
        return Image == null ? null : $"/images/items/{Image}".LoadAppLocalImage();
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

    public bool HasFoodSpoilModuleDefinition()
    {
        return FoodSpoilModuleDefinition != null;
    }

    public bool HasSourceActorModuleDefinition()
    {
        return SourceActorModuleDefinition != null;
    }

    public bool Matches(string normalizedLowercaseFullText)
    {
        return NormalizedLowercaseName.Contains(normalizedLowercaseFullText) ||
               NormalizedLowercaseType.Contains(normalizedLowercaseFullText);
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