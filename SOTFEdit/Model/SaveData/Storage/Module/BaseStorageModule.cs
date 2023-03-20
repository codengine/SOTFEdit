namespace SOTFEdit.Model.SaveData.Storage.Module;

public abstract record BaseStorageModule(int ModuleId) : SotfBaseModel, IStorageModule
{
    public int GetModuleId()
    {
        return ModuleId;
    }
}