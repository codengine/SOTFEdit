using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.SaveData.Storage.Module;

public class FoodSpoilStorageModuleWrapper
{
    private readonly int _itemId;
    private readonly int _variantId;

    public FoodSpoilStorageModuleWrapper(FoodSpoilStorageModule foodSpoilStorageModule, int itemId, int variantId)
    {
        _itemId = itemId;
        _variantId = variantId;
        FoodSpoilStorageModule = foodSpoilStorageModule;
    }

    public FoodSpoilStorageModule FoodSpoilStorageModule { get; }
    public string Name => TranslationManager.Get($"itemVariants.{_itemId}.{_variantId}");
}