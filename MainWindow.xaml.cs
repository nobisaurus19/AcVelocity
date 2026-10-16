using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using AcVelocity.Models;
using AcVelocity.ViewModels;

namespace AcVelocity
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NavTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tabName)
            {
                _vm.CurrentTab = tabName;
            }
        }

        private async void BtnLaunchSimulation_Click(object sender, RoutedEventArgs e)
        {
            await _vm.LaunchSimulationAsync();
        }

        private async void BtnLaunchOnline_Click(object sender, RoutedEventArgs e)
        {
            await _vm.LaunchOnlineAsync();
        }

        private async void BtnSaveGraphics_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SaveGraphicsSettingsAsync();
        }

        private async void BtnSaveCsp_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SaveCspSettingsAsync();
        }

        private async void BtnInstallCspZip_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "CSP Update Archives (*.zip)|*.zip|All Files (*.*)|*.*",
                Title = "Deploy Custom Shaders Patch Build Package"
            };

            if (dlg.ShowDialog() == true)
            {
                await _vm.InstallCspArchiveAsync(dlg.FileName);
            }
        }

        private async void BtnAutoInstallCsp_Click(object sender, RoutedEventArgs e)
        {
            await _vm.AutoInstallCspAsync();
        }

        private async void BtnOneClickUpdateCsp_Click(object sender, RoutedEventArgs e)
        {
            var rec = _vm.CspSettings.AvailableVersions.Find(v => v.IsRecommended);
            await _vm.AutoInstallCspAsync(rec);
        }

        private async void BtnRefreshContent_Click(object sender, RoutedEventArgs e)
        {
            await _vm.InitializeAsync();
        }

        private async void BtnSaveCurrentPreset_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtNewPresetName.Text?.Trim();
            if (string.IsNullOrEmpty(name)) name = "CUSTOM_PRESET";
            await _vm.SaveCurrentAsPresetAsync(name);
        }

        private async void BtnApplyPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is NexusPreset preset)
            {
                await _vm.ApplyPresetAsync(preset);
            }
        }

        private async void BtnSaveFfb_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SaveFfbSettingsAsync();
        }

        private void SteeringLock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tagStr && int.TryParse(tagStr, out int lk))
            {
                _vm.FfbSettings.SteeringLock = lk;
                _vm.Log($"Hardware Steering Lock set to: {lk}°");
            }
        }

        private void InputMethodPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string method)
            {
                _vm.SetInputMethod(method);
            }
        }

        private void ShifterMode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string mode)
            {
                _vm.FfbSettings.HShifterActive = (mode == "HSHIFTER");
                _vm.Log($"Transmission Shifter mode set to: {_vm.FfbSettings.ShifterModeDisplay}");
            }
        }

        private async void BtnRefreshServers_Click(object sender, RoutedEventArgs e)
        {
            await _vm.RefreshOnlineServersAsync();
        }

        private async void BtnConnectServerItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ServerInfo s)
            {
                await _vm.ConnectToServerAsync(s);
            }
        }

        private async void BtnScanServerMods_Click(object sender, RoutedEventArgs e)
        {
            await _vm.ScanSelectedServerModsAsync();
        }

        private async void BtnDownloadMissingServerMods_Click(object sender, RoutedEventArgs e)
        {
            await _vm.DownloadMissingServerModsAsync();
        }

        private async void BtnAddCustomServer_Click(object sender, RoutedEventArgs e)
        {
            var addr = TxtCustomServerAddress?.Text;
            if (!string.IsNullOrWhiteSpace(addr))
            {
                await _vm.AddCustomServerAsync(addr);
            }
        }

        private async void BtnCheckCarIntegrity_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CheckCarIntegrityAsync();
        }

        private void SessionMode_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string mode && _vm != null)
            {
                _vm.SelectedSessionMode = mode;
                _vm.Log($"Session Profile updated: {mode}");
            }
        }

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            _vm.ResetCarFilters();
        }

        private void BtnResetTrackFilters_Click(object sender, RoutedEventArgs e)
        {
            _vm.ResetTrackFilters();
        }

        private void ClassPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string cls)
            {
                _vm.SelectedClass = cls;
            }
        }

        private void CarOriginPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string origin)
            {
                _vm.SelectedCarOrigin = origin;
            }
        }

        private void TrackOriginPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string origin)
            {
                _vm.SelectedTrackOrigin = origin;
            }
        }

        private void SkinItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CarSkinInfo skin)
            {
                _vm.SelectSkin(skin);
                e.Handled = true;
            }
        }

        private void SpotlightCar_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.OpenCarSelectorModal();
        }

        private void SpotlightTrack_Click(object sender, MouseButtonEventArgs e)
        {
            _vm.OpenTrackSelectorModal();
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            _vm.CloseCarSelectorModal();
        }

        private void BtnConfirmModalCar_Click(object sender, RoutedEventArgs e)
        {
            _vm.ConfirmCarSelection(_vm.ModalPreviewCar);
        }

        private void ModalCarList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_vm.ModalPreviewCar != null)
            {
                _vm.ConfirmCarSelection(_vm.ModalPreviewCar);
            }
        }

        private void BtnCloseTrackModal_Click(object sender, RoutedEventArgs e)
        {
            _vm.CloseTrackSelectorModal();
        }

        private void BtnConfirmModalTrack_Click(object sender, RoutedEventArgs e)
        {
            _vm.ConfirmTrackSelection(_vm.ModalPreviewTrack);
        }

        private void ModalTrackList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_vm.ModalPreviewTrack != null)
            {
                _vm.ConfirmTrackSelection(_vm.ModalPreviewTrack);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_vm.IsCarSelectorModalOpen)
                {
                    _vm.CloseCarSelectorModal();
                    e.Handled = true;
                }
                else if (_vm.IsTrackSelectorModalOpen)
                {
                    _vm.CloseTrackSelectorModal();
                    e.Handled = true;
                }
            }
        }

        private async void BtnBrowseMod_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Assetto Corsa Mod Packages (*.zip;*.7z;*.rar)|*.zip;*.7z;*.rar|All Files (*.*)|*.*",
                Title = "Select Assetto Corsa Mod Archive"
            };

            if (ofd.ShowDialog() == true)
            {
                await _vm.InstallModArchiveAsync(ofd.FileName);
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        var ext = Path.GetExtension(file).ToLowerInvariant();
                        if (ext == ".zip" || ext == ".7z" || ext == ".rar" || Directory.Exists(file))
                        {
                            await _vm.InstallModArchiveAsync(file);
                        }
                    }
                }
            }
        }

        private async void BtnSaveAudio_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SaveAudioSettingsAsync();
        }

        private async void BtnSaveAssists_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SaveDrivingAssistsAsync();
        }

        private async void BtnClearCache_Click(object sender, RoutedEventArgs e)
        {
            await _vm.ClearCacheAsync();
        }

        private async void ManageOwnSettings_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.IsChecked.HasValue)
            {
                await _vm.ToggleManageOwnSettingsAsync(cb.IsChecked.Value);
            }
        }

        private void AbsPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tagStr && int.TryParse(tagStr, out int absVal))
            {
                _vm.DrivingAssists.AbsMode = absVal;
            }
        }

        private void TcPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tagStr && int.TryParse(tagStr, out int tcVal))
            {
                _vm.DrivingAssists.TcMode = tcVal;
            }
        }

        // ================= CONTROL PROFILE & HARDWARE BINDING STUDIO HANDLERS =================

        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (_vm != null && _vm.IsListeningForInput)
            {
                var capturedKey = e.Key == Key.System ? e.SystemKey : e.Key;
                await _vm.HandleKeyboardKeyCapturedAsync(capturedKey);
                e.Handled = true;
            }
        }

        private async void BtnLoadControlProfile_Click(object sender, RoutedEventArgs e)
        {
            await _vm.LoadSelectedControlProfileAsync();
        }

        private async void BtnSaveNewControlProfile_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtNewControlProfileName?.Text?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                await _vm.SaveNewControlProfileAsync(name);
            }
        }

        private async void BtnOverwriteControlProfile_Click(object sender, RoutedEventArgs e)
        {
            await _vm.OverwriteSelectedControlProfileAsync();
        }

        private void BtnDeleteControlProfile_Click(object sender, RoutedEventArgs e)
        {
            _vm.DeleteSelectedControlProfile();
        }

        private void BtnOpenControlProfilesFolder_Click(object sender, RoutedEventArgs e)
        {
            _vm.OpenControlProfilesFolder();
        }

        private void BtnControlCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string category)
            {
                _vm.SelectedControlCategory = category;
            }
        }

        private async void BtnBindControlAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ControlActionBinding binding)
            {
                await _vm.StartListeningForBindingAsync(binding);
            }
        }

        private async void BtnClearControlAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ControlActionBinding binding)
            {
                await _vm.ClearActionBindingAsync(binding);
            }
        }

        private async void BtnWheelBtnDec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ControlActionBinding binding)
            {
                await _vm.CycleWheelButtonAsync(binding, -1);
            }
        }

        private async void BtnWheelBtnInc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is ControlActionBinding binding)
            {
                await _vm.CycleWheelButtonAsync(binding, 1);
            }
        }

        private async void CmbXboxButton_DropDownClosed(object sender, EventArgs e)
        {
            await _vm.SaveFfbSettingsAsync();
        }
    }
}