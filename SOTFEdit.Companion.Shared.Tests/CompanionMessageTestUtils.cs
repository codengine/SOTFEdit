using MessagePack;
using SOTFEdit.Companion.Shared.Messages;

namespace SOTFEdit.Companion.Shared.Tests;

public static class CompanionMessageTestUtils
{
    public static ICompanionMessage Copy(ICompanionMessage original)
    {
        var serialized = MessagePackSerializer.Serialize(original);
        return MessagePackSerializer.Deserialize<ICompanionMessage>(serialized);
    }
}