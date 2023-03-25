using CommunityToolkit.Mvvm.ComponentModel;

namespace SOTFEdit.Model.SaveData.Actor;

public partial class Influence : ObservableObject
{
    [ObservableProperty] private float _anger;
    [ObservableProperty] private float _fear;
    [ObservableProperty] private float _sentiment;
    public string TypeId { get; init; }

    public static class Type
    {
        public const string Player = "Player";
        public const string Cannibal = "Cannibal";
        public const string Creepy = "Creepy";
    }
}