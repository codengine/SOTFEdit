using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Armour;

namespace SOTFEdit.ViewModel;

public partial class ArmorPageViewModel
{
    private readonly ItemList _itemList;
    private Savegame? _selectedSavegame;

    public ArmorPageViewModel(GameData gameData)
    {
        NewArmour = new NewArmourPiece(Armour, () => _selectedSavegame != null);
        _itemList = gameData.Items;
        foreach (var armorItem in _itemList.Where(item => item.Value.Type == "armor").OrderBy(item => item.Value.Name)) ArmourTypes.Add(armorItem.Value);

        SetupListeners();
    }

    public ObservableCollection<ArmourData> Armour { get; } = new();
    public ObservableCollection<Item> ArmourTypes { get; } = new();

    public NewArmourPiece NewArmour { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        Armour.Clear();
        var armour =
            m.SelectedSavegame?.SavegameStore.LoadJson<PlayerArmourDataModel>(SavegameStore.FileType
                .PlayerArmourSystemSaveData);

        if (armour != null)
        {
            foreach (var armourPiece in armour.Data.PlayerArmourSystem.ArmourPieces)
                Armour.Add(new ArmourData(armourPiece, _itemList.GetItem(armourPiece.ItemId)));
        }

        _selectedSavegame = m.SelectedSavegame;

        NewArmour.AddArmorCommand.NotifyCanExecuteChanged();
    }

    public bool Update(Savegame savegame, bool createBackup)
    {
        var playerArmourData =
            savegame.SavegameStore.LoadJson<PlayerArmourDataModel>(SavegameStore.FileType.PlayerArmourSystemSaveData);
        if (playerArmourData == null)
        {
            return false;
        }

        var selectedArmorPieces = Armour.Select(a => a.ArmourPiece).ToList();
        if (!PlayerArmourSystemModel.Merge(playerArmourData.Data.PlayerArmourSystem, selectedArmorPieces))
        {
            return false;
        }

        savegame.SavegameStore.StoreJson(SavegameStore.FileType.PlayerArmourSystemSaveData, playerArmourData,
            createBackup);
        return true;
    }

    public partial class NewArmourPiece : ObservableObject
    {
        private readonly Func<bool> _savegameSelected;
        private readonly Collection<ArmourData> _sink;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddArmorCommand))]
        private int _remainingArmourpoints;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddArmorCommand))]
        private Item? _selectedItem;

        internal NewArmourPiece(Collection<ArmourData> sink, Func<bool> savegameSelected)
        {
            _sink = sink;
            _savegameSelected = savegameSelected;
        }

        [RelayCommand(CanExecute = nameof(CanAddArmor))]
        private void AddArmor()
        {
            if (_sink.Count == 10)
            {
                return;
            }

            var maxSlot = _sink.Select(piece => piece.Slot)
                .DefaultIfEmpty(-1)
                .Max();

            var nextSlot = maxSlot == 1 ? 4 : maxSlot + 1;

            var armourPiece = new ArmourPieceModel
            {
                ItemId = SelectedItem!.Id,
                RemainingArmourpoints = RemainingArmourpoints,
                Slot = nextSlot
            };
            _sink.Add(new ArmourData(armourPiece, SelectedItem));

            AddArmorCommand.NotifyCanExecuteChanged();
        }

        public bool CanAddArmor()
        {
            return SelectedItem != null && RemainingArmourpoints > 0 &&
                   _sink.Count < 10 && _savegameSelected.Invoke();
        }
    }
}