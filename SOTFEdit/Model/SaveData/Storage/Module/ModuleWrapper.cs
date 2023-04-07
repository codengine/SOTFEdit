namespace SOTFEdit.Model.SaveData.Storage.Module;

public class ModuleWrapper
{
    public ModuleWrapper(IStorageModule module, string name)
    {
        Module = module;
        Name = name;
    }

    public IStorageModule Module { get; }
    public string Name { get; }
}