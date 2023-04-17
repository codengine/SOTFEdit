using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.SaveData.Storage.Module;

public class ModuleWrapper
{
    private readonly int _itemId;
    private readonly int _variantId;

    public ModuleWrapper(IStorageModule module, int itemId, int variantId)
    {
        _itemId = itemId;
        _variantId = variantId;
        Module = module;
    }

    public IStorageModule Module { get; }
    public string Name => TranslationManager.Get($"itemVariants.{_itemId}.{_variantId}");
}