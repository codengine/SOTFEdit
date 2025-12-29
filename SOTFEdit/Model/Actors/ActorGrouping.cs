using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Actors;

public class ActorGrouping(string name)
{
    public string Name => $"{name} ({ActorCollections.Count})";
    public ObservableCollectionEx<ActorCollection> ActorCollections { get; } = [];
}