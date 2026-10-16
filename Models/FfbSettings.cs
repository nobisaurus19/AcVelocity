using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace AcVelocity.Models
{
    public class ControlProfileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string InputMethodBadge { get; set; } = "WHEEL";
        public string DeviceName { get; set; } = string.Empty;
        public bool IsFactoryPreset { get; set; }
        public DateTime LastModified { get; set; }
        public string DisplayTitle => IsFactoryPreset
            ? $"[PRESET] {Name}"
            : (string.IsNullOrWhiteSpace(DeviceName) ? $"★ {Name} [{InputMethodBadge}]" : $"★ {Name} [{InputMethodBadge} — {DeviceName}]");
    }

    public class ControlActionBinding : INotifyPropertyChanged
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

        private string _sectionName = string.Empty;
        public string SectionName
        {
            get => _sectionName;
            set => SetField(ref _sectionName, value);
        }

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => SetField(ref _displayName, value);
        }

        private string _thaiDescription = string.Empty;
        public string ThaiDescription
        {
            get => _thaiDescription;
            set => SetField(ref _thaiDescription, value);
        }

        private string _category = "SHIFTER & GEARS";
        public string Category
        {
            get => _category;
            set => SetField(ref _category, value);
        }

        private int _joyIndex = 0;
        public int JoyIndex
        {
            get => _joyIndex;
            set
            {
                if (SetField(ref _joyIndex, value))
                {
                    OnPropertyChanged(nameof(WheelBindingDisplay));
                }
            }
        }

        private int _buttonIndex = -1;
        public int ButtonIndex
        {
            get => _buttonIndex;
            set
            {
                if (SetField(ref _buttonIndex, value))
                {
                    OnPropertyChanged(nameof(WheelBindingDisplay));
                    OnPropertyChanged(nameof(HasWheelBinding));
                }
            }
        }

        private string _keyHex = "-1";
        public string KeyHex
        {
            get => _keyHex;
            set
            {
                if (SetField(ref _keyHex, value))
                {
                    OnPropertyChanged(nameof(KeyBindingDisplay));
                    OnPropertyChanged(nameof(HasKeyBinding));
                }
            }
        }

        private string _xboxButton = string.Empty;
        public string XboxButton
        {
            get => _xboxButton;
            set
            {
                if (SetField(ref _xboxButton, value))
                {
                    OnPropertyChanged(nameof(XboxBindingDisplay));
                    OnPropertyChanged(nameof(HasXboxBinding));
                }
            }
        }

        private bool _isListening = false;
        public bool IsListening
        {
            get => _isListening;
            set
            {
                if (SetField(ref _isListening, value))
                {
                    OnPropertyChanged(nameof(CaptureButtonText));
                }
            }
        }

        // Computed properties
        public bool HasWheelBinding => ButtonIndex >= 0;
        public bool HasKeyBinding => !string.IsNullOrEmpty(KeyHex) && KeyHex != "-1" && KeyHex != "0" && KeyHex != "0x0";
        public bool HasXboxBinding => !string.IsNullOrEmpty(XboxButton) && XboxButton != "-1";

        public string WheelBindingDisplay => ButtonIndex >= 0 ? $"พวงมาลัย ปุ่ม #{ButtonIndex + 1} (B{ButtonIndex})" : "— ยังไม่ตั้งค่า —";
        public string KeyBindingDisplay => FormatVirtualKeyName(KeyHex);
        public string XboxBindingDisplay => string.IsNullOrEmpty(XboxButton) || XboxButton == "-1" ? "— ไม่มี —" : XboxButton;
        public string CaptureButtonText => IsListening ? "🔴 กดปุ่มเลย... (ESC ยกเลิก)" : "🎯 ตั้งปุ่ม (Bind)";

        public static string FormatVirtualKeyName(string? keyHex)
        {
            if (string.IsNullOrWhiteSpace(keyHex) || keyHex == "-1" || keyHex == "0" || keyHex == "0x0" || keyHex == "0x00")
                return "— ไม่มี —";

            int commentIdx = keyHex.IndexOf(';');
            if (commentIdx >= 0) keyHex = keyHex.Substring(0, commentIdx).Trim();

            int vk = -1;
            if (keyHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(keyHex.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out vk))
                    return keyHex.ToUpperInvariant();
            }
            else if (!int.TryParse(keyHex, NumberStyles.Integer, CultureInfo.InvariantCulture, out vk))
            {
                if (!int.TryParse(keyHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out vk))
                    return keyHex.ToUpperInvariant();
            }

            if (vk <= 0) return "— ไม่มี —";

            return vk switch
            {
                0x08 => "BACKSPACE",
                0x09 => "TAB",
                0x0C => "CLEAR",
                0x0D => "ENTER",
                0x10 => "SHIFT",
                0x11 => "CTRL",
                0x12 => "ALT",
                0x13 => "PAUSE",
                0x14 => "CAPS LOCK",
                0x1B => "ESC",
                0x20 => "SPACE",
                0x21 => "PAGE UP",
                0x22 => "PAGE DOWN",
                0x23 => "END",
                0x24 => "HOME",
                0x25 => "LEFT ARROW",
                0x26 => "UP ARROW",
                0x27 => "RIGHT ARROW",
                0x28 => "DOWN ARROW",
                0x2C => "PRINT SCREEN",
                0x2D => "INSERT",
                0x2E => "DELETE",
                >= 0x30 and <= 0x39 => $"KEY {(char)vk}",
                >= 0x41 and <= 0x5A => $"KEY {(char)vk}",
                0x5B => "LEFT WIN",
                0x5C => "RIGHT WIN",
                >= 0x60 and <= 0x69 => $"NUMPAD {vk - 0x60}",
                0x6A => "NUMPAD *",
                0x6B => "NUMPAD +",
                0x6C => "NUMPAD ENTER",
                0x6D => "NUMPAD -",
                0x6E => "NUMPAD .",
                0x6F => "NUMPAD /",
                >= 0x70 and <= 0x7B => $"F{vk - 0x70 + 1}",
                0xA0 => "LEFT SHIFT",
                0xA1 => "RIGHT SHIFT",
                0xA2 => "LEFT CTRL",
                0xA3 => "RIGHT CTRL",
                0xA4 => "LEFT ALT",
                0xA5 => "RIGHT ALT",
                0xBA => "; :",
                0xBB => "= +",
                0xBC => ", <",
                0xBD => "- _",
                0xBE => ". >",
                0xBF => "/ ?",
                0xC0 => "` ~",
                0xDB => "[ {",
                0xDC => "\\ |",
                0xDD => "] }",
                0xDE => "' \"",
                _ => $"0x{vk:X2}"
            };
        }
    }

    public class FfbSettings : INotifyPropertyChanged
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

        // === Input Device & Method ===
        private string _inputMethod = "WHEEL"; // WHEEL, X360, KEYBOARD
        public string InputMethod
        {
            get => _inputMethod;
            set
            {
                if (SetField(ref _inputMethod, value))
                {
                    OnPropertyChanged(nameof(IsWheelMode));
                    OnPropertyChanged(nameof(IsGamepadMode));
                    OnPropertyChanged(nameof(IsKeyboardMode));
                    OnPropertyChanged(nameof(DeviceStatusDisplay));
                }
            }
        }

        private string _connectedDeviceName = "R3 Racing Wheel and Pedals";
        public string ConnectedDeviceName
        {
            get => _connectedDeviceName;
            set
            {
                if (SetField(ref _connectedDeviceName, value))
                {
                    OnPropertyChanged(nameof(DeviceStatusDisplay));
                }
            }
        }

        public bool IsWheelMode
        {
            get => InputMethod == "WHEEL";
            set { if (value) InputMethod = "WHEEL"; }
        }

        public bool IsGamepadMode
        {
            get => InputMethod == "X360";
            set { if (value) InputMethod = "X360"; }
        }

        public bool IsKeyboardMode
        {
            get => InputMethod == "KEYBOARD";
            set { if (value) InputMethod = "KEYBOARD"; }
        }

        public string DeviceStatusDisplay => InputMethod switch
        {
            "WHEEL" => $"🟢 CONNECTED: {ConnectedDeviceName}",
            "X360" => "🟢 XINPUT GAMEPAD DETECTED",
            "KEYBOARD" => "🟢 KEYBOARD & MOUSE ARMED",
            _ => $"🟢 {InputMethod}"
        };

        // === FFB Wheel Base Motor ===
        private double _masterGain = 1.0;
        public double MasterGain
        {
            get => _masterGain;
            set
            {
                if (SetField(ref _masterGain, value))
                    OnPropertyChanged(nameof(DisplayMasterGain));
            }
        }

        private double _filter = 0.0;
        public double Filter
        {
            get => _filter;
            set => SetField(ref _filter, value);
        }

        private double _minForce = 0.0;
        public double MinForce
        {
            get => _minForce;
            set => SetField(ref _minForce, value);
        }

        private double _kerbsEffect = 0.02;
        public double KerbsEffect
        {
            get => _kerbsEffect;
            set => SetField(ref _kerbsEffect, value);
        }

        private double _roadEffect = 0.0;
        public double RoadEffect
        {
            get => _roadEffect;
            set => SetField(ref _roadEffect, value);
        }

        private double _slipEffect = 0.0;
        public double SlipEffect
        {
            get => _slipEffect;
            set => SetField(ref _slipEffect, value);
        }

        private double _absEffect = 0.0;
        public double AbsEffect
        {
            get => _absEffect;
            set => SetField(ref _absEffect, value);
        }

        private bool _enhancedUndersteer = false;
        public bool EnhancedUndersteer
        {
            get => _enhancedUndersteer;
            set => SetField(ref _enhancedUndersteer, value);
        }

        private int _steeringLock = 900;
        public int SteeringLock
        {
            get => _steeringLock;
            set
            {
                if (SetField(ref _steeringLock, value))
                    OnPropertyChanged(nameof(DisplayLock));
            }
        }

        // === Pedals & Axis Calibration ===
        private double _throttleGamma = 1.0;
        public double ThrottleGamma
        {
            get => _throttleGamma;
            set
            {
                if (SetField(ref _throttleGamma, value))
                    OnPropertyChanged(nameof(DisplayThrottleGamma));
            }
        }

        private double _brakesGamma = 2.4;
        public double BrakesGamma
        {
            get => _brakesGamma;
            set
            {
                if (SetField(ref _brakesGamma, value))
                    OnPropertyChanged(nameof(DisplayBrakesGamma));
            }
        }

        private double _clutchGamma = 1.0;
        public double ClutchGamma
        {
            get => _clutchGamma;
            set
            {
                if (SetField(ref _clutchGamma, value))
                    OnPropertyChanged(nameof(DisplayClutchGamma));
            }
        }

        private double _handbrakeGamma = 1.0;
        public double HandbrakeGamma
        {
            get => _handbrakeGamma;
            set
            {
                if (SetField(ref _handbrakeGamma, value))
                    OnPropertyChanged(nameof(DisplayHandbrakeGamma));
            }
        }

        private bool _hShifterActive = false; // true = H-Shifter, false = Paddles
        public bool HShifterActive
        {
            get => _hShifterActive;
            set
            {
                if (SetField(ref _hShifterActive, value))
                {
                    OnPropertyChanged(nameof(ShifterModeDisplay));
                    OnPropertyChanged(nameof(IsPaddleMode));
                }
            }
        }

        public bool IsPaddleMode
        {
            get => !HShifterActive;
            set { HShifterActive = !value; }
        }
        public string ShifterModeDisplay => HShifterActive ? "H-PATTERN SHIFTER (6/7-SPEED GATE)" : "PADDLE SHIFTERS (SEQUENTIAL)";

        // === Gamepad (X360) Settings ===
        private double _gamepadSpeedSensitivity = 0.5;
        public double GamepadSpeedSensitivity
        {
            get => _gamepadSpeedSensitivity;
            set => SetField(ref _gamepadSpeedSensitivity, value);
        }

        private double _gamepadSteerSpeed = 0.2;
        public double GamepadSteerSpeed
        {
            get => _gamepadSteerSpeed;
            set => SetField(ref _gamepadSteerSpeed, value);
        }

        private double _gamepadSteerDeadzone = 0.05;
        public double GamepadSteerDeadzone
        {
            get => _gamepadSteerDeadzone;
            set => SetField(ref _gamepadSteerDeadzone, value);
        }

        private double _gamepadSteerFilter = 0.6;
        public double GamepadSteerFilter
        {
            get => _gamepadSteerFilter;
            set => SetField(ref _gamepadSteerFilter, value);
        }

        private double _gamepadSteerGamma = 3.0;
        public double GamepadSteerGamma
        {
            get => _gamepadSteerGamma;
            set => SetField(ref _gamepadSteerGamma, value);
        }

        private double _gamepadRumbleIntensity = 1.0;
        public double GamepadRumbleIntensity
        {
            get => _gamepadRumbleIntensity;
            set => SetField(ref _gamepadRumbleIntensity, value);
        }

        // === Quick Button Mapping Display ===
        private string _buttonGearUp = "B13 (Right Paddle)";
        public string ButtonGearUp
        {
            get => _buttonGearUp;
            set => SetField(ref _buttonGearUp, value);
        }

        private string _buttonGearDown = "B12 (Left Paddle)";
        public string ButtonGearDown
        {
            get => _buttonGearDown;
            set => SetField(ref _buttonGearDown, value);
        }

        private string _buttonHeadlights = "B8";
        public string ButtonHeadlights
        {
            get => _buttonHeadlights;
            set => SetField(ref _buttonHeadlights, value);
        }

        private string _buttonHeadlightsFlash = "B34";
        public string ButtonHeadlightsFlash
        {
            get => _buttonHeadlightsFlash;
            set => SetField(ref _buttonHeadlightsFlash, value);
        }

        private string _buttonHorn = "B18";
        public string ButtonHorn
        {
            get => _buttonHorn;
            set => SetField(ref _buttonHorn, value);
        }

        private string _buttonChangeCamera = "B22";
        public string ButtonChangeCamera
        {
            get => _buttonChangeCamera;
            set => SetField(ref _buttonChangeCamera, value);
        }

        private string _buttonTcUp = "B26";
        public string ButtonTcUp
        {
            get => _buttonTcUp;
            set => SetField(ref _buttonTcUp, value);
        }

        private string _buttonTcDown = "B25";
        public string ButtonTcDown
        {
            get => _buttonTcDown;
            set => SetField(ref _buttonTcDown, value);
        }

        private string _buttonAbsUp = "B30";
        public string ButtonAbsUp
        {
            get => _buttonAbsUp;
            set => SetField(ref _buttonAbsUp, value);
        }

        private string _buttonAbsDown = "B29";
        public string ButtonAbsDown
        {
            get => _buttonAbsDown;
            set => SetField(ref _buttonAbsDown, value);
        }

        private string _buttonMgukUp = "B47";
        public string ButtonMgukUp
        {
            get => _buttonMgukUp;
            set => SetField(ref _buttonMgukUp, value);
        }

        private string _buttonMgukDown = "B46";
        public string ButtonMgukDown
        {
            get => _buttonMgukDown;
            set => SetField(ref _buttonMgukDown, value);
        }

        // === Advanced Control Options & Studio Collections ===
        private bool _combineWithKeyboard = true;
        public bool CombineWithKeyboard
        {
            get => _combineWithKeyboard;
            set => SetField(ref _combineWithKeyboard, value);
        }

        private string _activeProfileName = "Active (controls.ini)";
        public string ActiveProfileName
        {
            get => _activeProfileName;
            set => SetField(ref _activeProfileName, value);
        }

        private ObservableCollection<ControlActionBinding> _actionBindings = new();
        public ObservableCollection<ControlActionBinding> ActionBindings
        {
            get => _actionBindings;
            set => SetField(ref _actionBindings, value);
        }

        private ObservableCollection<ControlProfileInfo> _savedProfiles = new();
        public ObservableCollection<ControlProfileInfo> SavedProfiles
        {
            get => _savedProfiles;
            set => SetField(ref _savedProfiles, value);
        }

        public string DisplayMasterGain => $"{MasterGain * 100:0}%";
        public string DisplayLock => $"{SteeringLock}°";
        public string DisplayBrakesGamma => $"{BrakesGamma:F1}";
        public string DisplayThrottleGamma => $"{ThrottleGamma:F1}";
        public string DisplayClutchGamma => $"{ClutchGamma:F1}";
        public string DisplayHandbrakeGamma => $"{HandbrakeGamma:F1}";
    }
}
