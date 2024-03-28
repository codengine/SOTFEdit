using System.Windows.Media;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Actors;

// ReSharper disable once ClassNeverInstantiated.Global
public record ActorType(int Id, string Classification, string? Gender = null, string? Image = null, string? Icon = null)
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

    public string IconPath => Icon != null ? $"/images/actors/icons/{Icon}" : "/images/icons/question-mark.png";

    public string Name => Id == -1 ? "???" : TranslationManager.Get("actors.types." + Id);

    public string PrintableClassification =>
        TranslationManager.Get("actors.classification." + Classification);

    public string PrintableGender =>
        Gender == null ? "-" : TranslationManager.Get("actors.gender." + Gender);

    public virtual bool Equals(ActorType? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public bool IsFollower()
    {
        return Id is Constants.Actors.KelvinTypeId or Constants.Actors.VirginiaTypeId;
    }
}

internal record EmptyActorType() : ActorType(-1, "")
{
    public new string Name => "";
}