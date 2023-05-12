using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using SOTFEdit.Model;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.SaveData.Armour;
using SOTFEdit.Model.Savegame;

namespace SOTFEdit.ViewModel;

public partial class ArmorPageViewModel
{
    private readonly ObservableCollection<ArmourData> _armour = new();
    private readonly ItemList _itemList;

    public ArmorPageViewModel(GameData gameData)
    {
        NewArmour = new NewArmourPiece(_armour, () => SavegameManager.SelectedSavegame != null);
        _itemList = gameData.Items;
        foreach (var armorItem in
                 _itemList.Where(item => item.Value.IsEquippableArmor).OrderBy(item => item.Value.Name))
        {
            ArmourTypes.Add(armorItem.Value);
        }

        ArmourView = CollectionViewSource.GetDefaultView(_armour);
        ArmourView.SortDescriptions.Add(new SortDescription("Slot", ListSortDirection.Ascending));

        _armour.CollectionChanged += (_, _) =>
        {
            SetAllToMaxCommand.NotifyCanExecuteChanged();
            SetAllToDefaultCommand.NotifyCanExecuteChanged();
        };

        SetupListeners();
    }

    public ICollectionView ArmourView { get; }
    public ObservableCollection<Item> ArmourTypes { get; } = new();

    public NewArmourPiece NewArmour { get; }

    [RelayCommand]
    private void RemoveArmour(ArmourData armourData)
    {
        _armour.Remove(armourData);
        NewArmour.AddArmorCommand.NotifyCanExecuteChanged();
    }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChangedEvent>(this,
            (_, m) => OnSelectedSavegameChanged(m));
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChangedEvent m)
    {
        _armour.Clear();
        var armour =
            m.SelectedSavegame?.SavegameStore.LoadJsonRaw(SavegameStore.FileType
                .PlayerArmourSystemSaveData)?.Parent.ToObject<PlayerArmourDataModel>();

        if (armour != null)
        {
            foreach (var armourPiece in armour.Data.PlayerArmourSystem.ArmourPieces)
            {
                _armour.Add(new ArmourData(armourPiece, _itemList.GetItem(armourPiece.ItemId)));
            }
        }

        NewArmour.AddArmorCommand.NotifyCanExecuteChanged();
    }

    public bool Update(Savegame savegame)
    {
        if (savegame.SavegameStore.LoadJsonRaw(SavegameStore.FileType
                    .PlayerArmourSystemSaveData) is not
                { } saveDataWrapper || saveDataWrapper.Parent.ToObject<PlayerArmourDataModel>() is not
                { } playerArmourData)
        {
            return false;
        }

        var selectedArmorPieces = _armour.Select(a => a.ArmourPiece).ToList();
        var hasChanges = PlayerArmourSystemModel.Merge(playerArmourData.Data.PlayerArmourSystem, selectedArmorPieces);

        if (!hasChanges)
        {
            return hasChanges;
        }

        saveDataWrapper.GetJsonBasedToken(Constants.JsonKeys.PlayerArmourSystem)?["ArmourPieces"]?
            .Replace(JToken.FromObject(playerArmourData.Data.PlayerArmourSystem.ArmourPieces));
        saveDataWrapper.MarkAsModified(Constants.JsonKeys.PlayerArmourSystem);

        return hasChanges;
    }

    private bool HasArmorPieces()
    {
        return _armour.Count > 0;
    }

    [RelayCommand(CanExecute = nameof(HasArmorPieces))]
    private void SetAllToDefault()
    {
        foreach (var armourData in _armour)
        {
            armourData.ArmourPiece.RemainingArmourpoints = armourData.DefaultDurability;
        }
    }

    [RelayCommand(CanExecute = nameof(HasArmorPieces))]
    private void SetAllToMax()
    {
        foreach (var armourData in _armour)
        {
            armourData.ArmourPiece.RemainingArmourpoints = armourData.MaxDurability;
        }
    }

    public partial class NewArmourPiece : ObservableObject
    {
        private readonly Func<bool> _savegameSelected;
        private readonly Collection<ArmourData> _sink;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddArmorCommand))]
        private int _remainingArmourpoints;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddArmorCommand))]
        [NotifyCanExecuteChangedFor(nameof(SetMaxDurabilityForNewArmourCommand))]
        [NotifyCanExecuteChangedFor(nameof(SetDefaultDurabilityForNewArmourCommand))]
        private Item? _selectedItem;

        internal NewArmourPiece(Collection<ArmourData> sink, Func<bool> savegameSelected)
        {
            _sink = sink;
            _savegameSelected = savegameSelected;
        }

        partial void OnSelectedItemChanged(Item? value)
        {
            RemainingArmourpoints = value?.Durability?.Default ?? RemainingArmourpoints;
        }

        [RelayCommand(CanExecute = nameof(HasItemSelected))]
        private void SetDefaultDurabilityForNewArmour()
        {
            RemainingArmourpoints = SelectedItem?.Durability?.Default ?? 1;
        }

        [RelayCommand(CanExecute = nameof(HasItemSelected))]
        private void SetMaxDurabilityForNewArmour()
        {
            RemainingArmourpoints = SelectedItem?.Durability?.Max ?? 1;
        }

        [RelayCommand(CanExecute = nameof(CanAddArmor))]
        private void AddArmor()
        {
            if (_sink.Count == 10)
            {
                return;
            }

            var nextSlot = 0;

            foreach (var slot in _sink.Select(piece => piece.Slot).OrderBy(slot => slot))
            {
                if (slot == nextSlot)
                {
                    nextSlot = slot switch
                    {
                        0 => 1,
                        1 => 4,
                        _ => slot + 1
                    };
                }
                else
                {
                    break;
                }
            }

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
            return HasItemSelected() && RemainingArmourpoints > 0 &&
                   _sink.Count < 10 && _savegameSelected.Invoke();
        }

        private bool HasItemSelected()
        {
            return SelectedItem != null;
        }
    }
}