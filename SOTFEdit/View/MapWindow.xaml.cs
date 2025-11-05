using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using SOTFEdit.Infrastructure.Companion;
using SOTFEdit.Infrastructure.Converters;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Events;
using SOTFEdit.Model.Map;
using SOTFEdit.ViewModel;
using System.Windows.Threading;

namespace SOTFEdit.View
{
    public partial class MapWindow : MetroWindow
    {
        private readonly MapViewModel _dataContext;
        private IPoi? _clickedPoi;
        private readonly DispatcherTimer _autoActionTimer;

        public MapWindow(RequestOpenMapEvent message)
        {
            InitializeComponent();
            DataContext = _dataContext = new MapViewModel(message.PoiGroups, Ioc.Default.GetRequiredService<MapManager>(),
                Ioc.Default.GetRequiredService<GameData>());
            _dataContext.IsNotConnected = !Ioc.Default.GetRequiredService<CompanionConnectionManager>().IsConnected();
            SetupListeners();
            // Listen for connection and savegame changes to update title
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<SOTFEdit.Model.Events.CompanionConnectionStatusEvent>(this, (_, __) => UpdateWindowTitle());
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<SOTFEdit.Model.Events.SelectedSavegameChangedEvent>(this, (_, __) => UpdateWindowTitle());
            UpdateWindowTitle();

            // Restore window position, size, and screen robustly
            int screenIndex = SOTFEdit.Settings.Default.MapWindowScreenIndex;
            double left = SOTFEdit.Settings.Default.MapWindowLeft;
            double top = SOTFEdit.Settings.Default.MapWindowTop;
            double width = SOTFEdit.Settings.Default.MapWindowWidth;
            double height = SOTFEdit.Settings.Default.MapWindowHeight;
            string lastState = SOTFEdit.Settings.Default.MapWindowState;

            Screen? targetScreen = (screenIndex >= 0 && screenIndex < Screen.AllScreens.Length)
                ? Screen.AllScreens[screenIndex]
                : Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();
            var workingArea = (targetScreen ?? Screen.AllScreens[0]).WorkingArea;

            // If last state was maximized, restore to Normal, move to correct screen, then maximize
            if (lastState == WindowState.Maximized.ToString())
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                WindowState = WindowState.Normal;
                // Clamp to working area
                double safeWidth = Math.Min(Math.Max(width, 400), workingArea.Width - 20);
                double safeHeight = Math.Min(Math.Max(height, 300), workingArea.Height - 20);
                double safeLeft = Math.Min(Math.Max(left, workingArea.Left), workingArea.Right - safeWidth);
                double safeTop = Math.Min(Math.Max(top, workingArea.Top), workingArea.Bottom - safeHeight);

                // If out of bounds, center
                if (safeLeft < workingArea.Left || safeLeft > workingArea.Right - 100 || safeTop < workingArea.Top || safeTop > workingArea.Bottom - 100)
                {
                    safeLeft = workingArea.Left + (workingArea.Width - safeWidth) / 2;
                    safeTop = workingArea.Top + (workingArea.Height - safeHeight) / 2;
                }

                Width = safeWidth;
                Height = safeHeight;
                Left = safeLeft;
                Top = safeTop;

                // Maximize after the window is loaded to ensure it's on the correct screen
                Loaded += (s, e) =>
                {
                    if (targetScreen != null)
                    {
                        MoveWindowToScreen(this, targetScreen);
                    }
                    WindowState = WindowState.Maximized;
                };
            }
            else
            {
                // Normal state: just restore position/size
                if (left >= workingArea.Left && top >= workingArea.Top && width > 0 && height > 0)
                {
                    Width = width;
                    Height = height;
                    Left = left;
                    Top = top;
                }
                else
                {
                    Left = workingArea.Left + (workingArea.Width - Width) / 2;
                    Top = workingArea.Top + (workingArea.Height - Height) / 2;
                }
                if (Enum.TryParse<WindowState>(lastState, out var state))
                {
                    WindowState = state;
                }
            }

            // Setup periodic timer for AutoConnect/AutoReload
            _autoActionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _autoActionTimer.Tick += AutoActionTimer_Tick;
            _autoActionTimer.Start();
            _dataContext.PropertyChanged += DataContextOnPropertyChanged;
            
            // Handle state changes to restore proper size when un-maximizing
            StateChanged += OnStateChanged;
            
            // Trigger initial auto-actions immediately (don't wait 5 seconds)
            AutoActionTimer_Tick(null, EventArgs.Empty);
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            // When restoring from maximized, ensure we use the saved size
            if (WindowState == WindowState.Normal)
            {
                double savedWidth = SOTFEdit.Settings.Default.MapWindowWidth;
                double savedHeight = SOTFEdit.Settings.Default.MapWindowHeight;
                double savedLeft = SOTFEdit.Settings.Default.MapWindowLeft;
                double savedTop = SOTFEdit.Settings.Default.MapWindowTop;
                
                if (savedWidth > 0 && savedHeight > 0)
                {
                    Width = savedWidth;
                    Height = savedHeight;
                    Left = savedLeft;
                    Top = savedTop;
                }
            }
        }

        private void UpdateWindowTitle()
        {
            var companionManager = CommunityToolkit.Mvvm.DependencyInjection.Ioc.Default.GetRequiredService<SOTFEdit.Infrastructure.Companion.CompanionConnectionManager>();
            var isConnected = companionManager.IsConnected();
            var status = companionManager.Status;
            string connectionPart;
            if (isConnected)
            {
                var ip = SOTFEdit.Settings.Default.CompanionAddress;
                var port = SOTFEdit.Settings.Default.CompanionPort;
                connectionPart = SOTFEdit.Infrastructure.TranslationManager.GetFormatted("windows.main.connection.connected", ip, port);
            }
            else if (status == SOTFEdit.Infrastructure.Companion.CompanionConnectionManager.ConnectionStatus.Connecting)
            {
                connectionPart = SOTFEdit.Infrastructure.TranslationManager.Get("windows.main.connection.connecting");
            }
            else
            {
                connectionPart = SOTFEdit.Infrastructure.TranslationManager.Get("windows.main.connection.disconnected");
            }

            var savegame = SOTFEdit.SavegameManager.SelectedSavegame;
            string savegamePart;
            string modifiedPart = string.Empty;
            if (savegame != null)
            {
                string type = savegame.PrintableType;
                var fileName = savegame.FullPath.Split(System.IO.Path.DirectorySeparatorChar).LastOrDefault();
                savegamePart = SOTFEdit.Infrastructure.TranslationManager.GetFormatted("windows.main.savegame.loaded", type!, fileName!);
                var modified = savegame.SavegameStore.LastWriteTime;
                modifiedPart = SOTFEdit.Infrastructure.TranslationManager.GetFormatted("windows.main.modified", modified.ToString("yyyy-MM-dd HH:mm")!);
            }
            else
            {
                savegamePart = SOTFEdit.Infrastructure.TranslationManager.Get("windows.main.savegame.none");
            }

            Title = SOTFEdit.Infrastructure.TranslationManager.GetFormatted("windows.map.title", connectionPart, savegamePart, modifiedPart);
        }

        private void DataContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapViewModel.AutoConnect) || e.PropertyName == nameof(MapViewModel.AutoReload))
            {
                // Optionally, could pause timer if both are false
                if (!_dataContext.AutoConnect && !_dataContext.AutoReload)
                {
                    _autoActionTimer.Stop();
                }
                else if (!_autoActionTimer.IsEnabled)
                {
                    _autoActionTimer.Start();
                }
            }
        }

        private void AutoActionTimer_Tick(object? sender, EventArgs e)
        {
            if (_dataContext.AutoConnect)
            {
                var companionManager = Ioc.Default.GetRequiredService<CompanionConnectionManager>();
                if (!companionManager.IsConnected())
                {
                    // Fire and forget
                    _ = companionManager.ConnectAsync();
                }
            }
            if (_dataContext.AutoReload)
            {
                if (SavegameManager.SelectedSavegame is { } selectedSavegame)
                {
                    // Reload the current loaded savegame if it has been modified
                    var currentLastWriteTime = Directory.GetLastWriteTime(selectedSavegame.FullPath);
                    if (currentLastWriteTime > selectedSavegame.SavegameStore.LastWriteTime)
                    {
                        var reloaded = SavegameManager.ReloadSavegame(selectedSavegame);
                        if (reloaded != null)
                        {
                            SavegameManager.SelectedSavegame = reloaded;
                        }
                    }
                }
                else
                {
                    // No savegame loaded yet - load the most recent one
                    var savegames = SavegameManager.GetSavegames();
                    var mostRecent = savegames.Values.OrderByDescending(s => s.LastSaveTime).FirstOrDefault();
                    if (mostRecent != null)
                    {
                        SavegameManager.SelectedSavegame = mostRecent;
                    }
                }
            }
        }

        private void SetupListeners()
        {
            WeakReferenceMessenger.Default.Register<OpenCategorySelectorEvent>(this,
                (_, _) => OnOpenCategorySelectorEvent());
            WeakReferenceMessenger.Default.Register<ShowMapImageEvent>(this,
                (_, message) => OnShowMapImageEvent(message));
            PoiMessenger.Instance.Register<ShowTeleportWindowEvent>(this,
                (_, message) => OnShowTeleportWindowEvent(message));
            PoiMessenger.Instance.Register<ShowSpawnActorsWindowEvent>(this,
                (_, message) => OnShowSpawnActorsWindowEvent(message));
            PoiMessenger.Instance.Register<SpawnActorsEvent>(this,
                (_, message) => OnSpawnActorsEvent(message));
            PoiMessenger.Instance.Register<SelectedPoiChangedEvent>(this,
                (_, message) => OnSelectedPoiChangedEvent(message.IsSelected));
            PoiMessenger.Instance.Register<PlayerPosChangedEvent>(this,
                (_, message) => OnPlayerPosChangedEvent(message));
        }

        private void OnPlayerPosChangedEvent(PlayerPosChangedEvent message)
        {
            if (!_dataContext.FollowPlayer)
            {
                return;
            }

            var ingameToPixel = CoordinateConverter.IngameToPixel(message.NewPosition.X, message.NewPosition.Z);
            var zoomControl = MapZoomControl;
            if (zoomControl != null)
            {
                zoomControl.ZoomToPos(ingameToPixel.Item1, ingameToPixel.Item2, -16);
            }
        }

        private void OnSelectedPoiChangedEvent(bool isSelected)
        {
            var flyout = PoiDetailsFlyout;
            if (flyout != null)
            {
                flyout.IsOpen = isSelected;
            }
        }

        private static void OnSpawnActorsEvent(SpawnActorsEvent message)
        {
            if (SavegameManager.SelectedSavegame is { } selectedSavegame)
            {
                ActorModifier.Spawn(selectedSavegame, message.Position, message.ActorType, message.SpawnCount,
                    message.FamilyId, message.Influences, message.SpaceBetween, message.SpawnPattern);
            }
        }

        private void OnShowSpawnActorsWindowEvent(ShowSpawnActorsWindowEvent message)
        {
            var window = new MapSpawnActorsWindow(this, message.Poi);
            window.Show();
        }

        private void OnShowTeleportWindowEvent(ShowTeleportWindowEvent message)
        {
            var window = new MapTeleportWindow(this, message.Destination, message.TeleportationMode);
            window.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Save window state, position, and screen
            SOTFEdit.Settings.Default.MapWindowState = WindowState.ToString();
            
            // Get window interop for screen detection
            var windowInterop = new System.Windows.Interop.WindowInteropHelper(this);
            var screen = Screen.FromHandle(windowInterop.Handle);
            var wa = screen.WorkingArea;
            
            if (WindowState == WindowState.Normal)
            {
                // Save current position and size
                double clampedLeft = Math.Min(Math.Max(Left, wa.Left), wa.Right - Math.Max(Width, 100));
                double clampedTop = Math.Min(Math.Max(Top, wa.Top), wa.Bottom - Math.Max(Height, 100));
                double clampedWidth = Math.Min(Math.Max(Width, 400), wa.Width - 20);
                double clampedHeight = Math.Min(Math.Max(Height, 300), wa.Height - 20);
                SOTFEdit.Settings.Default.MapWindowLeft = clampedLeft;
                SOTFEdit.Settings.Default.MapWindowTop = clampedTop;
                SOTFEdit.Settings.Default.MapWindowWidth = clampedWidth;
                SOTFEdit.Settings.Default.MapWindowHeight = clampedHeight;
            }
            else if (WindowState == WindowState.Maximized)
            {
                // Save RestoreBounds so we know what size to restore to
                double clampedLeft = Math.Min(Math.Max(RestoreBounds.Left, wa.Left), wa.Right - Math.Max(RestoreBounds.Width, 100));
                double clampedTop = Math.Min(Math.Max(RestoreBounds.Top, wa.Top), wa.Bottom - Math.Max(RestoreBounds.Height, 100));
                double clampedWidth = Math.Min(Math.Max(RestoreBounds.Width, 400), wa.Width - 20);
                double clampedHeight = Math.Min(Math.Max(RestoreBounds.Height, 300), wa.Height - 20);
                SOTFEdit.Settings.Default.MapWindowLeft = clampedLeft;
                SOTFEdit.Settings.Default.MapWindowTop = clampedTop;
                SOTFEdit.Settings.Default.MapWindowWidth = clampedWidth;
                SOTFEdit.Settings.Default.MapWindowHeight = clampedHeight;
            }
            
            SOTFEdit.Settings.Default.MapWindowScreenIndex = Array.IndexOf(Screen.AllScreens, screen);
            SOTFEdit.Settings.Default.Save();

            WeakReferenceMessenger.Default.UnregisterAll(this);
            WeakReferenceMessenger.Default.UnregisterAll(DataContext);
            PoiMessenger.Instance.Reset();
            _dataContext.SaveSettings();
            base.OnClosing(e);
        }

        private void OnShowMapImageEvent(ShowMapImageEvent message)
        {
            var window = new ShowImageWindow(this, message.Url, message.Title);
            window.Show();
        }

        private void OnOpenCategorySelectorEvent()
        {
            var flyout = MapOptionsFlyout;
            if (flyout != null)
            {
                flyout.IsOpen = !flyout.IsOpen;
            }
        }

        private void PoiSelector_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ZoomControl_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ZoomControl.ZoomControl zoomControl)
            {
                return;
            }

            if (zoomControl.IsPanning)
            {
                _dataContext.FollowPlayer = false;
                return;
            }

            if (_clickedPoi is { } clickedPoi)
            {
                if (_dataContext.SelectedPoi is IClickToMovePoi { IsMoveRequested: true } clickToMovePoi)
                {
                    if (clickedPoi.Position is { } position)
                    {
                        clickToMovePoi.AcceptNewPos(position);
                    }
                }
                else
                {
                    _dataContext.SelectedPoi = clickedPoi;
                }

                _clickedPoi = null;
            }
            else
            {
                if (_dataContext.SelectedPoi is IClickToMovePoi clickToMovePoi)
                {
                    clickToMovePoi.IsMoveRequested = false;
                }

                _dataContext.SelectedPoi = null;
            }
        }

        private void sotfLink_Click(object sender, MouseButtonEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(RequestStartProcessEvent.ForUrl("https://sotf.th.gl/"));
        }

        private void MapWindow_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            e.Handled = true;
            Close();
        }

        private void ZoomControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clickedPoi = e.OriginalSource is Image { Tag: IPoi ipoi } ? ipoi : null;
        }

        private void AlwaysOnTop_OnChecked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void AlwaysOnTop_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
        }

        // P/Invoke to move window to correct monitor
        private static void MoveWindowToScreen(Window window, Screen screen)
        {
            var interop = new System.Windows.Interop.WindowInteropHelper(window);
            if (interop.Handle == IntPtr.Zero) return;
            var wa = screen.WorkingArea;
            SetWindowPos(interop.Handle, IntPtr.Zero, wa.Left, wa.Top, wa.Width, wa.Height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }
}