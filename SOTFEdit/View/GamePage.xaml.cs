﻿using System.Windows.Controls;
using System.Windows.Input;
using SOTFEdit.Model;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class GamePage
{
    private readonly GamePageViewModel _dataContext;

    public GamePage(GamePageViewModel gamePageViewModel)
    {
        DataContext = _dataContext = gamePageViewModel;
        InitializeComponent();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        return _dataContext.Update(savegame, createBackup);
    }

    private void GameState_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scv = (ScrollViewer)sender;
        scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
        e.Handled = true;
    }
}