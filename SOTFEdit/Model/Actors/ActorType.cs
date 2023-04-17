using System.Windows.Media;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Actors;

// ReSharper disable once ClassNeverInstantiated.Global
public record ActorType(int Id, string Classification, string? Gender, string? Image)
{
    public Color ActorColor
    {
        get
        {
            return Classification switch
            {
                "animal" => Colors.LightGreen,
                "creep" => Colors.DarkOrange,
                "cannibal" => Gender switch
                {
                    "male" => Colors.Red,
                    "female" => Colors.HotPink,
                    _ => Colors.DodgerBlue
                },
                _ => Colors.White
            };
        }
    }

    public string Name => TranslationManager.Get("actors.types." + Id);

    public string PrintableClassification =>
        TranslationManager.Get("actors.classification." + Classification);

    public string PrintableGender =>
        Gender == null ? "-" : TranslationManager.Get("actors.gender." + Gender);
}