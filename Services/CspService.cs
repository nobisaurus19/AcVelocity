using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class CspService
    {
        public string GetExtensionConfigDir()
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path1 = Path.Combine(myDocs, "Assetto Corsa", "cfg", "extension");
            if (Directory.Exists(path1)) return path1;

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path2 = Path.Combine(userProfile, "OneDrive", "Documents", "Assetto Corsa", "cfg", "extension");
            if (Directory.Exists(path2)) return path2;

            var path3 = Path.Combine(userProfile, "Documents", "Assetto Corsa", "cfg", "extension");
            if (Directory.Exists(path3)) return path3;

            return path1;
        }

        public async Task<CspSettings> LoadSettingsAsync(string acRoot)
        {
            return await Task.Run(() =>
            {
                var settings = new CspSettings();
                var configDir = GetExtensionConfigDir();

                // 1. Detect Installation & Master Switch in AC Root
                if (!string.IsNullOrEmpty(acRoot) && Directory.Exists(acRoot))
                {
                    var dwritePath = Path.Combine(acRoot, "dwrite.dll");
                    var dwriteDisabledPath = Path.Combine(acRoot, "dwrite.dll.disabled");

                    if (File.Exists(dwritePath))
                    {
                        settings.IsCspInstalled = true;
                        settings.MasterEnabled = true;
                        try
                        {
                            var fi = new FileInfo(dwritePath);
                            settings.DwriteSizeMb = Math.Round(fi.Length / (1024.0 * 1024.0), 1);
                        }
                        catch { }
                    }
                    else if (File.Exists(dwriteDisabledPath))
                    {
                        settings.IsCspInstalled = true;
                        settings.MasterEnabled = false;
                        try
                        {
                            var fi = new FileInfo(dwriteDisabledPath);
                            settings.DwriteSizeMb = Math.Round(fi.Length / (1024.0 * 1024.0), 1);
                        }
                        catch { }
                    }

                    var installedLog = Path.Combine(acRoot, "extension", "installed.log");
                    if (File.Exists(installedLog))
                    {
                        try
                        {
                            var lines = File.ReadAllLines(installedLog);
                            foreach (var line in lines)
                            {
                                var l = line.Trim();
                                if (l.StartsWith("version:", StringComparison.OrdinalIgnoreCase))
                                {
                                    settings.CspVersion = l.Substring(8).Trim();
                                }
                                else if (l.StartsWith("build:", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (int.TryParse(l.Substring(6).Trim(), out int b))
                                    {
                                        settings.CspBuild = b;
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    // Dynamically scan available weather implementations and controllers
                    var weatherDir = Path.Combine(acRoot, "extension", "weather");
                    if (Directory.Exists(weatherDir))
                    {
                        foreach (var dir in Directory.GetDirectories(weatherDir))
                        {
                            settings.AvailableWeatherImplementations.Add(Path.GetFileName(dir));
                        }
                    }
                    if (settings.AvailableWeatherImplementations.Count == 0)
                    {
                        settings.AvailableWeatherImplementations.AddRange(new[] { "Pure LCS", "pure", "base" });
                    }

                    var ctrlDir = Path.Combine(acRoot, "extension", "weather-controllers");
                    if (Directory.Exists(ctrlDir))
                    {
                        foreach (var dir in Directory.GetDirectories(ctrlDir))
                        {
                            settings.AvailableWeatherControllers.Add(Path.GetFileName(dir));
                        }
                    }
                    if (settings.AvailableWeatherControllers.Count == 0)
                    {
                        settings.AvailableWeatherControllers.AddRange(new[] { "pureCtrl static", "pureCtrl", "base-live", "base" });
                    }
                }

                if (!Directory.Exists(configDir))
                {
                    return settings;
                }

                // 2. Weather FX
                var wfxIni = new IniModifier(Path.Combine(configDir, "weather_fx.ini"));
                settings.WeatherFxEnabled = wfxIni.GetValue("BASIC", "ENABLED") != "0";
                settings.WeatherController = wfxIni.GetValue("BASIC", "CONTROLLER") ?? "pureCtrl static";
                settings.WeatherImplementation = wfxIni.GetValue("BASIC", "IMPLEMENTATION") ?? "Pure LCS";
                settings.MirageReflections = wfxIni.GetValue("MISCELLANEOUS", "USE_MIRAGE_REFLECTIONS") != "0";

                // 3. Lighting FX
                var lfxIni = new IniModifier(Path.Combine(configDir, "lighting_fx.ini"));
                settings.LightingFxEnabled = lfxIni.GetValue("BASIC", "ENABLED") != "0";
                if (double.TryParse(lfxIni.GetValue("BASIC", "HEADLIGHTS_RANGE_MULT"), NumberStyles.Any, CultureInfo.InvariantCulture, out double hlRange))
                    settings.HeadlightsRangeMult = hlRange;

                var bMultStr = lfxIni.GetValue("BASIC", "BOUNCED_LIGHT_MULT");
                if (double.TryParse(bMultStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double bMult) && bMult > 0)
                {
                    settings.BouncedLight = true;
                    settings.BouncedLightMult = bMult;
                }
                else
                {
                    settings.BouncedLight = false;
                }

                settings.DynamicCarLights = lfxIni.GetValue("PERFORMANCE", "CARS_WITH_LIGHTS") != "0";
                settings.FullResShadows = lfxIni.GetValue("SHADOWS", "FULL_RESOLUTION") == "1";
                if (int.TryParse(lfxIni.GetValue("PERFORMANCE", "CARS_WITH_LIGHTS"), out int cwl) && cwl > 0)
                    settings.CarsWithLights = cwl;

                // 4. Extra FX
                var efxIni = new IniModifier(Path.Combine(configDir, "extra_fx.ini"));
                settings.ExtraFxEnabled = efxIni.GetValue("BASIC", "ENABLED") == "1";
                
                var sslrQStr = efxIni.GetValue("SSLR", "TRACING_QUALITY");
                if (int.TryParse(sslrQStr, out int sslrQ))
                {
                    settings.SslrTracingQuality = sslrQ;
                    settings.SslrEnabled = sslrQ > 0;
                }
                else
                {
                    settings.SslrEnabled = efxIni.GetValue("SSLR", "ENABLED") == "1" || efxIni.GetValue("SSLR", "FAST_BLENDING") == "1";
                }

                settings.SsgiEnabled = efxIni.GetValue("SSGI", "ENABLED") == "1";
                settings.HbaoEnabled = efxIni.GetValue("HBAO", "ENABLED") == "1";
                settings.TaaEnabled = efxIni.GetValue("TAA", "ENABLED") == "1";
                settings.MotionBlurEnabled = efxIni.GetValue("MOTION_BLUR", "ENABLED") == "1";
                if (double.TryParse(efxIni.GetValue("MOTION_BLUR", "MULT"), NumberStyles.Any, CultureInfo.InvariantCulture, out double mbMult))
                    settings.MotionBlurMult = mbMult;

                // 5. Rain FX
                var rfxIni = new IniModifier(Path.Combine(configDir, "rain_fx.ini"));
                settings.RainFxEnabled = rfxIni.GetValue("BASIC", "ENABLED") != "0";
                settings.ScreenDrops = rfxIni.GetValue("SCREEN_DROPS", "ENABLED") != "0";
                if (double.TryParse(rfxIni.GetValue("VISUAL_TWEAKS", "HITS_INTENSITY"), NumberStyles.Any, CultureInfo.InvariantCulture, out double hits))
                    settings.DropsIntensity = hits;
                if (double.TryParse(rfxIni.GetValue("RACING_LINE_DEV", "SPLASH_MULT"), NumberStyles.Any, CultureInfo.InvariantCulture, out double splash))
                    settings.SplashMult = splash;

                // 6. Neck FX
                var neckIni = new IniModifier(Path.Combine(configDir, "neck.ini"));
                settings.NeckFxEnabled = neckIni.GetValue("BASIC", "ENABLED") != "0";
                if (double.TryParse(neckIni.GetValue("LOOKAHEAD", "GAIN"), NumberStyles.Any, CultureInfo.InvariantCulture, out double lookGain))
                    settings.NeckLookaheadGain = lookGain;

                // 7. Smart Mirror
                var mirrorIni = new IniModifier(Path.Combine(configDir, "smart_mirror.ini"));
                settings.SmartMirrorEnabled = mirrorIni.GetValue("REAL_MIRRORS", "ENABLED") != "0";

                // 8. Grass FX
                var grassIni = new IniModifier(Path.Combine(configDir, "grass_fx.ini"));
                var gqStr = grassIni.GetValue("BASIC", "QUALITY");
                if (int.TryParse(gqStr, out int gq) && gq > 0)
                {
                    settings.GrassQuality = gq;
                    settings.GrassFxEnabled = true;
                }
                else
                {
                    settings.GrassFxEnabled = gqStr != "0";
                }

                // 9. Particles FX
                var partIni = new IniModifier(Path.Combine(configDir, "particles_fx.ini"));
                settings.ParticlesFxEnabled = partIni.GetValue("MIRAGEDISRUPTION", "ENABLED") != "0";

                // 10. Graphics Adjustments
                var gaIni = new IniModifier(Path.Combine(configDir, "graphics_adjustments.ini"));
                settings.GraphicsAdjustmentsEnabled = gaIni.GetValue("BASIC", "ENABLED") != "0";
                if (double.TryParse(gaIni.GetValue("ANTIALIASING", "FFXCAS_SHARPNESS"), NumberStyles.Any, CultureInfo.InvariantCulture, out double cas))
                    settings.FfxCasSharpness = cas;
                if (double.TryParse(gaIni.GetValue("LOD", "MULTIPLIER"), NumberStyles.Any, CultureInfo.InvariantCulture, out double lod))
                    settings.LodMultiplier = lod;

                return settings;
            });
        }

        public async Task SaveSettingsAsync(CspSettings settings, string acRoot)
        {
            await Task.Run(() =>
            {
                // 1. Handle Master CSP Toggle via dwrite.dll
                if (!string.IsNullOrEmpty(acRoot) && Directory.Exists(acRoot))
                {
                    var dwritePath = Path.Combine(acRoot, "dwrite.dll");
                    var dwriteDisabledPath = Path.Combine(acRoot, "dwrite.dll.disabled");

                    if (!settings.MasterEnabled && File.Exists(dwritePath))
                    {
                        try { File.Move(dwritePath, dwriteDisabledPath, true); } catch { }
                    }
                    else if (settings.MasterEnabled && File.Exists(dwriteDisabledPath))
                    {
                        try { File.Move(dwriteDisabledPath, dwritePath, true); } catch { }
                    }
                }

                var configDir = GetExtensionConfigDir();
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // 2. Weather FX
                var wfxIni = new IniModifier(Path.Combine(configDir, "weather_fx.ini"));
                wfxIni.SetValue("BASIC", "ENABLED", settings.WeatherFxEnabled ? "1" : "0");
                wfxIni.SetValue("BASIC", "CONTROLLER", settings.WeatherController);
                wfxIni.SetValue("BASIC", "IMPLEMENTATION", settings.WeatherImplementation);
                wfxIni.SetValue("MISCELLANEOUS", "USE_MIRAGE_REFLECTIONS", settings.MirageReflections ? "1" : "0");
                wfxIni.Save();

                // 3. Lighting FX
                var lfxIni = new IniModifier(Path.Combine(configDir, "lighting_fx.ini"));
                lfxIni.SetValue("BASIC", "ENABLED", settings.LightingFxEnabled ? "1" : "0");
                lfxIni.SetValue("BASIC", "HEADLIGHTS_RANGE_MULT", settings.HeadlightsRangeMult.ToString("F2", CultureInfo.InvariantCulture));
                
                // Preserve user's custom bounced light multiplier when enabled
                double targetBounced = settings.BouncedLight ? (settings.BouncedLightMult > 0 ? settings.BouncedLightMult : 0.25) : 0.0;
                lfxIni.SetValue("BASIC", "BOUNCED_LIGHT_MULT", targetBounced.ToString("F2", CultureInfo.InvariantCulture));
                lfxIni.SetValue("SHADOWS", "FULL_RESOLUTION", settings.FullResShadows ? "1" : "0");
                lfxIni.SetValue("PERFORMANCE", "CARS_WITH_LIGHTS", settings.DynamicCarLights ? Math.Max(settings.CarsWithLights, 4).ToString() : "0");
                lfxIni.Save();

                // 4. Extra FX
                var efxIni = new IniModifier(Path.Combine(configDir, "extra_fx.ini"));
                efxIni.SetValue("BASIC", "ENABLED", settings.ExtraFxEnabled ? "1" : "0");
                
                int targetSslr = settings.SslrEnabled ? (settings.SslrTracingQuality > 0 ? settings.SslrTracingQuality : 2) : 0;
                efxIni.SetValue("SSLR", "TRACING_QUALITY", targetSslr.ToString());
                efxIni.SetValue("SSLR", "ENABLED", settings.SslrEnabled ? "1" : "0");
                efxIni.SetValue("SSGI", "ENABLED", settings.SsgiEnabled ? "1" : "0");
                efxIni.SetValue("HBAO", "ENABLED", settings.HbaoEnabled ? "1" : "0");
                efxIni.SetValue("TAA", "ENABLED", settings.TaaEnabled ? "1" : "0");
                efxIni.SetValue("MOTION_BLUR", "ENABLED", settings.MotionBlurEnabled ? "1" : "0");
                efxIni.SetValue("MOTION_BLUR", "MULT", settings.MotionBlurMult.ToString("F2", CultureInfo.InvariantCulture));
                efxIni.Save();

                // 5. Rain FX
                var rfxIni = new IniModifier(Path.Combine(configDir, "rain_fx.ini"));
                rfxIni.SetValue("BASIC", "ENABLED", settings.RainFxEnabled ? "1" : "0");
                rfxIni.SetValue("SCREEN_DROPS", "ENABLED", (settings.RainFxEnabled && settings.ScreenDrops) ? "1" : "0");
                rfxIni.SetValue("VISUAL_TWEAKS", "HITS_INTENSITY", settings.DropsIntensity.ToString("F2", CultureInfo.InvariantCulture));
                rfxIni.SetValue("RACING_LINE_DEV", "SPLASH_MULT", settings.SplashMult.ToString("F2", CultureInfo.InvariantCulture));
                rfxIni.Save();

                // 6. Neck FX
                var neckIni = new IniModifier(Path.Combine(configDir, "neck.ini"));
                neckIni.SetValue("BASIC", "ENABLED", settings.NeckFxEnabled ? "1" : "0");
                neckIni.SetValue("LOOKAHEAD", "GAIN", settings.NeckLookaheadGain.ToString("F2", CultureInfo.InvariantCulture));
                neckIni.Save();

                // 7. Smart Mirror
                var mirrorIni = new IniModifier(Path.Combine(configDir, "smart_mirror.ini"));
                mirrorIni.SetValue("REAL_MIRRORS", "ENABLED", settings.SmartMirrorEnabled ? "1" : "0");
                mirrorIni.Save();

                // 8. Grass FX
                var grassIni = new IniModifier(Path.Combine(configDir, "grass_fx.ini"));
                int targetGrassQ = settings.GrassFxEnabled ? (settings.GrassQuality > 0 ? settings.GrassQuality : 4) : 0;
                grassIni.SetValue("BASIC", "QUALITY", targetGrassQ.ToString());
                grassIni.Save();

                // 9. Particles FX
                var partIni = new IniModifier(Path.Combine(configDir, "particles_fx.ini"));
                partIni.SetValue("MIRAGEDISRUPTION", "ENABLED", settings.ParticlesFxEnabled ? "1" : "0");
                partIni.Save();

                // 10. Graphics Adjustments
                var gaIni = new IniModifier(Path.Combine(configDir, "graphics_adjustments.ini"));
                gaIni.SetValue("BASIC", "ENABLED", settings.GraphicsAdjustmentsEnabled ? "1" : "0");
                gaIni.SetValue("ANTIALIASING", "FFXCAS_SHARPNESS", settings.FfxCasSharpness.ToString("F2", CultureInfo.InvariantCulture));
                gaIni.SetValue("LOD", "MULTIPLIER", settings.LodMultiplier.ToString("F2", CultureInfo.InvariantCulture));
                gaIni.Save();
            });
        }

        public async Task<(bool Success, string Message)> InstallCspZipAsync(string zipFilePath, string acRoot)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(zipFilePath)) return (false, "File not found.");
                if (string.IsNullOrEmpty(acRoot) || !Directory.Exists(acRoot)) return (false, "Assetto Corsa root folder not found.");

                try
                {
                    using var archive = ZipFile.OpenRead(zipFilePath);
                    
                    // Robust root detection: handle packages with top-level wrapper directories
                    string prefix = string.Empty;
                    var dwriteEntry = archive.Entries.FirstOrDefault(e => e.Name.Equals("dwrite.dll", StringComparison.OrdinalIgnoreCase));
                    if (dwriteEntry != null)
                    {
                        var dir = Path.GetDirectoryName(dwriteEntry.FullName)?.Replace('\\', '/') ?? "";
                        if (!string.IsNullOrEmpty(dir)) prefix = dir + "/";
                    }
                    else
                    {
                        var extEntry = archive.Entries.FirstOrDefault(e => 
                            e.FullName.Contains("extension/", StringComparison.OrdinalIgnoreCase) || 
                            e.FullName.Contains("extension\\", StringComparison.OrdinalIgnoreCase));
                        
                        if (extEntry != null)
                        {
                            int idx = extEntry.FullName.IndexOf("extension", StringComparison.OrdinalIgnoreCase);
                            if (idx > 0) prefix = extEntry.FullName.Substring(0, idx);
                        }
                    }

                    int extractedCount = 0;
                    foreach (var entry in archive.Entries)
                    {
                        string relative = entry.FullName;
                        if (!string.IsNullOrEmpty(prefix) && relative.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            relative = relative.Substring(prefix.Length);
                        }

                        if (string.IsNullOrEmpty(relative) || relative.EndsWith("/") || relative.EndsWith("\\")) continue;

                        var targetPath = Path.Combine(acRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                        var targetDir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }

                        entry.ExtractToFile(targetPath, overwrite: true);
                        extractedCount++;
                    }

                    if (extractedCount == 0)
                    {
                        return (false, "No valid Custom Shaders Patch payload found inside archive.");
                    }

                    return (true, $"Custom Shaders Patch build successfully installed ({extractedCount} components updated).");
                }
                catch (Exception ex)
                {
                    return (false, $"Failed to unpack CSP build: {ex.Message}");
                }
            });
        }
    }

    public class IniModifier
    {
        private readonly List<string> _lines;
        private readonly string _filePath;

        public IniModifier(string filePath)
        {
            _filePath = filePath;
            _lines = File.Exists(filePath) ? File.ReadAllLines(filePath).ToList() : new List<string>();
        }

        public string? GetValue(string section, string key)
        {
            int curSection = -1;
            string targetSection = $"[{section.Trim().ToUpperInvariant()}]";

            for (int i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i].Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (line.ToUpperInvariant() == targetSection) curSection = i;
                    else if (curSection != -1) break; // leaving section
                }
                else if (curSection != -1)
                {
                    var eq = line.IndexOf('=');
                    if (eq > 0)
                    {
                        var k = line.Substring(0, eq).Trim().ToUpperInvariant();
                        if (k == key.ToUpperInvariant())
                        {
                            return line.Substring(eq + 1).Trim();
                        }
                    }
                }
            }
            return null;
        }

        public void SetValue(string section, string key, string value)
        {
            string targetSection = $"[{section.Trim().ToUpperInvariant()}]";
            int sectionIndex = -1;
            int nextSectionIndex = -1;

            for (int i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i].Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (line.ToUpperInvariant() == targetSection)
                    {
                        sectionIndex = i;
                    }
                    else if (sectionIndex != -1)
                    {
                        nextSectionIndex = i;
                        break;
                    }
                }
                else if (sectionIndex != -1)
                {
                    var eq = line.IndexOf('=');
                    if (eq > 0)
                    {
                        var k = line.Substring(0, eq).Trim().ToUpperInvariant();
                        if (k == key.ToUpperInvariant())
                        {
                            _lines[i] = $"{key}={value}";
                            return;
                        }
                    }
                }
            }

            // Key not found in section
            if (sectionIndex != -1)
            {
                int insertAt = nextSectionIndex != -1 ? nextSectionIndex : _lines.Count;
                _lines.Insert(insertAt, $"{key}={value}");
            }
            else
            {
                // Create section
                _lines.Add("");
                _lines.Add($"[{section}]");
                _lines.Add($"{key}={value}");
            }
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Automatic Safety Backup: create .bak before modifying
            if (File.Exists(_filePath))
            {
                var bakPath = _filePath + ".bak";
                try
                {
                    File.Copy(_filePath, bakPath, overwrite: true);
                }
                catch { }
            }

            File.WriteAllLines(_filePath, _lines);
        }
    }
}
