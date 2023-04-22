using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Actors;

public class ActorGrouping
{
    private readonly string _name;

    public ActorGrouping(string name)
    {
        _name = name;
    }

    public string Name => $"{_name} ({ActorCollections.Count})";
    public ObservableCollectionEx<ActorCollection> ActorCollections { get; } = new();
}