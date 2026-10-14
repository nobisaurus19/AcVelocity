using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class AcSystemSettingsService
    {
        public string GetCfgPath(string fileName)
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path1 = Path.Combine(myDocs, "Assetto Corsa", "cfg", fileName);
            if (File.Exists(path1)) return path1;

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path2 = Path.Combine(userProfile, "OneDrive", "Documents", "Assetto Corsa", "cfg", fileName);
            if (File.Exists(path2)) return path2;

            var path3 = Path.Combine(userProfile, "Documents", "Assetto Corsa", "cfg", fileName);
            if (File.Exists(path3)) return path3;

            return path1;
        }

        public string GetAcDocumentsDir()
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path1 = Path.Combine(myDocs, "Assetto Corsa");
            if (Directory.Exists(path1)) return path1;

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path2 = Path.Combine(userProfile, "OneDrive", "Documents", "Assetto Corsa");
            if (Directory.Exists(path2)) return path2;

            var path3 = Path.Combine(userProfile, "Documents", "Assetto Corsa");
            if (Directory.Exists(path3)) return path3;

            return path1;
        }

        public string GetNoHesiSettingsJsonPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "NoHesi", "settings.json");
        }

        // ================= AUDIO SETTINGS =================
        public async Task<AcAudioSettings> LoadAudioSettingsAsync()
        {
            return await Task.Run(() =>
            {
                var settings = new AcAudioSettings();
                var path = GetCfgPath("audio.ini");
                if (!File.Exists(path)) return settings;

                var ini = new IniModifier(path);

                if (double.TryParse(ini.GetValue("LEVELS", "MASTER"), NumberStyles.Any, CultureInfo.InvariantCulture, out double m))
                    settings.MasterVolume = m;
                if (double.TryParse(ini.GetValue("LEVELS", "ENGINE"), NumberStyles.Any, CultureInfo.InvariantCulture, out double e))
                    settings.EngineVolume = e;
                if (double.TryParse(ini.GetValue("LEVELS", "OPPONENTS"), NumberStyles.Any, CultureInfo.InvariantCulture, out double op))
                    settings.OpponentsVolume = op;
                if (double.TryParse(ini.GetValue("LEVELS", "TYRES"), NumberStyles.Any, CultureInfo.InvariantCulture, out double ty))
                    settings.TyresVolume = ty;
                if (double.TryParse(ini.GetValue("LEVELS", "WIND"), NumberStyles.Any, CultureInfo.InvariantCulture, out double w))
                    settings.WindVolume = w;
                if (double.TryParse(ini.GetValue("LEVELS", "BRAKES"), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
                    settings.BrakesVolume = b;
                if (double.TryParse(ini.GetValue("LEVELS", "SURFACES"), NumberStyles.Any, CultureInfo.InvariantCulture, out double s))
                    settings.SurfacesVolume = s;
                if (double.TryParse(ini.GetValue("LEVELS", "MUSIC"), NumberStyles.Any, CultureInfo.InvariantCulture, out double mu))
                    settings.MusicVolume = mu;
                if (double.TryParse(ini.GetValue("LEVELS", "UISOUNDS"), NumberStyles.Any, CultureInfo.InvariantCulture, out double ui))
                    settings.UiSoundsVolume = ui;
                if (double.TryParse(ini.GetValue("LEVELS_EXT", "WEATHER"), NumberStyles.Any, CultureInfo.InvariantCulture, out double we))
                    settings.WeatherVolume = we;

                return settings;
            });
        }

        public async Task SaveAudioSettingsAsync(AcAudioSettings settings)
        {
            await Task.Run(() =>
            {
                var path = GetCfgPath("audio.ini");
                if (!File.Exists(path)) return;

                var ini = new IniModifier(path);
                ini.SetValue("LEVELS", "MASTER", settings.MasterVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "ENGINE", settings.EngineVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "OPPONENTS", settings.OpponentsVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "TYRES", settings.TyresVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "WIND", settings.WindVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "BRAKES", settings.BrakesVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "SURFACES", settings.SurfacesVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "MUSIC", settings.MusicVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS", "UISOUNDS", settings.UiSoundsVolume.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("LEVELS_EXT", "WEATHER", settings.WeatherVolume.ToString("F2", CultureInfo.InvariantCulture));

                ini.Save();
            });
        }

        // ================= DRIVING ASSISTS =================
        public async Task<AcDrivingAssists> LoadDrivingAssistsAsync()
        {
            return await Task.Run(() =>
            {
                var assists = new AcDrivingAssists();
                var path = GetCfgPath("assists.ini");
                if (!File.Exists(path)) return assists;

                var ini = new IniModifier(path);

                if (int.TryParse(ini.GetValue("ASSISTS", "ABS"), out int abs)) assists.AbsMode = abs;
                if (int.TryParse(ini.GetValue("ASSISTS", "TRACTION_CONTROL"), out int tc)) assists.TcMode = tc;
                if (int.TryParse(ini.GetValue("ASSISTS", "STABILITY_CONTROL"), out int sc)) assists.StabilityControl = sc;

                assists.AutoClutch = ini.GetValue("ASSISTS", "AUTO_CLUTCH") == "1";
                assists.AutoBlip = ini.GetValue("ASSISTS", "AUTO_BLIP") == "1";
                assists.AutoShifter = ini.GetValue("ASSISTS", "AUTO_SHIFTER") == "1";
                assists.AutoBrake = ini.GetValue("ASSISTS", "AUTO_BRAKE") == "1";
                assists.IdealLine = ini.GetValue("ASSISTS", "IDEAL_LINE") == "1";
                assists.TyreBlankets = ini.GetValue("ASSISTS", "TYRE_BLANKETS") == "1";

                if (int.TryParse(ini.GetValue("ASSISTS", "DAMAGE"), out int dmg)) assists.Damage = dmg;
                if (int.TryParse(ini.GetValue("ASSISTS", "TYRE_WEAR"), out int tw)) assists.TyreWear = tw;
                if (int.TryParse(ini.GetValue("ASSISTS", "FUEL_RATE"), out int fr)) assists.FuelRate = fr;
                if (int.TryParse(ini.GetValue("ASSISTS", "SLIPSTREAM"), out int ss)) assists.SlipstreamMulti = ss;

                return assists;
            });
        }

        public async Task SaveDrivingAssistsAsync(AcDrivingAssists assists)
        {
            await Task.Run(() =>
            {
                var path = GetCfgPath("assists.ini");
                if (!File.Exists(path)) return;

                var ini = new IniModifier(path);
                ini.SetValue("ASSISTS", "ABS", assists.AbsMode.ToString());
                ini.SetValue("ASSISTS", "TRACTION_CONTROL", assists.TcMode.ToString());
                ini.SetValue("ASSISTS", "STABILITY_CONTROL", assists.StabilityControl.ToString());
                ini.SetValue("ASSISTS", "AUTO_CLUTCH", assists.AutoClutch ? "1" : "0");
                ini.SetValue("ASSISTS", "AUTO_BLIP", assists.AutoBlip ? "1" : "0");
                ini.SetValue("ASSISTS", "AUTO_SHIFTER", assists.AutoShifter ? "1" : "0");
                ini.SetValue("ASSISTS", "AUTO_BRAKE", assists.AutoBrake ? "1" : "0");
                ini.SetValue("ASSISTS", "IDEAL_LINE", assists.IdealLine ? "1" : "0");
                ini.SetValue("ASSISTS", "TYRE_BLANKETS", assists.TyreBlankets ? "1" : "0");
                ini.SetValue("ASSISTS", "DAMAGE", assists.Damage.ToString());
                ini.SetValue("ASSISTS", "TYRE_WEAR", assists.TyreWear.ToString());
                ini.SetValue("ASSISTS", "FUEL_RATE", assists.FuelRate.ToString());
                ini.SetValue("ASSISTS", "SLIPSTREAM", assists.SlipstreamMulti.ToString());

                ini.Save();
            });
        }

        // ================= SYSTEM & GAME FILES =================
        public async Task<AcSystemSettings> LoadSystemSettingsAsync(string acRoot)
        {
            return await Task.Run(() =>
            {
                var sys = new AcSystemSettings();

                // Read ManageOwnSettings from NoHesi settings if present
                var noHesiJson = GetNoHesiSettingsJsonPath();
                if (File.Exists(noHesiJson))
                {
                    try
                    {
                        var json = File.ReadAllText(noHesiJson);
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("ManageOwnSettings", out var prop))
                        {
                            sys.ManageOwnSettings = prop.GetBoolean();
                        }
                    }
                    catch { }
                }

                // Check Prerequisites
                sys.SteamRunning = Process.GetProcessesByName("steam").Length > 0;
                sys.AcInstalled = File.Exists(Path.Combine(acRoot, "AssettoCorsa.exe")) || File.Exists(Path.Combine(acRoot, "acs.exe"));
                sys.CspInstalled = File.Exists(Path.Combine(acRoot, "dwrite.dll"));

                var cmPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcTools Content Manager", "Content Manager.exe");
                sys.CmInstalled = File.Exists(cmPath) || Process.GetProcessesByName("Content Manager").Length > 0;

                var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
                sys.DirectXInstalled = File.Exists(Path.Combine(system32, "d3d11.dll")) && File.Exists(Path.Combine(system32, "dxgi.dll"));
                sys.VcRedistInstalled = File.Exists(Path.Combine(system32, "vcruntime140.dll")) || File.Exists(Path.Combine(system32, "msvcp140.dll"));

                // Calculate Cache Size
                long cacheBytes = 0;
                var acDocs = GetAcDocumentsDir();
                var cacheDir = Path.Combine(acDocs, "cache");
                if (Directory.Exists(cacheDir))
                {
                    try
                    {
                        cacheBytes += Directory.GetFiles(cacheDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
                    }
                    catch { }
                }

                var logsDir = Path.Combine(acDocs, "logs");
                if (Directory.Exists(logsDir))
                {
                    try
                    {
                        cacheBytes += Directory.GetFiles(logsDir, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
                    }
                    catch { }
                }

                sys.CacheSizeDisplay = cacheBytes > 1024 * 1024 * 1024
                    ? $"{(double)cacheBytes / (1024 * 1024 * 1024):F2} GB"
                    : $"{cacheBytes / (1024 * 1024)} MB";

                return sys;
            });
        }

        public async Task<string> CalculateTotalDiskUsageAsync(string acRoot)
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(acRoot)) return "0 GB";
                try
                {
                    long totalBytes = 0;
                    var di = new DirectoryInfo(acRoot);
                    foreach (var fi in di.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        totalBytes += fi.Length;
                    }

                    return totalBytes > 1024L * 1024L * 1024L
                        ? $"{(double)totalBytes / (1024L * 1024L * 1024L):F1} GB"
                        : $"{totalBytes / (1024L * 1024L)} MB";
                }
                catch
                {
                    return "Calculating (Large library)";
                }
            });
        }

        public async Task<(bool success, string message)> ClearGameCacheAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    int clearedFiles = 0;
                    long freedBytes = 0;
                    var acDocs = GetAcDocumentsDir();

                    var targetFolders = new[]
                    {
                        Path.Combine(acDocs, "cache"),
                        Path.Combine(acDocs, "logs"),
                        Path.Combine(acDocs, "screens_cache")
                    };

                    foreach (var dir in targetFolders)
                    {
                        if (Directory.Exists(dir))
                        {
                            foreach (var f in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                            {
                                try
                                {
                                    freedBytes += new FileInfo(f).Length;
                                    File.Delete(f);
                                    clearedFiles++;
                                }
                                catch { }
                            }
                        }
                    }

                    string sizeStr = freedBytes > 1024 * 1024
                        ? $"{freedBytes / (1024 * 1024)} MB"
                        : $"{freedBytes / 1024} KB";

                    return (true, $"Cache purged successfully: {clearedFiles} temporary files ({sizeStr}) removed.");
                }
                catch (Exception ex)
                {
                    return (false, $"Failed to clear cache: {ex.Message}");
                }
            });
        }

        public async Task SaveManageOwnSettingsAsync(bool manageOwn)
        {
            await Task.Run(() =>
            {
                var noHesiJson = GetNoHesiSettingsJsonPath();
                if (File.Exists(noHesiJson))
                {
                    try
                    {
                        var json = File.ReadAllText(noHesiJson);
                        using var doc = JsonDocument.Parse(json);
                        var dict = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(json);
                        if (dict != null)
                        {
                            dict["ManageOwnSettings"] = manageOwn;
                            var updated = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(noHesiJson, updated);
                        }
                    }
                    catch { }
                }
            });
        }
    }
}
