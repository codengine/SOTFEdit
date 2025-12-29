using SOTFEdit.Infrastructure;

namespace SOTFEdit.Model.SaveData.Storage.Module;

public class FoodSpoilStorageModuleWrapper(FoodSpoilStorageModule foodSpoilStorageModule, int itemId, int variantId)
{
    public FoodSpoilStorageModule FoodSpoilStorageModule { get; } = foodSpoilStorageModule;
    public string Name => TranslationManager.Get($"itemVariants.{itemId}.{variantId}");
}