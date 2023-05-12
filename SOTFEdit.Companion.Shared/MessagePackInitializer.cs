using MessagePack;
using MessagePack.Resolvers;

namespace SOTFEdit.Companion.Shared;

public static class MessagePackInitializer
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        StaticCompositeResolver.Instance.Register(
            GeneratedResolver.Instance,
            StandardResolver.Instance
        );

        var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

        MessagePackSerializer.DefaultOptions = option;
        _initialized = true;
    }
}