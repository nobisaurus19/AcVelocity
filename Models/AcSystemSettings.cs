using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcVelocity.Models
{
    public class AcSystemSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool _manageOwnSettings = false;
        public bool ManageOwnSettings
        {
            get => _manageOwnSettings;
            set
            {
                if (SetField(ref _manageOwnSettings, value))
                {
                    OnPropertyChanged(nameof(SettingsManageStatusDisplay));
                }
            }
        }

        public string SettingsManageStatusDisplay => ManageOwnSettings
            ? "LOCKED BY CONTENT MANAGER (Manual In-App Control Disabled)"
            : "AUTONOMOUS NEXUS CONTROL (Auto-Optimization Active)";

        private string _diskUsageDisplay = "Calculating...";
        public string DiskUsageDisplay
        {
            get => _diskUsageDisplay;
            set => SetField(ref _diskUsageDisplay, value);
        }

        private string _cacheSizeDisplay = "Calculating...";
        public string CacheSizeDisplay
        {
            get => _cacheSizeDisplay;
            set => SetField(ref _cacheSizeDisplay, value);
        }

        private bool _steamRunning = true;
        public bool SteamRunning
        {
            get => _steamRunning;
            set => SetField(ref _steamRunning, value);
        }

        private bool _acInstalled = true;
        public bool AcInstalled
        {
            get => _acInstalled;
            set => SetField(ref _acInstalled, value);
        }

        private bool _cmInstalled = true;
        public bool CmInstalled
        {
            get => _cmInstalled;
            set => SetField(ref _cmInstalled, value);
        }

        private bool _cspInstalled = true;
        public bool CspInstalled
        {
            get => _cspInstalled;
            set => SetField(ref _cspInstalled, value);
        }

        private bool _directXInstalled = true;
        public bool DirectXInstalled
        {
            get => _directXInstalled;
            set => SetField(ref _directXInstalled, value);
        }

        private bool _vcRedistInstalled = true;
        public bool VcRedistInstalled
        {
            get => _vcRedistInstalled;
            set => SetField(ref _vcRedistInstalled, value);
        }
    }
}
