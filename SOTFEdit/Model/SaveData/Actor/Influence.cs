using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.SaveData.Actor;

public partial class Influence : ObservableObject
{
    [ObservableProperty]
    private float _anger;

    [ObservableProperty]
    private float _fear;

    [ObservableProperty]
    private float _sentiment;

    public string TypeId { get; init; }

    [JsonIgnore]
    public static IEnumerable<string> AllTypes => new[] { Type.Player, Type.Cannibal, Type.Creepy };

    [JsonIgnore]
    public string PrintableTypeId => TranslationManager.Get("actors.influenceType." + TypeId);

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