using CommunityToolkit.Mvvm.Messaging;

namespace SOTFEdit.Model.Map;

public static class PoiMessenger
{
    public static WeakReferenceMessenger Instance { get; } = new();
}