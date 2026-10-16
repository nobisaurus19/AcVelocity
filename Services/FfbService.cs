using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public string GetSavedSetupsDir()
        {
            var cfgDir = Path.GetDirectoryName(GetControlsIniPath()) ?? string.Empty;
            var dir = Path.Combine(cfgDir, "controllers", "savedsetups");
            if (!Directory.Exists(dir))
            {
                try { Directory.CreateDirectory(dir); } catch { }
            }
            return dir;
        }

        public string GetPresetsDir()
        {
            var cfgDir = Path.GetDirectoryName(GetControlsIniPath()) ?? string.Empty;
            var dir = Path.Combine(cfgDir, "controllers", "presets");
            if (!Directory.Exists(dir))
            {
                try { Directory.CreateDirectory(dir); } catch { }
            }
            return dir;
        }

        public List<ControlActionBinding> GetDefaultActionCatalog()
        {
            return new List<ControlActionBinding>
            {
                // === 1. SHIFTER & GEARS (14 Actions) ===
                new() { SectionName = "GEARUP", DisplayName = "Gear Up (Shift Up)", ThaiDescription = "เปลี่ยนเกียร์ขึ้น (Paddle Shift ขวา หรือ ปุ่ม)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "GEARDN", DisplayName = "Gear Down (Shift Down)", ThaiDescription = "เปลี่ยนเกียร์ลง (Paddle Shift ซ้าย หรือ ปุ่ม)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_1", DisplayName = "H-Shifter: Gear 1", ThaiDescription = "เข้าเกียร์ 1 (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_2", DisplayName = "H-Shifter: Gear 2", ThaiDescription = "เข้าเกียร์ 2 (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_3", DisplayName = "H-Shifter: Gear 3", ThaiDescription = "เข้าเกียร์ 3 (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_4", DisplayName = "H-Shifter: Gear 4", ThaiDescription = "เข้าเกียร์ 4 (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_5", DisplayName = "H-Shifter: Gear 5", ThaiDescription = "เข้าเกียร์ 5 (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_6", DisplayName = "H-Shifter: Gear 6", ThaiDescription = "เข้าเกียร์ 6 (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_7", DisplayName = "H-Shifter: Gear 7", ThaiDescription = "เข้าเกียร์ 7 (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "SHIFTER:GEAR_R", DisplayName = "H-Shifter: Reverse Gear", ThaiDescription = "เข้าเกียร์ถอยหลัง R (H-Pattern)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "HANDBRAKE", DisplayName = "Handbrake", ThaiDescription = "เบรกมือ (Handbrake)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "AUTO_SHIFTER", DisplayName = "Automatic Shifter Toggle", ThaiDescription = "สลับโหมดเกียร์อัตโนมัติ / แมนนวล", Category = "SHIFTER & GEARS" },
                new() { SectionName = "__EXT_GEAR_NEUTRAL", DisplayName = "Neutral Gear (CSP)", ThaiDescription = "เข้าเกียร์ว่าง N (CSP Extra)", Category = "SHIFTER & GEARS" },
                new() { SectionName = "__EXT_SWITCH_HSHIFTER", DisplayName = "Toggle H-Shifter / Sequential (CSP)", ThaiDescription = "สลับระหว่างเกียร์กระปุกและเกียร์ซีเควนเชียล", Category = "SHIFTER & GEARS" },

                // === 2. LIGHTS & COCKPIT (13 Actions) ===
                new() { SectionName = "ACTION_HEADLIGHTS", DisplayName = "Headlights On/Off", ThaiDescription = "เปิด/ปิด ไฟหน้าหลัก", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "ACTION_HEADLIGHTS_FLASH", DisplayName = "Flash High Beams", ThaiDescription = "กระพริบไฟสูง (Flash Pass)", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_LOW_BEAM", DisplayName = "Low Beam / Fog Lights (CSP)", ThaiDescription = "ไฟตัดหมอก / ไฟหรี่ (CSP)", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_HAZARDS", DisplayName = "Hazard Lights (CSP)", ThaiDescription = "ไฟฉุกเฉิน / ไฟผ่าหมาก", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_TURNSIGNAL_LEFT", DisplayName = "Turn Signal Left (CSP)", ThaiDescription = "ไฟเลี้ยวซ้าย", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_TURNSIGNAL_RIGHT", DisplayName = "Turn Signal Right (CSP)", ThaiDescription = "ไฟเลี้ยวขวา", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_TURNSIGNAL_CANCEL", DisplayName = "Cancel Turn Signals (CSP)", ThaiDescription = "ยกเลิก/ปิดสัญญาณไฟเลี้ยว", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "ACTION_HORN", DisplayName = "Horn", ThaiDescription = "แตรสัญญาณ", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_WIPERS_MORE", DisplayName = "Wipers Faster (CSP)", ThaiDescription = "เพิ่มความเร็วที่ปัดน้ำฝน", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_WIPERS_LESS", DisplayName = "Wipers Slower (CSP)", ThaiDescription = "ลดความเร็วที่ปัดน้ำฝน", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_WIPERS_OFF", DisplayName = "Wipers Off (CSP)", ThaiDescription = "ปิดที่ปัดน้ำฝน", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "STARTER", DisplayName = "Engine Ignition / Starter", ThaiDescription = "สตาร์ทเครื่องยนต์", Category = "LIGHTS & COCKPIT" },
                new() { SectionName = "__EXT_STARTER", DisplayName = "CSP Extended Starter Button", ThaiDescription = "ปุ่มสตาร์ทเครื่องยนต์เสริม CSP", Category = "LIGHTS & COCKPIT" },

                // === 3. CSP EXTRAS (8 Actions) ===
                new() { SectionName = "__EXT_LIGHT_A", DisplayName = "CSP Extra Option A / Light A", ThaiDescription = "ปุ่มฟังก์ชันเสริม CSP Extra A (เปิดประตู, ไฟตกแต่ง, หลังคาเปิด)", Category = "CSP EXTRAS" },
                new() { SectionName = "__EXT_LIGHT_B", DisplayName = "CSP Extra Option B / Light B", ThaiDescription = "ปุ่มฟังก์ชันเสริม CSP Extra B (ไฟในเก๋ง, สปอยเลอร์)", Category = "CSP EXTRAS" },
                new() { SectionName = "__EXT_LIGHT_C", DisplayName = "CSP Extra Option C / Light C", ThaiDescription = "ปุ่มฟังก์ชันเสริม CSP Extra C (แอนิเมชันพิเศษ)", Category = "CSP EXTRAS" },
                new() { SectionName = "__EXT_LIGHT_D", DisplayName = "CSP Extra Option D / Light D", ThaiDescription = "ปุ่มฟังก์ชันเสริม CSP Extra D", Category = "CSP EXTRAS" },
                new() { SectionName = "__EXT_LIGHT_E", DisplayName = "CSP Extra Option E / Light E", ThaiDescription = "ปุ่มฟังก์ชันเสริม CSP Extra E", Category = "CSP EXTRAS" },
                new() { SectionName = "__EXT_LIGHT_F", DisplayName = "CSP Extra Option F / Light F", ThaiDescription = "ปุ่มฟังก์ชันเสริม CSP Extra F", Category = "CSP EXTRAS" },
                new() { SectionName = "__EXT_VIRTUAL_MIRROR", DisplayName = "Virtual Mirror Toggle (CSP)", ThaiDescription = "เปิด/ปิด กระจกมองหลังเสมือนจริง", Category = "CSP EXTRAS" },
                new() { SectionName = "__EXT_HIDE_UI", DisplayName = "Hide / Show HUD UI (CSP)", ThaiDescription = "ซ่อน / แสดง หน้าจอ HUD UI", Category = "CSP EXTRAS" },

                // === 4. ELECTRONICS (19 Actions) ===
                new() { SectionName = "TCUP", DisplayName = "Traction Control Up (TC+)", ThaiDescription = "เพิ่มระดับ Traction Control (TC+)", Category = "ELECTRONICS" },
                new() { SectionName = "TCDN", DisplayName = "Traction Control Down (TC-)", ThaiDescription = "ลดระดับ Traction Control (TC-)", Category = "ELECTRONICS" },
                new() { SectionName = "ABSUP", DisplayName = "ABS Level Up (ABS+)", ThaiDescription = "เพิ่มระดับเบรก ABS (ABS+)", Category = "ELECTRONICS" },
                new() { SectionName = "ABSDN", DisplayName = "ABS Level Down (ABS-)", ThaiDescription = "ลดระดับเบรก ABS (ABS-)", Category = "ELECTRONICS" },
                new() { SectionName = "TURBOUP", DisplayName = "Turbo Boost Up", ThaiDescription = "เพิ่มบูสต์เทอร์โบ (Turbo+)", Category = "ELECTRONICS" },
                new() { SectionName = "TURBODN", DisplayName = "Turbo Boost Down", ThaiDescription = "ลดบูสต์เทอร์โบ (Turbo-)", Category = "ELECTRONICS" },
                new() { SectionName = "BALANCEUP", DisplayName = "Brake Bias Forward", ThaiDescription = "ปรับสมดุลเบรกไปข้างหน้า (Bias Front)", Category = "ELECTRONICS" },
                new() { SectionName = "BALANCEDN", DisplayName = "Brake Bias Rearward", ThaiDescription = "ปรับสมดุลเบรกไปข้างหลัง (Bias Rear)", Category = "ELECTRONICS" },
                new() { SectionName = "ENGINE_BRAKE_UP", DisplayName = "Engine Brake Up", ThaiDescription = "เพิ่มระดับแรงเบรกเครื่องยนต์ (Engine Brake+)", Category = "ELECTRONICS" },
                new() { SectionName = "ENGINE_BRAKE_DN", DisplayName = "Engine Brake Down", ThaiDescription = "ลดระดับแรงเบรกเครื่องยนต์ (Engine Brake-)", Category = "ELECTRONICS" },
                new() { SectionName = "__EXT_ENGINEMAP_UP", DisplayName = "Engine Map Cycle (CSP)", ThaiDescription = "เปลี่ยนแมปการทำงานเครื่องยนต์ (Engine Map / ECU Mode)", Category = "ELECTRONICS" },
                new() { SectionName = "__EXT_PIT_LIMITER", DisplayName = "Pit Limiter Toggle (CSP)", ThaiDescription = "เปิด/ปิด ลิมิตความเร็วพิทเลน (Pit Limiter)", Category = "ELECTRONICS" },
                new() { SectionName = "DRS", DisplayName = "DRS (Drag Reduction System)", ThaiDescription = "เปิดปีก DRS (ลดแรงต้านอากาศ)", Category = "ELECTRONICS" },
                new() { SectionName = "KERS", DisplayName = "KERS / P2P Boost", ThaiDescription = "ปุ่มเร่งพลังงานไฟฟ้าไฮบริด KERS / Push to Pass", Category = "ELECTRONICS" },
                new() { SectionName = "MGUK_DELIVERY_UP", DisplayName = "MGU-K Delivery Up", ThaiDescription = "เพิ่มระดับการจ่ายพลังงาน MGU-K", Category = "ELECTRONICS" },
                new() { SectionName = "MGUK_DELIVERY_DN", DisplayName = "MGU-K Delivery Down", ThaiDescription = "ลดระดับการจ่ายพลังงาน MGU-K", Category = "ELECTRONICS" },
                new() { SectionName = "MGUK_RECOVERY_UP", DisplayName = "MGU-K Recovery Up", ThaiDescription = "เพิ่มระดับการชาร์จไฟกลับ MGU-K", Category = "ELECTRONICS" },
                new() { SectionName = "MGUK_RECOVERY_DN", DisplayName = "MGU-K Recovery Down", ThaiDescription = "ลดระดับการชาร์จไฟกลับ MGU-K", Category = "ELECTRONICS" },
                new() { SectionName = "MGUH_MODE", DisplayName = "MGU-H Mode Toggle", ThaiDescription = "สลับโหมดการทำงาน MGU-H (Motor / Battery)", Category = "ELECTRONICS" },

                // === 5. CAMERA & SESSION (11 Actions) ===
                new() { SectionName = "ACTION_CHANGE_CAMERA", DisplayName = "Change Camera View", ThaiDescription = "เปลี่ยนมุมมองกล้อง (Cockpit / Bonnet / Chase)", Category = "CAMERA & SESSION" },
                new() { SectionName = "GLANCELEFT", DisplayName = "Glance Left", ThaiDescription = "หันมองซ้าย", Category = "CAMERA & SESSION" },
                new() { SectionName = "GLANCERIGHT", DisplayName = "Glance Right", ThaiDescription = "หันมองขวา", Category = "CAMERA & SESSION" },
                new() { SectionName = "GLANCEBACK", DisplayName = "Glance Back", ThaiDescription = "หันมองหลัง", Category = "CAMERA & SESSION" },
                new() { SectionName = "__CM_TO_PITS", DisplayName = "Teleport to Pitlane (Return to Pit)", ThaiDescription = "วาร์ปกลับพิท (Return to Pit)", Category = "CAMERA & SESSION" },
                new() { SectionName = "__CM_RESET_SESSION", DisplayName = "Restart Session", ThaiDescription = "เริ่มเซสชันใหม่ทันที (Restart)", Category = "CAMERA & SESSION" },
                new() { SectionName = "__CM_PAUSE", DisplayName = "Pause / Resume Session", ThaiDescription = "หยุดเกมชั่วคราว (Pause / Resume)", Category = "CAMERA & SESSION" },
                new() { SectionName = "ACTION_CELEBRATE", DisplayName = "Celebrate / Taunt Gesture", ThaiDescription = "แสดงท่าทางดีใจ / ยกมือฉลอง", Category = "CAMERA & SESSION" },
                new() { SectionName = "ACTION_CLAIM", DisplayName = "Claim / Protest Gesture", ThaiDescription = "แสดงท่าทางประท้วง / สื่อสารในสนาม", Category = "CAMERA & SESSION" },
                new() { SectionName = "__CM_ONLINE_POLL_YES", DisplayName = "Online Vote Poll: Yes", ThaiDescription = "โหวตออนไลน์: ใช่ (Vote YES)", Category = "CAMERA & SESSION" },
                new() { SectionName = "__CM_ONLINE_POLL_NO", DisplayName = "Online Vote Poll: No", ThaiDescription = "โหวตออนไลน์: ไม่ใช่ (Vote NO)", Category = "CAMERA & SESSION" },
            };
        }

        public ObservableCollection<ControlProfileInfo> LoadSavedProfiles()
        {
            var list = new List<ControlProfileInfo>();

            // 1. User saved profiles
            var userDir = GetSavedSetupsDir();
            if (Directory.Exists(userDir))
            {
                foreach (var file in Directory.GetFiles(userDir, "*.ini"))
                {
                    try
                    {
                        var ini = new IniModifier(file);
                        var method = ini.GetValue("HEADER", "INPUT_METHOD") ?? "WHEEL";
                        var con0 = ini.GetValue("CONTROLLERS", "CON0") ?? "";
                        list.Add(new ControlProfileInfo
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            FilePath = file,
                            InputMethodBadge = method.ToUpperInvariant(),
                            DeviceName = con0,
                            IsFactoryPreset = false,
                            LastModified = File.GetLastWriteTime(file)
                        });
                    }
                    catch { }
                }
            }

            // Order user profiles by modification date descending
            list.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

            // 2. Factory presets
            var presetsDir = GetPresetsDir();
            if (Directory.Exists(presetsDir))
            {
                var factoryList = new List<ControlProfileInfo>();
                foreach (var file in Directory.GetFiles(presetsDir, "*.ini"))
                {
                    try
                    {
                        var ini = new IniModifier(file);
                        var method = ini.GetValue("HEADER", "INPUT_METHOD") ?? "WHEEL";
                        var con0 = ini.GetValue("CONTROLLERS", "CON0") ?? "";
                        factoryList.Add(new ControlProfileInfo
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            FilePath = file,
                            InputMethodBadge = method.ToUpperInvariant(),
                            DeviceName = con0,
                            IsFactoryPreset = true,
                            LastModified = File.GetLastWriteTime(file)
                        });
                    }
                    catch { }
                }
                factoryList.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                list.AddRange(factoryList);
            }

            return new ObservableCollection<ControlProfileInfo>(list);
        }

        private static string CleanKeyString(string raw)
        {
            int semi = raw.IndexOf(';');
            if (semi >= 0) raw = raw.Substring(0, semi);
            return raw.Trim();
        }

        private static void PopulateBindingFromIni(IniModifier ini, ControlActionBinding action)
        {
            string sec = action.SectionName;
            if (sec.StartsWith("SHIFTER:GEAR_"))
            {
                var gearKey = sec.Substring("SHIFTER:".Length);
                var shiftJoy = ini.GetValue("SHIFTER", "JOY");
                if (int.TryParse(shiftJoy, out int sj)) action.JoyIndex = sj;

                var shiftBtn = ini.GetValue("SHIFTER", gearKey);
                if (int.TryParse(shiftBtn, out int sb)) action.ButtonIndex = sb;

                // Fallback to specific gear section [GEAR_X]
                var directJoy = ini.GetValue(gearKey, "JOY");
                if (int.TryParse(directJoy, out int dj) && dj >= 0) action.JoyIndex = dj;

                var directBtn = ini.GetValue(gearKey, "BUTTON");
                if (int.TryParse(directBtn, out int db) && db >= 0) action.ButtonIndex = db;

                var directKey = ini.GetValue(gearKey, "KEY");
                if (!string.IsNullOrEmpty(directKey)) action.KeyHex = CleanKeyString(directKey);

                var directXbox = ini.GetValue(gearKey, "XBOXBUTTON");
                if (!string.IsNullOrEmpty(directXbox)) action.XboxButton = directXbox;
            }
            else
            {
                var joyStr = ini.GetValue(sec, "JOY");
                if (int.TryParse(joyStr, out int j)) action.JoyIndex = j;

                var btnStr = ini.GetValue(sec, "BUTTON");
                if (int.TryParse(btnStr, out int b)) action.ButtonIndex = b;

                var keyStr = ini.GetValue(sec, "KEY");
                if (!string.IsNullOrEmpty(keyStr)) action.KeyHex = CleanKeyString(keyStr);

                var xboxStr = ini.GetValue(sec, "XBOXBUTTON");
                if (!string.IsNullOrEmpty(xboxStr)) action.XboxButton = xboxStr;
            }
        }

        private static void SaveBindingToIni(IniModifier ini, ControlActionBinding action)
        {
            string sec = action.SectionName;
            if (sec.StartsWith("SHIFTER:GEAR_"))
            {
                var gearKey = sec.Substring("SHIFTER:".Length);
                ini.SetValue("SHIFTER", gearKey, action.ButtonIndex.ToString());
                if (action.JoyIndex >= 0) ini.SetValue("SHIFTER", "JOY", action.JoyIndex.ToString());

                ini.SetValue(gearKey, "BUTTON", action.ButtonIndex.ToString());
                ini.SetValue(gearKey, "JOY", action.JoyIndex.ToString());
                if (!string.IsNullOrEmpty(action.KeyHex)) ini.SetValue(gearKey, "KEY", action.KeyHex);
                if (!string.IsNullOrEmpty(action.XboxButton)) ini.SetValue(gearKey, "XBOXBUTTON", action.XboxButton);
            }
            else
            {
                ini.SetValue(sec, "BUTTON", action.ButtonIndex.ToString());
                ini.SetValue(sec, "JOY", action.JoyIndex.ToString());
                if (!string.IsNullOrEmpty(action.KeyHex)) ini.SetValue(sec, "KEY", action.KeyHex);
                if (!string.IsNullOrEmpty(action.XboxButton)) ini.SetValue(sec, "XBOXBUTTON", action.XboxButton);
            }
        }

        private static void PopulateSettingsFromIni(IniModifier ini, FfbSettings settings)
        {
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

            // Advanced Options
            var combineVal = ini.GetValue("ADVANCED", "COMBINE_WITH_KEYBOARD_CONTROL");
            settings.CombineWithKeyboard = combineVal != "0";

            // Action Bindings Catalog
            var ffbService = new FfbService();
            var catalog = ffbService.GetDefaultActionCatalog();
            foreach (var action in catalog)
            {
                PopulateBindingFromIni(ini, action);
            }
            settings.ActionBindings = new ObservableCollection<ControlActionBinding>(catalog);

            // Sync Quick Button Displays
            SyncQuickSummary(settings);
        }

        private static void SyncQuickSummary(FfbSettings settings)
        {
            foreach (var b in settings.ActionBindings)
            {
                if (b.SectionName == "GEARUP" && b.ButtonIndex >= 0) settings.ButtonGearUp = $"B{b.ButtonIndex} (Right Paddle)";
                else if (b.SectionName == "GEARDN" && b.ButtonIndex >= 0) settings.ButtonGearDown = $"B{b.ButtonIndex} (Left Paddle)";
                else if (b.SectionName == "ACTION_HEADLIGHTS" && b.ButtonIndex >= 0) settings.ButtonHeadlights = $"B{b.ButtonIndex}";
                else if (b.SectionName == "ACTION_HEADLIGHTS_FLASH" && b.ButtonIndex >= 0) settings.ButtonHeadlightsFlash = $"B{b.ButtonIndex}";
                else if (b.SectionName == "ACTION_HORN" && b.ButtonIndex >= 0) settings.ButtonHorn = $"B{b.ButtonIndex}";
                else if (b.SectionName == "ACTION_CHANGE_CAMERA" && b.ButtonIndex >= 0) settings.ButtonChangeCamera = $"B{b.ButtonIndex}";
                else if (b.SectionName == "TCUP" && b.ButtonIndex >= 0) settings.ButtonTcUp = $"B{b.ButtonIndex}";
                else if (b.SectionName == "TCDN" && b.ButtonIndex >= 0) settings.ButtonTcDown = $"B{b.ButtonIndex}";
                else if (b.SectionName == "ABSUP" && b.ButtonIndex >= 0) settings.ButtonAbsUp = $"B{b.ButtonIndex}";
                else if (b.SectionName == "ABSDN" && b.ButtonIndex >= 0) settings.ButtonAbsDown = $"B{b.ButtonIndex}";
                else if (b.SectionName == "MGUK_DELIVERY_UP" && b.ButtonIndex >= 0) settings.ButtonMgukUp = $"B{b.ButtonIndex}";
                else if (b.SectionName == "MGUK_DELIVERY_DN" && b.ButtonIndex >= 0) settings.ButtonMgukDown = $"B{b.ButtonIndex}";
            }
        }

        private static void WriteSettingsToIni(IniModifier ini, FfbSettings settings)
        {
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

            // Advanced Options
            ini.SetValue("ADVANCED", "COMBINE_WITH_KEYBOARD_CONTROL", settings.CombineWithKeyboard ? "1" : "0");

            // Action Bindings
            foreach (var action in settings.ActionBindings)
            {
                SaveBindingToIni(ini, action);
            }
        }

        public async Task<FfbSettings> LoadSettingsAsync()
        {
            return await Task.Run(() =>
            {
                var settings = new FfbSettings();
                var path = GetControlsIniPath();
                if (!File.Exists(path)) return settings;

                var ini = new IniModifier(path);
                PopulateSettingsFromIni(ini, settings);
                settings.SavedProfiles = LoadSavedProfiles();
                settings.ActiveProfileName = "Active (controls.ini)";

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
                WriteSettingsToIni(ini, settings);
                SyncQuickSummary(settings);
                ini.Save();
            });
        }

        public async Task<FfbSettings> LoadProfileAsync(string profileIniPath)
        {
            return await Task.Run(() =>
            {
                var settings = new FfbSettings();
                if (!File.Exists(profileIniPath)) return settings;

                var controlsPath = GetControlsIniPath();
                try
                {
                    var dir = Path.GetDirectoryName(controlsPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.Copy(profileIniPath, controlsPath, overwrite: true);
                }
                catch { }

                var ini = new IniModifier(profileIniPath);
                PopulateSettingsFromIni(ini, settings);
                settings.ActiveProfileName = Path.GetFileNameWithoutExtension(profileIniPath);
                settings.SavedProfiles = LoadSavedProfiles();

                return settings;
            });
        }

        public async Task<ControlProfileInfo?> SaveProfileAsync(FfbSettings settings, string profileName)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(profileName)) return null;

                var sanitizedName = string.Join("_", profileName.Split(Path.GetInvalidFileNameChars())).Trim();
                if (string.IsNullOrWhiteSpace(sanitizedName)) return null;

                var targetPath = Path.Combine(GetSavedSetupsDir(), $"{sanitizedName}.ini");

                // Save active controls.ini first, then seed/update the profile .ini
                var controlsPath = GetControlsIniPath();
                if (File.Exists(controlsPath))
                {
                    var activeIni = new IniModifier(controlsPath);
                    WriteSettingsToIni(activeIni, settings);
                    activeIni.Save();

                    if (!File.Exists(targetPath))
                    {
                        try { File.Copy(controlsPath, targetPath, overwrite: false); } catch { }
                    }
                }

                var ini = new IniModifier(targetPath);
                WriteSettingsToIni(ini, settings);
                ini.Save();

                settings.ActiveProfileName = sanitizedName;
                settings.SavedProfiles = LoadSavedProfiles();
                return settings.SavedProfiles.FirstOrDefault(p => string.Equals(p.FilePath, targetPath, StringComparison.OrdinalIgnoreCase))
                    ?? new ControlProfileInfo
                    {
                        Name = sanitizedName,
                        FilePath = targetPath,
                        InputMethodBadge = settings.InputMethod,
                        DeviceName = settings.ConnectedDeviceName,
                        IsFactoryPreset = false,
                        LastModified = DateTime.Now
                    };
            });
        }

        public bool DeleteProfile(string profileFilePath)
        {
            if (string.IsNullOrWhiteSpace(profileFilePath) || !File.Exists(profileFilePath))
                return false;

            try
            {
                var presetsDir = GetPresetsDir();
                // Never delete factory presets
                if (profileFilePath.StartsWith(presetsDir, StringComparison.OrdinalIgnoreCase))
                    return false;

                File.Delete(profileFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
