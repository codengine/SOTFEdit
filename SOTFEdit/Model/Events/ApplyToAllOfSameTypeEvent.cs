using SOTFEdit.Model.Storage;

namespace SOTFEdit.Model.Events;

public class ApplyToAllOfSameTypeEvent
{
    public ApplyToAllOfSameTypeEvent(IStorage storage)
    {
        Storage = storage;
    }

    public IStorage Storage { get; }
}