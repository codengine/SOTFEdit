using System.Collections.Generic;
using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.Actors;

public class ActorCollection
{
    private readonly string _name;

    public ActorCollection(string name, IEnumerable<Actor> actors)
    {
        Actors.AddRange(actors);
        _name = name;
    }

    public string Name => $"{_name} ({Actors.Count})";
    public ObservableCollectionEx<Actor> Actors { get; } = new();
}