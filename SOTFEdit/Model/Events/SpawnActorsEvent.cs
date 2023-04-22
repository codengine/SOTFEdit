using System.Collections.Generic;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.SaveData.Actor;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class SpawnActorsEvent
{
    public SpawnActorsEvent(Position position, ActorType actorType, int spawnCount, int? familyId,
        List<Influence> influences, int spaceBetween, SpawnPattern spawnPattern)
    {
        Position = position;
        ActorType = actorType;
        SpawnCount = spawnCount;
        FamilyId = familyId;
        Influences = influences;
        SpaceBetween = spaceBetween;
        SpawnPattern = spawnPattern;
    }

    public Position Position { get; }
    public ActorType ActorType { get; }
    public int SpawnCount { get; }
    public int? FamilyId { get; }
    public List<Influence> Influences { get; }
    public int SpaceBetween { get; }
    public SpawnPattern SpawnPattern { get; }
}