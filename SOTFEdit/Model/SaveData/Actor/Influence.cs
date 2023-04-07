using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.SaveData.Actor;

public partial class Influence : ObservableObject
{
    [ObservableProperty] private float? _anger;
    [ObservableProperty] private float? _fear;
    [ObservableProperty] private float? _sentiment;
    public string TypeId { get; init; }
    public static IEnumerable<string> AllTypes => new[] { "Player", "Cannibal", "Creepy" };
    public string PrintableTypeId => TranslationManager.GetString("InfluenceType_", TypeId);

    public static Influence AsFillerWithDefaults(string typeId)
    {
        return new Influence
        {
            Anger = 0,
            Fear = 0,
            Sentiment = 0,
            TypeId = typeId
        };
    }

    public static class Type
    {
        public const string Player = "Player";
        public const string Cannibal = "Cannibal";
        public const string Creepy = "Creepy";
    }
}