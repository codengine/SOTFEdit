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
    }

    public static class Items
    {
        public const int DefaultPlayerClothItemId = 495;
    }
}