using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AcVelocity.Services
{
    public class AcVideoSettings
    {
        public bool Fullscreen { get; set; } = true;
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public int RefreshRate { get; set; } = 144;
        public int Msaa { get; set; } = 4;
        public int ShadowMapSize { get; set; } = 2048;
        public int FpsCap { get; set; } = 0; // 0 = unlimited
        public bool VSync { get; set; } = false;
        public bool Fxaa { get; set; } = true;
        public int SmokeLevel { get; set; } = 2;

        public string DisplayResolution => $"{Width}x{Height} @ {RefreshRate}Hz";
    }

    public class AcVideoSettingsService
    {
        public string GetVideoIniPath()
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(myDocs, "Assetto Corsa", "cfg", "video.ini");
        }

        public async Task<AcVideoSettings> LoadSettingsAsync()
        {
            return await Task.Run(() =>
            {
                var settings = new AcVideoSettings();
                var path = GetVideoIniPath();

                if (!File.Exists(path))
                {
                    return settings;
                }

                try
                {
                    var lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith(";") || trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed)) continue;

                        var parts = trimmed.Split('=', 2);
                        if (parts.Length != 2) continue;

                        var key = parts[0].Trim().ToUpperInvariant();
                        var val = parts[1].Trim();

                        switch (key)
                        {
                            case "FULLSCREEN":
                                settings.Fullscreen = val == "1";
                                break;
                            case "WIDTH":
                                if (int.TryParse(val, out int w)) settings.Width = w;
                                break;
                            case "HEIGHT":
                                if (int.TryParse(val, out int h)) settings.Height = h;
                                break;
                            case "REFRESH":
                                if (int.TryParse(val, out int r)) settings.RefreshRate = r;
                                break;
                            case "AASAMPLES":
                                if (int.TryParse(val, out int aa)) settings.Msaa = aa;
                                break;
                            case "SHADOW_MAP_SIZE":
                                if (int.TryParse(val, out int sh)) settings.ShadowMapSize = sh;
                                break;
                            case "FPS_CAP_MS":
                                if (int.TryParse(val, out int fps)) settings.FpsCap = fps;
                                break;
                            case "VSYNC":
                                settings.VSync = val == "1";
                                break;
                            case "FXAA":
                                settings.Fxaa = val == "1";
                                break;
                            case "SMOKE":
                                if (int.TryParse(val, out int sm)) settings.SmokeLevel = sm;
                                break;
                        }
                    }
                }
                catch { }

                return settings;
            });
        }

        public async Task SaveSettingsAsync(AcVideoSettings s)
        {
            var path = GetVideoIniPath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var sb = new StringBuilder();
            sb.AppendLine("[VIDEO]");
            sb.AppendLine($"FULLSCREEN={(s.Fullscreen ? 1 : 0)}");
            sb.AppendLine($"WIDTH={s.Width}");
            sb.AppendLine($"HEIGHT={s.Height}");
            sb.AppendLine($"REFRESH={s.RefreshRate}");
            sb.AppendLine($"AASAMPLES={s.Msaa}");
            sb.AppendLine($"VSYNC={(s.VSync ? 1 : 0)}");
            sb.AppendLine("AAQUALITY=0");
            sb.AppendLine($"SHADOW_MAP_SIZE={s.ShadowMapSize}");
            sb.AppendLine("DISABLE_HDR=0");
            sb.AppendLine($"FPS_CAP_MS={s.FpsCap}");
            sb.AppendLine();

            sb.AppendLine("[EFFECTS]");
            sb.AppendLine("MOTION_BLUR=0");
            sb.AppendLine($"FXAA={(s.Fxaa ? 1 : 0)}");
            sb.AppendLine($"SMOKE={s.SmokeLevel}");
            sb.AppendLine("RENDER_SMOKE_IN_MIRROR=0");
            sb.AppendLine();

            sb.AppendLine("[POST_PROCESS]");
            sb.AppendLine("ENABLED=1");
            sb.AppendLine("BRIGHTNESS=100");
            sb.AppendLine("EXPOSURE=25");

            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
