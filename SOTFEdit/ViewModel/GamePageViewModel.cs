using SOTFEdit.Model.Savegame;
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

    public bool Update(Savegame savegame)
    {
        var hasChanges = false;
        hasChanges = GameSetup.Update(savegame) || hasChanges;
        hasChanges = GameState.Update(savegame) || hasChanges;
        hasChanges = Weather.Update(savegame) || hasChanges;
        return hasChanges;
    }
}