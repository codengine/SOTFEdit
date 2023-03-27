using SOTFEdit.Model;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public class GamePageViewModel
{
    public GamePageViewModel(GameSetupPage gameSetupPage, GameStatePage gameStatePage, WeatherPage weatherPage)
    {
        GameSetup = gameSetupPage;
        GameState = gameStatePage;
        Weather = weatherPage;
    }

    public GameSetupPage GameSetup { get; }
    public GameStatePage GameState { get; }
    public WeatherPage Weather { get; }

    public bool Update(Savegame savegame, bool createBackup)
    {
        var hasChanges = false;
        hasChanges = GameSetup.Update(savegame, createBackup) || hasChanges;
        hasChanges = GameState.Update(savegame, createBackup) || hasChanges;
        hasChanges = Weather.Update(savegame, createBackup) || hasChanges;
        return hasChanges;
    }
}