using SOTFEdit.Model.Savegame;
using SOTFEdit.ViewModel;

namespace SOTFEdit.View;

public partial class StructuresPage
{
    private readonly StructuresPageViewModel _dataContext;

    public StructuresPage(StructuresPageViewModel viewModel)
    {
        DataContext = _dataContext = viewModel;
        InitializeComponent();
    }

    public bool Update(Savegame savegame)
    {
        return _dataContext.Update(savegame);
    }
}