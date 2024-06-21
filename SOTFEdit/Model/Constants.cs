namespace SOTFEdit.Model;

public static class Constants
{
    public static class Actors
    {
        public const int KelvinTypeId = 9;
        public const int VirginiaTypeId = 10;
        public const int StateAlive = 2;
        public const float FullHealth = 100.0f;
        public const float NoAnger = 0.0f;
        public const float NoFear = 0.0f;
        public const float FullSentiment = 100.0f;
        public const float FullAnger = 100.0f;
        public const float FullFear = 100.0f;
        public const float FullAffection = 100.0f;
        public const float FullEnergy = 100.0f;
        public const float FullHydration = 100.0f;
        public const float LowestSentiment = -100.0f;
    }

    public static class Settings
    {
        public const int SettingTypeBool = 0;
        public const int SettingTypeString = 3;
    }

    public static class GameSetupKeys
    {
        public const string Mode = "Mode";
        public const string Uid = "UID";
        public const string EnemyHealth = "GameSetting.Vail.EnemyHealth";
        public const string EnemyDamage = "GameSetting.Vail.EnemyDamage";
        public const string EnemyArmour = "GameSetting.Vail.EnemyArmour";
        public const string EnemyAggression = "GameSetting.Vail.EnemyAggression";
        public const string EnemySearchParties = "GameSetting.Vail.EnemySearchParties";
        public const string AnimalSpawnRate = "GameSetting.Vail.AnimalSpawnRate";
        public const string StartingSeason = "GameSetting.Environment.StartingSeason";
        public const string SeasonLength = "GameSetting.Environment.SeasonLength";
        public const string DayLength = "GameSetting.Environment.DayLength";
        public const string PrecipitationFrequency = "GameSetting.Environment.PrecipitationFrequency";
        public const string ConsumableEffects = "GameSetting.Survival.ConsumableEffects";
        public const string PlayerStatsDamage = "GameSetting.Survival.PlayerStatsDamage";
        public const string PlayersTriggerTraps = "GameSetting.Survival.PlayersTriggerTraps";
        public const string BuildingResistance = "GameSetting.Survival.BuildingResistance";
        public const string CreativeMode = "GameSetting.Survival.CreativeMode";
        public const string PlayersImmortalMode = "GameSetting.Survival.PlayersImmortalMode";
        public const string OneHitToCutTrees = "GameSetting.Survival.OneHitToCutTrees";
        public const string EnemySpawn = "GameSetting.Vail.EnemySpawn";
        public const string InventoryPause = "GameSetting.Vail.InventoryPause";
        public const string ColdPenalties = "GameSetting.Survival.ColdPenalties";
        public const string ReducedFoodInContainers = "GameSetting.Survival.ReducedFoodInContainers";
        public const string ReducedAmmoInContainers = "GameSetting.Survival.ReducedAmmoInContainers";
        public const string SingleUseContainers = "GameSetting.Survival.SingleUseContainers";
        public const string ForcePlaceFullLoad = "GameSetting.FreeForm.ForcePlaceFullLoad";
        public const string NoCuttingsSpawn = "GameSetting.Construction.NoCuttingsSpawn";
        public const string PvpDamage = "GameSetting.Multiplayer.PvpDamage";
        public const string ColdPenaltiesStatReduction = "GameSetting.Survival.ColdPenaltiesStatReduction";
    }

    public static class SettingValueKeys
    {
        public const string IntValue = "IntValue";
        public const string FloatValue = "FloatValue";
        public const string StringValue = "StringValue";
        public const string BoolValue = "BoolValue";
    }

    public static class JsonKeys
    {
        public const string PlayerInventory = "Data.PlayerInventory";
        public const string GameSetup = "Data.GameSetup";
        public const string GameState = "Data.GameState";
        public const string VailWorldSim = "Data.VailWorldSim";
        public const string NpcItemInstances = "Data.NpcItemInstances";
        public const string PlayerClothingSystem = "Data.PlayerClothingSystem";
        public const string PlayerState = "Data.PlayerState";
        public const string WeatherSystem = "Data.WeatherSystem";
        public const string ScrewStructureInstances = "Data.ScrewStructureInstances";
        public const string WorldObjectLocatorManager = "Data.WorldObjectLocatorManager";
        public const string PlayerArmourSystem = "Data.PlayerArmourSystem";
        public const string Fires = "Data.Fires";
        public const string ScrewStructureNodeInstances = "Data.ScrewStructureNodeInstances";
        public const string StructureDestruction = "Data.StructureDestruction";
        public const string WorldItemManager = "Data.WorldItemManager";
        public const string ZipLineManager = "Data.ZipLineManager";
        public const string ScrewTraps = "Data.ScrewTraps";
        public const string Constructions = "Data.Constructions";
    }


    public static class Items
    {
        public const int DefaultPlayerClothItemId = 495;
    }
}