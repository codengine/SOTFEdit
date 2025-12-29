using System.Collections.Generic;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.SaveData.Actor;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model.Events;

public class SpawnActorsEvent(
    Position position, ActorType actorType, int spawnCount, int? familyId,
    List<Influence> influences, int spaceBetween, SpawnPattern spawnPattern)
{
    public Position Position { get; } = position;
    public ActorType ActorType { get; } = actorType;
    public int SpawnCount { get; } = spawnCount;
    public int? FamilyId { get; } = familyId;
    public List<Influence> Influences { get; } = influences;
    public int SpaceBetween { get; } = spaceBetween;
    public SpawnPattern SpawnPattern { get; } = spawnPattern;
}