using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcVelocity.Models
{
    public class ServerCarSlotInfo : INotifyPropertyChanged
    {
        private string _carId = string.Empty;
        private string _displayName = string.Empty;
        private string _skin = string.Empty;
        private int _totalSlots = 1;
        private int _freeSlots = 1;
        private bool _isInstalled;
        private string? _downloadUrl;
        private string? _previewImagePath;

        public string CarId
        {
            get => _carId;
            set { if (SetField(ref _carId, value)) OnPropertyChanged(nameof(IsTrafficCar)); }
        }

        public string DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? _carId : _displayName;
            set => SetField(ref _displayName, value);
        }

        public string Skin
        {
            get => _skin;
            set => SetField(ref _skin, value);
        }

        public int TotalSlots
        {
            get => _totalSlots;
            set { if (SetField(ref _totalSlots, value)) OnPropertyChanged(nameof(SlotBadge)); }
        }

        public int FreeSlots
        {
            get => _freeSlots;
            set
            {
                if (SetField(ref _freeSlots, value))
                {
                    OnPropertyChanged(nameof(SlotBadge));
                    OnPropertyChanged(nameof(HasFreeSlot));
                }
            }
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                if (SetField(ref _isInstalled, value))
                {
                    OnPropertyChanged(nameof(InstallStatusBadge));
                }
            }
        }

        public string? DownloadUrl
        {
            get => _downloadUrl;
            set
            {
                if (SetField(ref _downloadUrl, value))
                {
                    OnPropertyChanged(nameof(HasDownloadUrl));
                    OnPropertyChanged(nameof(InstallStatusBadge));
                }
            }
        }

        public string? PreviewImagePath
        {
            get => _previewImagePath;
            set => SetField(ref _previewImagePath, value);
        }

        public bool IsTrafficCar => _carId.Contains("traffic", StringComparison.OrdinalIgnoreCase);
        public bool HasFreeSlot => FreeSlots > 0;
        public bool HasDownloadUrl => !string.IsNullOrWhiteSpace(DownloadUrl);
        public string SlotBadge => IsTrafficCar ? "AI TRAFFIC" : $"{FreeSlots}/{TotalSlots} FREE";
        public string InstallStatusBadge => IsInstalled
            ? "✅ INSTALLED"
            : (HasDownloadUrl ? "📥 MISSING (CDN)" : "❌ MISSING");

        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? prop = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class ServerInfo : INotifyPropertyChanged
    {
        private string _name = "Community Server";
        private string _ip = "127.0.0.1";
        private int _port = 9600;
        private int _httpPort = 8081;
        private string _track = "Unknown Track";
        private string _rawTrackId = string.Empty;
        private string _cleanTrackId = string.Empty;
        private string _trackLayoutId = string.Empty;
        private string? _trackDownloadUrl;
        private int _requiredCspBuild = 0;
        private int _clients = 0;
        private int _maxClients = 24;
        private int _pingMs = 45;
        private bool _isOnline = true;
        private bool _hasPassword = false;
        private string _password = string.Empty;
        private string _country = "GLOBAL";

        private bool _isTrackInstalled = true;
        private bool _isCspRequirementMet = true;
        private int _installedCarCount = 0;
        private int _missingCarCount = 0;
        private int _downloadableMissingCount = 0;

        public string Name { get => _name; set => SetField(ref _name, value); }
        public string Ip { get => _ip; set { if (SetField(ref _ip, value)) OnPropertyChanged(nameof(AddressDisplay)); } }
        public int Port { get => _port; set { if (SetField(ref _port, value)) OnPropertyChanged(nameof(AddressDisplay)); } }
        public int HttpPort { get => _httpPort; set { if (SetField(ref _httpPort, value)) OnPropertyChanged(nameof(AddressDisplay)); } }
        public string Track { get => _track; set => SetField(ref _track, value); }
        public string RawTrackId { get => _rawTrackId; set => SetField(ref _rawTrackId, value); }
        public string CleanTrackId { get => _cleanTrackId; set { if (SetField(ref _cleanTrackId, value)) NotifyModStatusChanged(); } }
        public string TrackLayoutId { get => _trackLayoutId; set { if (SetField(ref _trackLayoutId, value)) NotifyModStatusChanged(); } }
        public string? TrackDownloadUrl { get => _trackDownloadUrl; set => SetField(ref _trackDownloadUrl, value); }
        public int RequiredCspBuild { get => _requiredCspBuild; set { if (SetField(ref _requiredCspBuild, value)) NotifyModStatusChanged(); } }

        public int Clients { get => _clients; set { if (SetField(ref _clients, value)) { OnPropertyChanged(nameof(PlayersDisplay)); OnPropertyChanged(nameof(HasSlots)); } } }
        public int MaxClients { get => _maxClients; set { if (SetField(ref _maxClients, value)) { OnPropertyChanged(nameof(PlayersDisplay)); OnPropertyChanged(nameof(HasSlots)); } } }
        public int PingMs { get => _pingMs; set => SetField(ref _pingMs, value); }
        public bool IsOnline { get => _isOnline; set { if (SetField(ref _isOnline, value)) NotifyModStatusChanged(); } }
        public bool HasPassword { get => _hasPassword; set => SetField(ref _hasPassword, value); }
        public string Password { get => _password; set => SetField(ref _password, value); }
        public string Country { get => _country; set => SetField(ref _country, value); }

        public List<string> RequiredCarIds { get; set; } = new();
        public Dictionary<string, string> CarDownloadUrls { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> MissingCarIds { get; set; } = new();
        public ObservableCollection<ServerCarSlotInfo> CarSlots { get; } = new();

        public bool IsTrackInstalled { get => _isTrackInstalled; set { if (SetField(ref _isTrackInstalled, value)) NotifyModStatusChanged(); } }
        public bool IsCspRequirementMet { get => _isCspRequirementMet; set { if (SetField(ref _isCspRequirementMet, value)) NotifyModStatusChanged(); } }
        public int InstalledCarCount { get => _installedCarCount; set { if (SetField(ref _installedCarCount, value)) NotifyModStatusChanged(); } }
        public int MissingCarCount { get => _missingCarCount; set { if (SetField(ref _missingCarCount, value)) NotifyModStatusChanged(); } }
        public int DownloadableMissingCount { get => _downloadableMissingCount; set { if (SetField(ref _downloadableMissingCount, value)) NotifyModStatusChanged(); } }

        public string PlayersDisplay => $"{Clients}/{MaxClients} PILOTS";
        public string AddressDisplay => $"{Ip}:{HttpPort}";
        public bool HasSlots => Clients < MaxClients;

        public bool IsReadyToJoin => IsOnline && IsTrackInstalled && MissingCarCount == 0 && IsCspRequirementMet;
        public bool HasMissingMods => !IsTrackInstalled || MissingCarCount > 0 || !IsCspRequirementMet;
        public bool CanAutoDownloadMissing => DownloadableMissingCount > 0;

        public string ReadinessBadge
        {
            get
            {
                if (!IsOnline) return "OFFLINE";
                if (RequiredCarIds.Count == 0 && string.IsNullOrEmpty(CleanTrackId)) return "PROBING...";
                if (IsReadyToJoin) return "✅ READY";
                int totalMissing = MissingCarCount + (IsTrackInstalled ? 0 : 1);
                return totalMissing > 0 ? $"⚠️ {totalMissing} MISSING" : "⚠️ CSP REQ";
            }
        }

        public string TrackStatusDisplay => string.IsNullOrEmpty(CleanTrackId)
            ? $"Circuit: {Track}"
            : (IsTrackInstalled
                ? $"✅ Track Installed: {CleanTrackId}" + (string.IsNullOrEmpty(TrackLayoutId) ? "" : $" [{TrackLayoutId}]")
                : $"❌ MISSING TRACK: {CleanTrackId}" + (string.IsNullOrEmpty(TrackLayoutId) ? "" : $" [{TrackLayoutId}]"));

        public string CspStatusDisplay => RequiredCspBuild > 0
            ? (IsCspRequirementMet
                ? $"✅ CSP Build {RequiredCspBuild}+ Requirement Met"
                : $"❌ Requires Custom Shaders Patch Build {RequiredCspBuild}+")
            : "✅ Standard CSP Compatible";

        public string CarsStatusDisplay => RequiredCarIds.Count == 0
            ? "Click 'SCAN SERVER MODS' to verify entry list"
            : (MissingCarCount == 0
                ? $"✅ All {InstalledCarCount}/{RequiredCarIds.Count} Server Cars Installed"
                : $"⚠️ {InstalledCarCount}/{RequiredCarIds.Count} Cars Installed ({MissingCarCount} Missing{(DownloadableMissingCount > 0 ? $", {DownloadableMissingCount} Auto-Downloadable" : "")})");

        public void NotifyModStatusChanged()
        {
            OnPropertyChanged(nameof(IsReadyToJoin));
            OnPropertyChanged(nameof(HasMissingMods));
            OnPropertyChanged(nameof(CanAutoDownloadMissing));
            OnPropertyChanged(nameof(ReadinessBadge));
            OnPropertyChanged(nameof(TrackStatusDisplay));
            OnPropertyChanged(nameof(CspStatusDisplay));
            OnPropertyChanged(nameof(CarsStatusDisplay));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? prop = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
