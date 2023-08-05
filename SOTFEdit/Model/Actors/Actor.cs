using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SOTFEdit.Infrastructure;
using SOTFEdit.Model.SaveData.Actor;

namespace SOTFEdit.Model.Actors;

// ReSharper disable once ClassNeverInstantiated.Global
public class Actor
{
    public ActorType? ActorType { get; set; }
    public int? FamilyId { get; set; }
    public List<Influence>? Influences { get; set; }
    public KillStat? KillStats { get; set; }
    public float? LastVisitTime { get; set; }
    public Position? Position { get; set; }
    public int? SpawnerId { get; set; }
    public Spawner? Spawner { get; set; }
    public int State { get; set; }

    [JsonIgnore]
    public string StatePrintable => TranslationManager.Get("actors.state." + State, $"??? ({State})");

    public int VariationId { get; set; }
    public Dictionary<string, float>? Stats { get; set; }
    public int TypeId { get; set; }
    public int UniqueId { get; set; }
    public int OutfitId { get; set; }
    public int? GraphMask { get; set; }

    [JsonIgnore]
    public Color ActorColor => ActorType?.ActorColor ?? Colors.SaddleBrown;

    [JsonIgnore]
    public BitmapImage ActorImage => LoadImage();

    public string PrintableTitle =>
        $"{ActorType?.Name ?? "???"} - UniqueId {UniqueId} - TypeId {TypeId} - FamilyId {FamilyId}";

    private BitmapImage LoadImage()
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri($"pack://application:,,,/SOTFEdit;component/images/actors/{ActorType?.Image}");
        bitmap.EndInit();
        return bitmap;
    }
}