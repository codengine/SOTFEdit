// ReSharper disable NotAccessedPositionalProperty.Global

namespace SOTFEdit.Model.SaveData.Storage.Module;

public record FoodSpoilStorageModule
    (int ModuleId, int CurrentState, long TimeRemainingInState = 86400, bool PauseDecay = true) : BaseStorageModule(
        ModuleId);