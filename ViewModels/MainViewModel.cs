using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using AcVelocity.Collections;
using AcVelocity.Converters;
using AcVelocity.Models;
using AcVelocity.Services;

namespace AcVelocity.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AcScannerService _scanner = new();
        private readonly AcLauncherService _launcher = new();
        private readonly AcVideoSettingsService _videoService = new();
        private readonly ModInstallerService _modService = new();
        private readonly CspService _cspService = new();
        private readonly FfbService _ffbService = new();
        private readonly PresetService _presetService = new();
        private readonly ServerBrowserService _serverService = new();
        private readonly ContentManagerUtilityService _cmUtilService = new();
        private readonly AcSystemSettingsService _systemService = new();

        private readonly DispatcherTimer _carSearchDebounceTimer;
        private readonly DispatcherTimer _trackSearchDebounceTimer;

        private string _acRootPath = string.Empty;
        private bool _isLoading;
        private string _statusMessage = "INITIALIZING NEXUS CORE...";
        private string _currentTab = "LAUNCHER"; // LAUNCHER, SKINS, CONTROLS, PRESETS, ONLINE, GRAPHICS, MODS, CSP, AUDIO, SYSTEM

        private string _carSearchQuery = string.Empty;
        private string _trackSearchQuery = string.Empty;
        private string _selectedBrand = "ALL BRANDS";
        private string _selectedClass = "ALL CLASSES";
        private string _selectedCarOrigin = "ALL"; // ALL, OFFICIAL, MODS
        private string _selectedTrackOrigin = "ALL"; // ALL, OFFICIAL, MODS

        private CarInfo? _selectedCar;
        private TrackInfo? _selectedTrack;
        private string _selectedSessionMode = "PRACTICE";
        private int _aiCount = 6;
        private int _aiDifficulty = 96;
        private int _sunAngle = 48;
        private string _weatherName = "3_clear";

        // Video Settings
        private AcVideoSettings _videoSettings = new();

        // Audio & Assists Settings
        private AcAudioSettings _audioSettings = new();
        public AcAudioSettings AudioSettings
        {
            get => _audioSettings;
            set { SetField(ref _audioSettings, value); }
        }

        private AcDrivingAssists _drivingAssists = new();
        public AcDrivingAssists DrivingAssists
        {
            get => _drivingAssists;
            set { SetField(ref _drivingAssists, value); }
        }

        // System & Diagnostics Settings
        private AcSystemSettings _systemSettings = new();
        public AcSystemSettings SystemSettings
        {
            get => _systemSettings;
            set { SetField(ref _systemSettings, value); }
        }

        // Online Multiplay
        private string _onlineServerIp = "127.0.0.1";
        private int _onlineServerPort = 9600;
        private string _onlinePassword = string.Empty;
        private string _onlinePilotName = "Velocity_Pilot";

        public ObservableCollection<CarInfo> AllCars { get; } = new();
        public RangeObservableCollection<CarInfo> FilteredCars { get; } = new();

        public ObservableCollection<string> AvailableBrands { get; } = new();
        public ObservableCollection<string> AvailableClasses { get; } = new();

        public ObservableCollection<TrackInfo> AllTracks { get; } = new();
        public RangeObservableCollection<TrackInfo> FilteredTracks { get; } = new();

        public ObservableCollection<string> ConsoleLogs { get; } = new();

        public string CurrentTab
        {
            get => _currentTab;
            set
            {
                if (SetField(ref _currentTab, value))
                {
                    OnPropertyChanged(nameof(IsLauncherTab));
                    OnPropertyChanged(nameof(IsSkinsTab));
                    OnPropertyChanged(nameof(IsControlsTab));
                    OnPropertyChanged(nameof(IsPresetsTab));
                    OnPropertyChanged(nameof(IsOnlineTab));
                    OnPropertyChanged(nameof(IsGraphicsTab));
                    OnPropertyChanged(nameof(IsModsTab));
                    OnPropertyChanged(nameof(IsCspTab));
                    OnPropertyChanged(nameof(IsAudioTab));
                    OnPropertyChanged(nameof(IsSystemTab));
                    Log($"Navigated to Module: {value}");
                }
            }
        }

        public bool IsLauncherTab { get => CurrentTab == "LAUNCHER"; set { if (value) CurrentTab = "LAUNCHER"; } }
        public bool IsSkinsTab { get => CurrentTab == "SKINS"; set { if (value) CurrentTab = "SKINS"; } }
        public bool IsControlsTab { get => CurrentTab == "CONTROLS"; set { if (value) CurrentTab = "CONTROLS"; } }
        public bool IsPresetsTab { get => CurrentTab == "PRESETS"; set { if (value) CurrentTab = "PRESETS"; } }
        public bool IsOnlineTab { get => CurrentTab == "ONLINE"; set { if (value) CurrentTab = "ONLINE"; } }
        public bool IsGraphicsTab { get => CurrentTab == "GRAPHICS"; set { if (value) CurrentTab = "GRAPHICS"; } }
        public bool IsModsTab { get => CurrentTab == "MODS"; set { if (value) CurrentTab = "MODS"; } }
        public bool IsCspTab { get => CurrentTab == "CSP"; set { if (value) CurrentTab = "CSP"; } }
        public bool IsAudioTab { get => CurrentTab == "AUDIO"; set { if (value) CurrentTab = "AUDIO"; } }
        public bool IsSystemTab { get => CurrentTab == "SYSTEM"; set { if (value) CurrentTab = "SYSTEM"; } }

        private CspSettings _cspSettings = new();
        public CspSettings CspSettings
        {
            get => _cspSettings;
            set { SetField(ref _cspSettings, value); }
        }

        public string AcRootPath
        {
            get => _acRootPath;
            set { SetField(ref _acRootPath, value); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { SetField(ref _isLoading, value); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { SetField(ref _statusMessage, value); }
        }

        public string CarSearchQuery
        {
            get => _carSearchQuery;
            set
            {
                if (SetField(ref _carSearchQuery, value))
                {
                    _carSearchDebounceTimer.Stop();
                    _carSearchDebounceTimer.Start();
                }
            }
        }

        public string SelectedCarOrigin
        {
            get => _selectedCarOrigin;
            set
            {
                if (SetField(ref _selectedCarOrigin, value))
                {
                    ApplyCarFilter();
                    Log($"Vehicle Origin Filter: {value}");
                }
            }
        }

        public string SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (SetField(ref _selectedBrand, value))
                {
                    ApplyCarFilter();
                    Log($"Filtered Brand: {value}");
                }
            }
        }

        public string SelectedClass
        {
            get => _selectedClass;
            set
            {
                if (SetField(ref _selectedClass, value))
                {
                    ApplyCarFilter();
                    Log($"Filtered Class: {value}");
                }
            }
        }

        public string TrackSearchQuery
        {
            get => _trackSearchQuery;
            set
            {
                if (SetField(ref _trackSearchQuery, value))
                {
                    _trackSearchDebounceTimer.Stop();
                    _trackSearchDebounceTimer.Start();
                }
            }
        }

        public string SelectedTrackOrigin
        {
            get => _selectedTrackOrigin;
            set
            {
                if (SetField(ref _selectedTrackOrigin, value))
                {
                    ApplyTrackFilter();
                    Log($"Circuit Origin Filter: {value}");
                }
            }
        }

        public CarInfo? SelectedCar
        {
            get => _selectedCar;
            set
            {
                if (SetField(ref _selectedCar, value))
                {
                    OnPropertyChanged(nameof(HasSelectedCar));
                    Log($"Engaged Vehicle Matrix: {value?.DisplayTitle ?? "None"}");
                }
            }
        }

        public bool HasSelectedCar => SelectedCar != null;

        private bool _isCarSelectorModalOpen;
        public bool IsCarSelectorModalOpen
        {
            get => _isCarSelectorModalOpen;
            set { SetField(ref _isCarSelectorModalOpen, value); }
        }

        private CarInfo? _modalPreviewCar;
        public CarInfo? ModalPreviewCar
        {
            get => _modalPreviewCar ?? SelectedCar;
            set { SetField(ref _modalPreviewCar, value); }
        }

        public void OpenCarSelectorModal()
        {
            ModalPreviewCar = SelectedCar;
            IsCarSelectorModalOpen = true;
            Log("Activated Vehicle Selector Matrix.");
        }

        public void CloseCarSelectorModal()
        {
            IsCarSelectorModalOpen = false;
        }

        public void ConfirmCarSelection(CarInfo? car)
        {
            if (car != null)
            {
                SelectedCar = car;
                Log($"Engaged Fleet Asset: {car.DisplayTitle}");
            }
            IsCarSelectorModalOpen = false;
        }

        // --- Track Selector Modal Properties & Methods ---
        private bool _isTrackSelectorModalOpen;
        public bool IsTrackSelectorModalOpen
        {
            get => _isTrackSelectorModalOpen;
            set { SetField(ref _isTrackSelectorModalOpen, value); }
        }

        private TrackInfo? _modalPreviewTrack;
        public TrackInfo? ModalPreviewTrack
        {
            get => _modalPreviewTrack ?? SelectedTrack;
            set { SetField(ref _modalPreviewTrack, value); }
        }

        public void OpenTrackSelectorModal()
        {
            ModalPreviewTrack = SelectedTrack;
            IsTrackSelectorModalOpen = true;
            Log("Activated Circuit Selector Matrix.");
        }

        public void CloseTrackSelectorModal()
        {
            IsTrackSelectorModalOpen = false;
        }

        public void ConfirmTrackSelection(TrackInfo? track)
        {
            if (track != null)
            {
                SelectedTrack = track;
                Log($"Locked Circuit Grid: {track.DisplayTitle}");
            }
            IsTrackSelectorModalOpen = false;
        }

        public void ResetTrackFilters()
        {
            _selectedTrackOrigin = "ALL";
            _trackSearchQuery = string.Empty;
            OnPropertyChanged(nameof(SelectedTrackOrigin));
            OnPropertyChanged(nameof(TrackSearchQuery));
            ApplyTrackFilter();
            Log("Reset circuit filters.");
        }

        public TrackInfo? SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                if (SetField(ref _selectedTrack, value))
                {
                    OnPropertyChanged(nameof(HasSelectedTrack));
                    Log($"Locked Circuit Grid: {value?.DisplayTitle ?? "None"}");
                }
            }
        }

        public bool HasSelectedTrack => SelectedTrack != null;

        public string SelectedSessionMode
        {
            get => _selectedSessionMode;
            set { SetField(ref _selectedSessionMode, value); }
        }

        public int AiCount
        {
            get => _aiCount;
            set { SetField(ref _aiCount, value); }
        }

        public int AiDifficulty
        {
            get => _aiDifficulty;
            set { SetField(ref _aiDifficulty, value); }
        }

        public int SunAngle
        {
            get => _sunAngle;
            set
            {
                if (SetField(ref _sunAngle, value))
                {
                    OnPropertyChanged(nameof(TimeOfDayDisplay));
                }
            }
        }

        public string TimeOfDayDisplay
        {
            get
            {
                // Convert sun angle to approximate time (AC standard: 48 = 13:00)
                int totalMinutes = (int)(((SunAngle % 360) / 360.0) * 1440.0) + 480; // offset
                int hour = (totalMinutes / 60) % 24;
                int min = (totalMinutes % 60) / 10 * 10;
                return $"{hour:D2}:{min:D2}";
            }
        }

        public string WeatherName
        {
            get => _weatherName;
            set { SetField(ref _weatherName, value); }
        }

        public ObservableCollection<string> AvailableWeatherPresets { get; } = new();

        private int _trackGrip = 100;
        public int TrackGrip
        {
            get => _trackGrip;
            set
            {
                if (SetField(ref _trackGrip, value))
                {
                    OnPropertyChanged(nameof(TrackGripDisplay));
                }
            }
        }

        public string TrackGripDisplay => TrackGrip switch
        {
            >= 100 => "OPTIMUM (100%)",
            >= 98 => "FAST (98%)",
            >= 95 => "GREEN (95%)",
            >= 90 => "DUSTY (90%)",
            _ => $"{TrackGrip}%"
        };

        private int _ambientTemp = 24;
        public int AmbientTemp
        {
            get => _ambientTemp;
            set
            {
                if (SetField(ref _ambientTemp, value))
                {
                    OnPropertyChanged(nameof(AmbientTempDisplay));
                }
            }
        }

        public string AmbientTempDisplay => $"{AmbientTemp}°C";

        private int _timeMultiplier = 1;
        public int TimeMultiplier
        {
            get => _timeMultiplier;
            set
            {
                if (SetField(ref _timeMultiplier, value))
                {
                    OnPropertyChanged(nameof(TimeMultiplierDisplay));
                }
            }
        }

        public string TimeMultiplierDisplay => $"{TimeMultiplier}x";

        // Controls & Force Feedback Props
        private FfbSettings _ffbSettings = new();
        public FfbSettings FfbSettings
        {
            get => _ffbSettings;
            set { SetField(ref _ffbSettings, value); }
        }

        // Simulation Presets Matrix
        public ObservableCollection<NexusPreset> AvailablePresets { get; } = new();
        private NexusPreset? _selectedPreset;
        public NexusPreset? SelectedPreset
        {
            get => _selectedPreset;
            set { SetField(ref _selectedPreset, value); }
        }

        // Graphics Props
        public AcVideoSettings VideoSettings
        {
            get => _videoSettings;
            set { SetField(ref _videoSettings, value); }
        }

        // Online Props & Server Directory
        public ObservableCollection<ServerInfo> OnlineServers { get; } = new();
        private ServerInfo? _selectedOnlineServer;
        public ServerInfo? SelectedOnlineServer
        {
            get => _selectedOnlineServer;
            set
            {
                if (SetField(ref _selectedOnlineServer, value) && value != null)
                {
                    OnlineServerIp = value.Ip;
                    OnlineServerPort = value.Port;
                    Log($"Targeted Remote Server: {value.Name} ({value.Ip}:{value.Port})");
                }
            }
        }

        private bool _isRefreshingServers;
        public bool IsRefreshingServers
        {
            get => _isRefreshingServers;
            set { SetField(ref _isRefreshingServers, value); }
        }

        public string OnlineServerIp
        {
            get => _onlineServerIp;
            set { SetField(ref _onlineServerIp, value); }
        }

        public int OnlineServerPort
        {
            get => _onlineServerPort;
            set { SetField(ref _onlineServerPort, value); }
        }

        public string OnlinePassword
        {
            get => _onlinePassword;
            set { SetField(ref _onlinePassword, value); }
        }

        public string OnlinePilotName
        {
            get => _onlinePilotName;
            set { SetField(ref _onlinePilotName, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _carSearchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _carSearchDebounceTimer.Tick += (s, e) =>
            {
                _carSearchDebounceTimer.Stop();
                ApplyCarFilter();
            };

            _trackSearchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _trackSearchDebounceTimer.Tick += (s, e) =>
            {
                _trackSearchDebounceTimer.Stop();
                ApplyTrackFilter();
            };

            Log("VELOCITY // NEXUS OS initialized.");
            Log("Connecting neural telemetry bus...");
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            StatusMessage = "DETECTING ASSETTO CORSA INSTALLATION...";

            AcRootPath = _scanner.DetectAssettoCorsaPath();

            if (string.IsNullOrEmpty(AcRootPath))
            {
                StatusMessage = "ASSETTO CORSA PATH NOT FOUND";
                Log("Warning: Could not automatically detect Assetto Corsa directory.");
                IsLoading = false;
                return;
            }

            Log($"Mounting Assetto Corsa Depot: {AcRootPath}");
            StatusMessage = "SYNCHRONIZING CONTENT DEPOT...";

            try
            {
                var cars = await _scanner.ScanCarsAsync(AcRootPath);
                AllCars.Clear();
                foreach (var c in cars) AllCars.Add(c);

                // Build Available Brands list
                AvailableBrands.Clear();
                AvailableBrands.Add("ALL BRANDS");
                var distinctBrands = AllCars
                    .Where(c => !string.IsNullOrWhiteSpace(c.Brand))
                    .GroupBy(c => c.Brand.Trim())
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key);

                foreach (var b in distinctBrands)
                {
                    AvailableBrands.Add(b);
                }

                // Build Available Classes
                AvailableClasses.Clear();
                AvailableClasses.Add("ALL CLASSES");
                AvailableClasses.Add("STREET");
                AvailableClasses.Add("RACE");
                AvailableClasses.Add("DRIFT");
                AvailableClasses.Add("SUPERCAR");
                AvailableClasses.Add("TRAFFIC");

                ApplyCarFilter();
                Log($"Indexed {AllCars.Count} vehicles in hangar across {AvailableBrands.Count - 1} brands.");

                var tracks = await _scanner.ScanTracksAsync(AcRootPath);
                AllTracks.Clear();
                foreach (var t in tracks) AllTracks.Add(t);
                ApplyTrackFilter();
                Log($"Mapped {AllTracks.Count} high-fidelity circuits.");

                // Load Video settings
                VideoSettings = await _videoService.LoadSettingsAsync();
                Log($"Loaded graphics profile: {VideoSettings.DisplayResolution}");

                // Load Custom Shaders Patch (CSP) settings
                CspSettings = await _cspService.LoadSettingsAsync(AcRootPath);
                if (CspSettings.IsCspInstalled)
                {
                    Log($"CSP Online: v{CspSettings.CspVersion} (Build {CspSettings.CspBuild}) | Controller: {CspSettings.WeatherController}");
                }
                else
                {
                    Log("Custom Shaders Patch binary not detected in Assetto Corsa root.");
                }

                // Load Controls & Force Feedback (FFB) settings
                FfbSettings = await _ffbService.LoadSettingsAsync();
                Log($"Controls & FFB calibrated: Gain {FfbSettings.DisplayMasterGain}, Lock {FfbSettings.DisplayLock}");

                // Load Audio settings
                AudioSettings = await _systemService.LoadAudioSettingsAsync();
                Log($"Audio Matrix calibrated: Master {AudioSettings.DisplayMasterVolume}, Engine {AudioSettings.DisplayEngineVolume}");

                // Load Driving Assists
                DrivingAssists = await _systemService.LoadDrivingAssistsAsync();
                Log($"Driving Assists online: ABS {DrivingAssists.DisplayAbsMode}, TC {DrivingAssists.DisplayTcMode}");

                // Load System & Diagnostics
                SystemSettings = await _systemService.LoadSystemSettingsAsync(AcRootPath);
                Log($"System Diagnostics loaded: Cache {SystemSettings.CacheSizeDisplay}");

                // Calculate Total Disk Usage asynchronously
                _ = Task.Run(async () =>
                {
                    var disk = await _systemService.CalculateTotalDiskUsageAsync(AcRootPath);
                    SystemSettings.DiskUsageDisplay = disk;
                });

                // Load Simulation Presets
                await LoadPresetsAsync();
                Log($"Registered {AvailablePresets.Count} high-performance simulation presets.");

                // Scan Weather Presets
                ScanWeatherPresets();

                // Refresh Online Server bookmarks in background
                _ = RefreshOnlineServersAsync();

                // Default selections
                SelectedCar = FilteredCars.FirstOrDefault(c => c.Id == "abarth500") ?? FilteredCars.FirstOrDefault();
                SelectedTrack = FilteredTracks.FirstOrDefault(t => t.Id == "imola") ?? FilteredTracks.FirstOrDefault();

                StatusMessage = $"READY: {AllCars.Count} VEHICLES // {AllTracks.Count} CIRCUITS ONLINE";
                Log("Neural Simulation Matrix operational.");

                // Pre-cache preview images in background without blocking UI
                _ = Task.Run(() =>
                {
                    foreach (var c in cars)
                    {
                        if (!string.IsNullOrEmpty(c.PreviewImagePath))
                        {
                            StringToImageSourceConverter.GetCachedImage(c.PreviewImagePath, 300);
                        }
                    }
                    foreach (var t in tracks)
                    {
                        if (!string.IsNullOrEmpty(t.PreviewImagePath))
                        {
                            StringToImageSourceConverter.GetCachedImage(t.PreviewImagePath, 300);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"SCAN ERROR: {ex.Message}";
                Log($"Error scanning content: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void SelectSkin(CarSkinInfo skin)
        {
            if (SelectedCar != null && skin != null)
            {
                SelectedCar.SelectedSkin = skin.Name;
                if (!string.IsNullOrEmpty(skin.PreviewImagePath))
                {
                    SelectedCar.PreviewImagePath = skin.PreviewImagePath;
                }
                Log($"Applied Livery: {skin.DisplayName} to {SelectedCar.DisplayTitle}");
            }
        }

        public void ResetCarFilters()
        {
            _selectedBrand = "ALL BRANDS";
            _selectedClass = "ALL CLASSES";
            _selectedCarOrigin = "ALL";
            _carSearchQuery = string.Empty;
            OnPropertyChanged(nameof(SelectedBrand));
            OnPropertyChanged(nameof(SelectedClass));
            OnPropertyChanged(nameof(SelectedCarOrigin));
            OnPropertyChanged(nameof(CarSearchQuery));
            ApplyCarFilter();
            Log("Reset all vehicle filters.");
        }

        public async Task SaveGraphicsSettingsAsync()
        {
            try
            {
                await _videoService.SaveSettingsAsync(VideoSettings);
                Log($"Saved graphics profile to video.ini: {VideoSettings.DisplayResolution}");
                StatusMessage = "GRAPHICS PROFILE COMMITTED";
            }
            catch (Exception ex)
            {
                Log($"Failed to save graphics settings: {ex.Message}");
            }
        }

        public async Task InstallModArchiveAsync(string zipFilePath)
        {
            Log($"Unpacking mod archive: {Path.GetFileName(zipFilePath)}...");
            StatusMessage = "INJECTING MOD PACKAGE...";

            var res = await _modService.InstallZipModAsync(zipFilePath, AcRootPath);
            if (res.Success)
            {
                Log($"Success: [{res.ModType}] {res.Message}");
                StatusMessage = $"MOD INJECTED: {res.ModName}";
                await InitializeAsync();
            }
            else
            {
                Log($"Mod install failed: {res.Message}");
                StatusMessage = "MOD INSTALLATION FAILED";
            }
        }

        public async Task SaveCspSettingsAsync()
        {
            try
            {
                await _cspService.SaveSettingsAsync(CspSettings, AcRootPath);
                Log("Custom Shaders Patch settings committed to cfg/extension.");
                StatusMessage = "CSP SETTINGS COMMITTED SUCCESSFULLY";
            }
            catch (Exception ex)
            {
                Log($"Failed to save CSP settings: {ex.Message}");
                StatusMessage = "FAILED TO SAVE CSP SETTINGS";
            }
        }

        public async Task InstallCspArchiveAsync(string zipFilePath)
        {
            Log($"Unpacking Custom Shaders Patch build: {Path.GetFileName(zipFilePath)}...");
            StatusMessage = "INJECTING CSP PACKAGE...";

            var res = await _cspService.InstallCspZipAsync(zipFilePath, AcRootPath);
            if (res.Success)
            {
                Log($"CSP Success: {res.Message}");
                StatusMessage = "CSP BUILD DEPLOYED";
                CspSettings = await _cspService.LoadSettingsAsync(AcRootPath);
            }
            else
            {
                Log($"CSP install failed: {res.Message}");
                StatusMessage = "CSP INSTALLATION FAILED";
            }
        }

        public async Task LaunchSimulationAsync()
        {
            if (SelectedCar == null)
            {
                Log("Error: No vehicle selected for deployment.");
                return;
            }

            if (SelectedTrack == null)
            {
                Log("Error: No circuit selected for deployment.");
                return;
            }

            Log($"Compiling race profile: {SelectedCar.DisplayTitle} @ {SelectedTrack.DisplayTitle} ({SelectedSessionMode})...");
            StatusMessage = "COMPILING RACE CONFIGURATION...";

            try
            {
                var iniPath = await _launcher.GenerateRaceIniAsync(
                    SelectedCar,
                    SelectedTrack,
                    SelectedSessionMode,
                    AiCount,
                    AiDifficulty,
                    SunAngle,
                    WeatherName,
                    TrackGrip,
                    AmbientTemp,
                    AmbientTemp + 6,
                    TimeMultiplier
                );

                Log($"Written configuration matrix to {iniPath} [Grip: {TrackGripDisplay}, Temp: {AmbientTempDisplay}, Speed: {TimeMultiplierDisplay}]");
                Log("Executing direct simulation binary dispatch...");
                StatusMessage = "LAUNCHING ASSETTO CORSA CORE...";

                var (success, msg) = await _launcher.LaunchGameAsync(AcRootPath);
                if (success)
                {
                    Log($"Success: {msg}");
                    StatusMessage = "SIMULATION RUNNING // TELEMETRY ACTIVE";
                }
                else
                {
                    Log($"Launch Failed: {msg}");
                    StatusMessage = "LAUNCH FAILED";
                }
            }
            catch (Exception ex)
            {
                Log($"Launch exception: {ex.Message}");
                StatusMessage = "EXECUTION ERROR";
            }
        }

        public async Task LaunchOnlineAsync()
        {
            if (SelectedCar == null)
            {
                Log("Error: Please select a vehicle for online dispatch.");
                return;
            }

            if (string.IsNullOrWhiteSpace(OnlineServerIp))
            {
                Log("Error: Server IP cannot be empty.");
                return;
            }

            Log($"Establishing remote neural connection to {OnlineServerIp}:{OnlineServerPort}...");
            StatusMessage = "CONNECTING TO REMOTE SERVER...";

            try
            {
                var iniPath = await _launcher.GenerateOnlineRaceIniAsync(
                    OnlineServerIp,
                    OnlineServerPort,
                    OnlinePassword,
                    SelectedCar,
                    OnlinePilotName
                );

                Log($"Generated online profile in {iniPath}");
                var (success, msg) = await _launcher.LaunchGameAsync(AcRootPath);
                if (success)
                {
                    Log($"Dispatched: {msg}");
                    StatusMessage = "ONLINE SIMULATION ACTIVE";
                }
                else
                {
                    Log($"Connection failed: {msg}");
                    StatusMessage = "ONLINE DISPATCH FAILED";
                }
            }
            catch (Exception ex)
            {
                Log($"Online dispatch error: {ex.Message}");
            }
        }

        private void ScanWeatherPresets()
        {
            AvailableWeatherPresets.Clear();
            var weatherDir = Path.Combine(AcRootPath, "content", "weather");
            if (Directory.Exists(weatherDir))
            {
                var dirs = Directory.GetDirectories(weatherDir);
                foreach (var d in dirs)
                {
                    AvailableWeatherPresets.Add(Path.GetFileName(d));
                }
            }

            if (AvailableWeatherPresets.Count == 0)
            {
                var defaults = new[] { "3_clear", "4_mid_clear", "sol_01_clear", "sol_02_few_clouds", "sol_06_overcast", "sol_31_light_drizzle", "sol_35_rain", "sol_36_heavy_rain", "1_heavy_fog" };
                foreach (var def in defaults) AvailableWeatherPresets.Add(def);
            }

            if (!AvailableWeatherPresets.Contains(WeatherName))
            {
                WeatherName = AvailableWeatherPresets.FirstOrDefault() ?? "3_clear";
            }
        }

        public async Task SaveFfbSettingsAsync()
        {
            try
            {
                await _ffbService.SaveSettingsAsync(FfbSettings);
                Log($"Controls & Input saved to controls.ini: [{FfbSettings.InputMethod}] Gain {FfbSettings.DisplayMasterGain}, Lock {FfbSettings.DisplayLock}, Brake Gamma {FfbSettings.DisplayBrakesGamma}");
                StatusMessage = "CONTROLS & INPUT MATRIX COMMITTED";
            }
            catch (Exception ex)
            {
                Log($"Failed to save Controls settings: {ex.Message}");
                StatusMessage = "CONTROLS SAVE FAILED";
            }
        }

        public void SetInputMethod(string method)
        {
            if (FfbSettings != null)
            {
                FfbSettings.InputMethod = method;
                OnPropertyChanged(nameof(FfbSettings));
                Log($"Active Control Input System changed to: {method}");
            }
        }

        public async Task SaveAudioSettingsAsync()
        {
            try
            {
                await _systemService.SaveAudioSettingsAsync(AudioSettings);
                Log($"Audio Matrix saved to audio.ini: Master {AudioSettings.DisplayMasterVolume}, Engine {AudioSettings.DisplayEngineVolume}");
                StatusMessage = "AUDIO MATRIX COMMITTED";
            }
            catch (Exception ex)
            {
                Log($"Failed to save audio settings: {ex.Message}");
                StatusMessage = "AUDIO SAVE FAILED";
            }
        }

        public async Task SaveDrivingAssistsAsync()
        {
            try
            {
                await _systemService.SaveDrivingAssistsAsync(DrivingAssists);
                Log($"Driving Assists saved to assists.ini: ABS {DrivingAssists.DisplayAbsMode}, TC {DrivingAssists.DisplayTcMode}");
                StatusMessage = "DRIVING ASSISTS COMMITTED";
            }
            catch (Exception ex)
            {
                Log($"Failed to save driving assists: {ex.Message}");
                StatusMessage = "ASSISTS SAVE FAILED";
            }
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                StatusMessage = "PURGING TEMPORARY GAME CACHE...";
                var (success, msg) = await _systemService.ClearGameCacheAsync();
                Log(msg);
                StatusMessage = success ? "GAME CACHE PURGED" : "CACHE PURGE FAILED";

                // Refresh cache size
                var sys = await _systemService.LoadSystemSettingsAsync(AcRootPath);
                SystemSettings.CacheSizeDisplay = sys.CacheSizeDisplay;
            }
            catch (Exception ex)
            {
                Log($"Error clearing cache: {ex.Message}");
                StatusMessage = "CACHE PURGE ERROR";
            }
        }

        public async Task ToggleManageOwnSettingsAsync(bool manageOwn)
        {
            try
            {
                SystemSettings.ManageOwnSettings = manageOwn;
                await _systemService.SaveManageOwnSettingsAsync(manageOwn);
                Log($"Manage Own Settings toggled: {manageOwn} (No Hesi state synchronized)");
                StatusMessage = manageOwn ? "CONTENT MANAGER CONTROL ACTIVE" : "NEXUS AUTONOMOUS CONTROL ACTIVE";
            }
            catch (Exception ex)
            {
                Log($"Failed to update ManageOwnSettings: {ex.Message}");
            }
        }

        public async Task LoadPresetsAsync()
        {
            AvailablePresets.Clear();
            var builtIns = _presetService.GetBuiltInPresets();
            foreach (var p in builtIns) AvailablePresets.Add(p);

            var customs = await _presetService.LoadCustomPresetsAsync();
            foreach (var c in customs) AvailablePresets.Add(c);
        }

        public async Task ApplyPresetAsync(NexusPreset preset)
        {
            if (preset == null) return;
            Log($"Applying preset profile: [{preset.Name}]...");
            StatusMessage = $"APPLYING PRESET: {preset.Name}...";

            try
            {
                if (preset.Video != null)
                {
                    VideoSettings = preset.Video;
                    await _videoService.SaveSettingsAsync(VideoSettings);
                }
                if (preset.Csp != null)
                {
                    CspSettings = preset.Csp;
                    await _cspService.SaveSettingsAsync(CspSettings, AcRootPath);
                }
                if (preset.Ffb != null)
                {
                    FfbSettings = preset.Ffb;
                    await _ffbService.SaveSettingsAsync(FfbSettings);
                }
                SelectedPreset = preset;
                Log($"Profile activated: {preset.Name}. Graphics, CSP, and FFB committed.");
                StatusMessage = $"PRESET ACTIVE: {preset.Name}";
            }
            catch (Exception ex)
            {
                Log($"Error applying preset: {ex.Message}");
                StatusMessage = "PRESET APPLICATION FAILED";
            }
        }

        public async Task SaveCurrentAsPresetAsync(string presetName, string desc = "")
        {
            if (string.IsNullOrWhiteSpace(presetName)) return;

            var newPreset = new NexusPreset
            {
                Name = presetName.Trim(),
                Description = string.IsNullOrWhiteSpace(desc) ? $"Custom configuration created {DateTime.Now:yyyy-MM-dd HH:mm}" : desc.Trim(),
                Icon = "💾",
                Video = VideoSettings,
                Csp = CspSettings,
                Ffb = FfbSettings
            };

            await _presetService.SavePresetAsync(newPreset);
            await LoadPresetsAsync();
            SelectedPreset = AvailablePresets.FirstOrDefault(p => p.Name == newPreset.Name);
            Log($"Saved custom preset profile: {presetName}");
            StatusMessage = $"PRESET SAVED: {presetName}";
        }

        public async Task RefreshOnlineServersAsync()
        {
            IsRefreshingServers = true;
            StatusMessage = "PROBING ONLINE AC SERVERS...";
            try
            {
                var servers = await _serverService.LoadServersAsync();
                OnlineServers.Clear();
                foreach (var s in servers) OnlineServers.Add(s);

                Log($"Parsed {OnlineServers.Count} server bookmarks from Content Manager registry.");

                _ = Task.Run(async () =>
                {
                    foreach (var s in OnlineServers)
                    {
                        await _serverService.QueryServerStatsAsync(s);
                    }
                });
            }
            catch (Exception ex)
            {
                Log($"Error loading servers: {ex.Message}");
            }
            finally
            {
                IsRefreshingServers = false;
                StatusMessage = "ONLINE SERVER DIRECTORY READY";
            }
        }

        public async Task ConnectToServerAsync(ServerInfo server)
        {
            if (server == null) return;
            SelectedOnlineServer = server;
            await LaunchOnlineAsync();
        }

        public async Task CheckCarIntegrityAsync()
        {
            if (SelectedCar == null)
            {
                Log("No vehicle selected for integrity diagnosis.");
                return;
            }

            Log($"Diagnosing asset integrity for: {SelectedCar.DisplayTitle} ({SelectedCar.Id})...");
            var (isValid, issues) = await Task.Run(() => _cmUtilService.CheckCarIntegrity(SelectedCar, AcRootPath));

            if (isValid)
            {
                Log($"✅ Vehicle Integrity PASSED: {SelectedCar.DisplayTitle} physics, sounds, and liveries verified.");
                StatusMessage = $"VEHICLE INTEGRITY: 100% OPERATIONAL";
            }
            else
            {
                Log($"⚠️ Vehicle Integrity WARNING: {issues.Count} issue(s) detected for {SelectedCar.Id}:");
                foreach (var issue in issues)
                {
                    Log($"   - {issue}");
                }
                StatusMessage = $"INTEGRITY ALERT: {issues.Count} ISSUES DETECTED";
            }
        }

        private void ApplyCarFilter()
        {
            var q = (CarSearchQuery ?? string.Empty).Trim().ToLowerInvariant();
            var brandFilter = SelectedBrand;
            var classFilter = SelectedClass;
            var originFilter = SelectedCarOrigin;

            var query = AllCars.AsEnumerable();

            // 1. Filter by Origin (ALL, OFFICIAL, MODS)
            if (originFilter == "OFFICIAL")
            {
                query = query.Where(c => !c.IsMod);
            }
            else if (originFilter == "MODS")
            {
                query = query.Where(c => c.IsMod);
            }

            // 2. Filter by Brand / Manufacturer
            if (!string.IsNullOrEmpty(brandFilter) && brandFilter != "ALL BRANDS")
            {
                query = query.Where(c => string.Equals(c.Brand, brandFilter, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Filter by Class / Category
            if (!string.IsNullOrEmpty(classFilter) && classFilter != "ALL CLASSES")
            {
                var cf = classFilter.ToLowerInvariant();
                query = query.Where(c => 
                    (c.Class?.ToLowerInvariant().Contains(cf) ?? false) ||
                    (c.DisplayClass?.ToLowerInvariant().Contains(cf) ?? false) ||
                    (c.Tags != null && c.Tags.Any(t => t.ToLowerInvariant().Contains(cf))) ||
                    (cf == "race" && (c.Name?.ToLowerInvariant().Contains("gt3") == true || c.Name?.ToLowerInvariant().Contains("cup") == true)) ||
                    (cf == "drift" && (c.Name?.ToLowerInvariant().Contains("drift") == true || c.Tags?.Any(t => t.ToLowerInvariant().Contains("drift")) == true)) ||
                    (cf == "traffic" && (c.Class?.ToLowerInvariant().Contains("traffic") == true || c.Id?.ToLowerInvariant().Contains("traffic") == true))
                );
            }

            // 4. Filter by Model Search Query
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(c => (c.Name?.ToLowerInvariant().Contains(q) ?? false) ||
                                         (c.Brand?.ToLowerInvariant().Contains(q) ?? false) ||
                                         (c.Class?.ToLowerInvariant().Contains(q) ?? false) ||
                                         (c.Id?.ToLowerInvariant().Contains(q) ?? false) ||
                                         (c.Tags != null && c.Tags.Any(t => t.ToLowerInvariant().Contains(q))));
            }

            FilteredCars.ReplaceRange(query);
        }

        private void ApplyTrackFilter()
        {
            var q = (TrackSearchQuery ?? string.Empty).Trim().ToLowerInvariant();
            var originFilter = SelectedTrackOrigin;

            var query = AllTracks.AsEnumerable();

            // 1. Filter by Origin (ALL, OFFICIAL, MODS)
            if (originFilter == "OFFICIAL")
            {
                query = query.Where(t => !t.IsMod);
            }
            else if (originFilter == "MODS")
            {
                query = query.Where(t => t.IsMod);
            }

            // 2. Filter by Search Query
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(t => (t.Name?.ToLowerInvariant().Contains(q) ?? false) ||
                                         (t.Country?.ToLowerInvariant().Contains(q) ?? false) ||
                                         (t.City?.ToLowerInvariant().Contains(q) ?? false) ||
                                         (t.Id?.ToLowerInvariant().Contains(q) ?? false));
            }

            FilteredTracks.ReplaceRange(query);
        }

        public void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            ConsoleLogs.Insert(0, $"[{timestamp}] {message}");
            if (ConsoleLogs.Count > 100) ConsoleLogs.RemoveAt(ConsoleLogs.Count - 1);
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
