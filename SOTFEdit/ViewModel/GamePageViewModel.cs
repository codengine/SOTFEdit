using SOTFEdit.Model.Savegame;
using SOTFEdit.View;

namespace SOTFEdit.ViewModel;

public class GamePageViewModel(GameSetupPage gameSetupPage, GameStatePage gameStatePage, WeatherPage weatherPage)
{
    public GameSetupPage GameSetup { get; } = gameSetupPage;
    public GameStatePage GameState { get; } = gameStatePage;
    public WeatherPage Weather { get; } = weatherPage;

    public bool Update(Savegame savegame)
    {
        var hasChanges = false;
        hasChanges = GameSetup.Update(savegame) || hasChanges;
        hasChanges = GameState.Update(savegame) || hasChanges;
        hasChanges = Weather.Update(savegame) || hasChanges;
        return hasChanges;
    }
}