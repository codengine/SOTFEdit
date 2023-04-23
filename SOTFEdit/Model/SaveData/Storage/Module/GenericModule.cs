using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SOTFEdit.Model.SaveData.Storage.Module;

[JsonConverter(typeof(GenericStorageModuleConverter))]
public record GenericModule(int ModuleId, JObject ModuleToken) : BaseStorageModule(ModuleId);