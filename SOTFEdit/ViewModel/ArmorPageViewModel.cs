using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Model;
using SOTFEdit.Model.SaveData;

namespace SOTFEdit.ViewModel;

public partial class ArmorPageViewModel
{
    private readonly ItemList _itemList;
    private Savegame? _selectedSavegame;

    public ArmorPageViewModel()
    {
        NewArmour = new NewArmourPiece(Armour, () => _selectedSavegame != null);
        _itemList = Ioc.Default.GetRequiredService<ItemList>();
        foreach (var armorItem in _itemList.Where(item => item.Value.Type == "armor").OrderBy(item => item.Value.Name))
        {
            ArmourTypes.Add(armorItem.Value);
        }

        SetupListeners();
    }

    public ObservableCollection<ArmourData> Armour { get; } = new();
    public ObservableCollection<Item> ArmourTypes { get; } = new();

    public NewArmourPiece NewArmour { get; }

    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<SelectedSavegameChanged>(this,
            (_, m) => { OnSelectedSavegameChanged(m); });
    }

    private void OnSelectedSavegameChanged(SelectedSavegameChanged m)
    {
        lock (Armour)
        {
            Armour.Clear();
            var armour =
                m.SelectedSavegame?.SavegameStore.LoadJson<PlayerArmourData>(SavegameStore.FileType
                    .PlayerArmourSystemSaveData);

            if (armour != null)
            {
                foreach (var armourPiece in armour.Data.PlayerArmourSystem.ArmourPieces)
                {
                    Armour.Add(new ArmourData(armourPiece, _itemList.GetItem(armourPiece.ItemId)));
                }
            }

            _selectedSavegame = m.SelectedSavegame;
            NewArmour.AddArmorCommand.NotifyCanExecuteChanged();
        }
    }

    public void Update(Savegame savegame, bool createBackup)
    {
        var playerArmourData =
            savegame.SavegameStore.LoadJson<PlayerArmourData>(SavegameStore.FileType.PlayerArmourSystemSaveData);
        if (playerArmourData == null)
        {
            return;
        }

        var selectedArmorPieces = Armour.Select(a => a.ArmourPiece).ToList();
        if (PlayerArmourSystem.Merge(playerArmourData.Data.PlayerArmourSystem, selectedArmorPieces))
        {
            savegame.SavegameStore.StoreJson(SavegameStore.FileType.PlayerArmourSystemSaveData, playerArmourData,
                createBackup);
        }
    }

    public partial class NewArmourPiece : ObservableObject
    {
        private readonly Func<bool> _savegameSelected;
        private readonly Collection<ArmourData> _sink;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddArmorCommand))]
        private Item? _item = null;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddArmorCommand))]
        private int _remainingArmourpoints = 0;

        internal NewArmourPiece(Collection<ArmourData> sink, Func<bool> savegameSelected)
        {
            _sink = sink;
            _savegameSelected = savegameSelected;
        }

        [RelayCommand(CanExecute = nameof(CanAddArmor))]
        public void AddArmor()
        {
            lock (_sink)
            {
                if (_sink.Count == 10)
                {
                    return;
                }

                var maxSlot = _sink.Select(piece => piece.Slot)
                    .DefaultIfEmpty(-1)
                    .Max();

                var nextSlot = maxSlot == 1 ? 4 : maxSlot + 1;

                var armourPiece = new ArmourPiece
                {
                    ItemId = Item!.Id,
                    RemainingArmourpoints = RemainingArmourpoints,
                    Slot = nextSlot
                };
                _sink.Add(new ArmourData(armourPiece, Item));
                AddArmorCommand.NotifyCanExecuteChanged();
            }
        }

        public bool CanAddArmor()
        {
            lock (_sink)
            {
                return Item != null && RemainingArmourpoints > 0 &&
                       _sink.Count < 10 && _savegameSelected.Invoke();
            }
        }
    }
}

public record ArmourData
{
    private readonly Item? _item;

    public ArmourData(ArmourPiece armourPiece, Item? item)
    {
        ArmourPiece = armourPiece;
        _item = item;
    }

    public ArmourPiece ArmourPiece { get; }

    public int Id => _item?.Id ?? ArmourPiece.ItemId;

    public float RemainingArmourpoints
    {
        get => ArmourPiece.RemainingArmourpoints;
        set => ArmourPiece.RemainingArmourpoints = value;
    }

    public string Name => _item?.Name ?? "??? Unknown Item";
    public string NameDe => _item?.NameDe ?? "";

    public int Slot => ArmourPiece.Slot;
}