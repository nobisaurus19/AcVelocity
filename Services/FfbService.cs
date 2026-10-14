using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class FfbService
    {
        public string GetControlsIniPath()
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path1 = Path.Combine(myDocs, "Assetto Corsa", "cfg", "controls.ini");
            if (File.Exists(path1)) return path1;

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path2 = Path.Combine(userProfile, "OneDrive", "Documents", "Assetto Corsa", "cfg", "controls.ini");
            if (File.Exists(path2)) return path2;

            var path3 = Path.Combine(userProfile, "Documents", "Assetto Corsa", "cfg", "controls.ini");
            if (File.Exists(path3)) return path3;

            return path1;
        }

        public async Task<FfbSettings> LoadSettingsAsync()
        {
            return await Task.Run(() =>
            {
                var settings = new FfbSettings();
                var path = GetControlsIniPath();
                if (!File.Exists(path)) return settings;

                var ini = new IniModifier(path);

                // Input Method & Controller Detection
                var method = ini.GetValue("HEADER", "INPUT_METHOD");
                if (!string.IsNullOrEmpty(method)) settings.InputMethod = method.ToUpperInvariant();

                var devName = ini.GetValue("CONTROLLERS", "CON0");
                if (!string.IsNullOrEmpty(devName)) settings.ConnectedDeviceName = devName;

                // Master Gain & Filter
                var gainStr = ini.GetValue("STEER", "FF_GAIN");
                if (double.TryParse(gainStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double gain))
                    settings.MasterGain = gain;

                var filterStr = ini.GetValue("STEER", "FILTER_FF");
                if (double.TryParse(filterStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double filter))
                    settings.Filter = filter;

                var lockStr = ini.GetValue("STEER", "LOCK");
                if (int.TryParse(lockStr, out int lk))
                    settings.SteeringLock = lk;

                // Pedals Gamma
                var throttleGamma = ini.GetValue("THROTTLE", "GAMMA");
                if (double.TryParse(throttleGamma, NumberStyles.Any, CultureInfo.InvariantCulture, out double tg))
                    settings.ThrottleGamma = tg;

                var brakesGamma = ini.GetValue("BRAKES", "GAMMA");
                if (double.TryParse(brakesGamma, NumberStyles.Any, CultureInfo.InvariantCulture, out double bg))
                    settings.BrakesGamma = bg;

                var clutchGamma = ini.GetValue("CLUTCH", "GAMMA");
                if (double.TryParse(clutchGamma, NumberStyles.Any, CultureInfo.InvariantCulture, out double cg))
                    settings.ClutchGamma = cg;

                var handbrakeGamma = ini.GetValue("HANDBRAKE", "GAMMA");
                if (double.TryParse(handbrakeGamma, NumberStyles.Any, CultureInfo.InvariantCulture, out double hbg))
                    settings.HandbrakeGamma = hbg;

                // Shifter Mode
                settings.HShifterActive = ini.GetValue("SHIFTER", "ACTIVE") == "1";

                // Gamepad (X360) Settings
                var speedSens = ini.GetValue("X360", "SPEED_SENSITIVITY");
                if (double.TryParse(speedSens, NumberStyles.Any, CultureInfo.InvariantCulture, out double ss))
                    settings.GamepadSpeedSensitivity = ss;

                var steerSpd = ini.GetValue("X360", "STEER_SPEED");
                if (double.TryParse(steerSpd, NumberStyles.Any, CultureInfo.InvariantCulture, out double sspd))
                    settings.GamepadSteerSpeed = sspd;

                var deadzone = ini.GetValue("X360", "STEER_DEADZONE");
                if (double.TryParse(deadzone, NumberStyles.Any, CultureInfo.InvariantCulture, out double dz))
                    settings.GamepadSteerDeadzone = dz;

                var steerFilter = ini.GetValue("X360", "STEER_FILTER");
                if (double.TryParse(steerFilter, NumberStyles.Any, CultureInfo.InvariantCulture, out double sf))
                    settings.GamepadSteerFilter = sf;

                var steerGamma = ini.GetValue("X360", "STEER_GAMMA");
                if (double.TryParse(steerGamma, NumberStyles.Any, CultureInfo.InvariantCulture, out double sg))
                    settings.GamepadSteerGamma = sg;

                var rumble = ini.GetValue("X360", "RUMBLE_INTENSITY");
                if (double.TryParse(rumble, NumberStyles.Any, CultureInfo.InvariantCulture, out double rb))
                    settings.GamepadRumbleIntensity = rb;

                // FFB Tweaks
                var minStr = ini.GetValue("FF_TWEAKS", "MIN_FF");
                if (double.TryParse(minStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double minF))
                    settings.MinForce = minF;

                // Vibrations & Enhancements
                var curbsStr = ini.GetValue("FF_ENHANCEMENT", "CURBS");
                if (double.TryParse(curbsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double curbs))
                    settings.KerbsEffect = curbs;

                var roadStr = ini.GetValue("FF_ENHANCEMENT", "ROAD");
                if (double.TryParse(roadStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double road))
                    settings.RoadEffect = road;

                var slipsStr = ini.GetValue("FF_ENHANCEMENT", "SLIPS");
                if (double.TryParse(slipsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double slips))
                    settings.SlipEffect = slips;

                var absStr = ini.GetValue("FF_ENHANCEMENT", "ABS");
                if (double.TryParse(absStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double abs))
                    settings.AbsEffect = abs;

                settings.EnhancedUndersteer = ini.GetValue("FF_ENHANCEMENT_2", "UNDERSTEER") == "1";

                // Quick Button Mapping Detection
                var gUp = ini.GetValue("GEARUP", "BUTTON");
                if (!string.IsNullOrEmpty(gUp) && gUp != "-1") settings.ButtonGearUp = $"B{gUp} (Right Paddle)";

                var gDn = ini.GetValue("GEARDN", "BUTTON");
                if (!string.IsNullOrEmpty(gDn) && gDn != "-1") settings.ButtonGearDown = $"B{gDn} (Left Paddle)";

                var hl = ini.GetValue("ACTION_HEADLIGHTS", "BUTTON");
                if (!string.IsNullOrEmpty(hl) && hl != "-1") settings.ButtonHeadlights = $"B{hl}";

                var hlf = ini.GetValue("ACTION_HEADLIGHTS_FLASH", "BUTTON");
                if (!string.IsNullOrEmpty(hlf) && hlf != "-1") settings.ButtonHeadlightsFlash = $"B{hlf}";

                var horn = ini.GetValue("ACTION_HORN", "BUTTON");
                if (!string.IsNullOrEmpty(horn) && horn != "-1") settings.ButtonHorn = $"B{horn}";

                var cam = ini.GetValue("ACTION_CHANGE_CAMERA", "BUTTON");
                if (!string.IsNullOrEmpty(cam) && cam != "-1") settings.ButtonChangeCamera = $"B{cam}";

                var tcUp = ini.GetValue("TCUP", "BUTTON");
                if (!string.IsNullOrEmpty(tcUp) && tcUp != "-1") settings.ButtonTcUp = $"B{tcUp}";

                var tcDn = ini.GetValue("TCDN", "BUTTON");
                if (!string.IsNullOrEmpty(tcDn) && tcDn != "-1") settings.ButtonTcDown = $"B{tcDn}";

                var absUp = ini.GetValue("ABSUP", "BUTTON");
                if (!string.IsNullOrEmpty(absUp) && absUp != "-1") settings.ButtonAbsUp = $"B{absUp}";

                var absDn = ini.GetValue("ABSDN", "BUTTON");
                if (!string.IsNullOrEmpty(absDn) && absDn != "-1") settings.ButtonAbsDown = $"B{absDn}";

                var mgukUp = ini.GetValue("MGUK_DELIVERY_UP", "BUTTON");
                if (!string.IsNullOrEmpty(mgukUp) && mgukUp != "-1") settings.ButtonMgukUp = $"B{mgukUp}";

                var mgukDn = ini.GetValue("MGUK_DELIVERY_DN", "BUTTON");
                if (!string.IsNullOrEmpty(mgukDn) && mgukDn != "-1") settings.ButtonMgukDown = $"B{mgukDn}";

                return settings;
            });
        }

        public async Task SaveSettingsAsync(FfbSettings settings)
        {
            await Task.Run(() =>
            {
                var path = GetControlsIniPath();
                if (!File.Exists(path)) return;

                var ini = new IniModifier(path);

                // Input Method
                ini.SetValue("HEADER", "INPUT_METHOD", settings.InputMethod);

                // Steer & FFB
                ini.SetValue("STEER", "FF_GAIN", settings.MasterGain.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("STEER", "FILTER_FF", settings.Filter.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("STEER", "LOCK", settings.SteeringLock.ToString());

                // Pedals Gamma
                ini.SetValue("THROTTLE", "GAMMA", settings.ThrottleGamma.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("BRAKES", "GAMMA", settings.BrakesGamma.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("CLUTCH", "GAMMA", settings.ClutchGamma.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("HANDBRAKE", "GAMMA", settings.HandbrakeGamma.ToString("F2", CultureInfo.InvariantCulture));

                // Shifter Mode
                ini.SetValue("SHIFTER", "ACTIVE", settings.HShifterActive ? "1" : "0");

                // Gamepad (X360)
                ini.SetValue("X360", "SPEED_SENSITIVITY", settings.GamepadSpeedSensitivity.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("X360", "STEER_SPEED", settings.GamepadSteerSpeed.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("X360", "STEER_DEADZONE", settings.GamepadSteerDeadzone.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("X360", "STEER_FILTER", settings.GamepadSteerFilter.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("X360", "STEER_GAMMA", settings.GamepadSteerGamma.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("X360", "RUMBLE_INTENSITY", settings.GamepadRumbleIntensity.ToString("F2", CultureInfo.InvariantCulture));

                // FFB Enhancements
                ini.SetValue("FF_TWEAKS", "MIN_FF", settings.MinForce.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("FF_ENHANCEMENT", "CURBS", settings.KerbsEffect.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("FF_ENHANCEMENT", "ROAD", settings.RoadEffect.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("FF_ENHANCEMENT", "SLIPS", settings.SlipEffect.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("FF_ENHANCEMENT", "ABS", settings.AbsEffect.ToString("F2", CultureInfo.InvariantCulture));
                ini.SetValue("FF_ENHANCEMENT_2", "UNDERSTEER", settings.EnhancedUndersteer ? "1" : "0");

                ini.Save();
            });
        }
    }
}
