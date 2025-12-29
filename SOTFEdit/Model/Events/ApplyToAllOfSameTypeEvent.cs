using SOTFEdit.Model.Storage;

namespace SOTFEdit.Model.Events;

public class ApplyToAllOfSameTypeEvent(IStorage storage)
{
    public IStorage Storage { get; } = storage;
}