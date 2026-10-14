using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcVelocity.Models
{
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

        public string DisplayMasterGain => $"{MasterGain * 100:0}%";
        public string DisplayLock => $"{SteeringLock}°";
        public string DisplayBrakesGamma => $"{BrakesGamma:F1}";
        public string DisplayThrottleGamma => $"{ThrottleGamma:F1}";
        public string DisplayClutchGamma => $"{ClutchGamma:F1}";
        public string DisplayHandbrakeGamma => $"{HandbrakeGamma:F1}";
    }
}
