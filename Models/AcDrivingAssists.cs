using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcVelocity.Models
{
    public class AcDrivingAssists : INotifyPropertyChanged
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

        // ABS: 0=Off, 1=Factory, 2=Forced On
        private int _absMode = 1;
        public int AbsMode
        {
            get => _absMode;
            set
            {
                if (SetField(ref _absMode, value))
                {
                    OnPropertyChanged(nameof(DisplayAbsMode));
                    OnPropertyChanged(nameof(IsAbsOff));
                    OnPropertyChanged(nameof(IsAbsFactory));
                    OnPropertyChanged(nameof(IsAbsOn));
                }
            }
        }

        public bool IsAbsOff { get => AbsMode == 0; set { if (value) AbsMode = 0; } }
        public bool IsAbsFactory { get => AbsMode == 1; set { if (value) AbsMode = 1; } }
        public bool IsAbsOn { get => AbsMode == 2; set { if (value) AbsMode = 2; } }

        public string DisplayAbsMode => AbsMode switch
        {
            0 => "DISABLED (OFF)",
            1 => "FACTORY (OEM SPEC)",
            2 => "ALWAYS ON",
            _ => "FACTORY"
        };

        // Traction Control: 0=Off, 1=Factory, 2=Forced On
        private int _tcMode = 1;
        public int TcMode
        {
            get => _tcMode;
            set
            {
                if (SetField(ref _tcMode, value))
                {
                    OnPropertyChanged(nameof(DisplayTcMode));
                    OnPropertyChanged(nameof(IsTcOff));
                    OnPropertyChanged(nameof(IsTcFactory));
                    OnPropertyChanged(nameof(IsTcOn));
                }
            }
        }

        public bool IsTcOff { get => TcMode == 0; set { if (value) TcMode = 0; } }
        public bool IsTcFactory { get => TcMode == 1; set { if (value) TcMode = 1; } }
        public bool IsTcOn { get => TcMode == 2; set { if (value) TcMode = 2; } }

        public string DisplayTcMode => TcMode switch
        {
            0 => "DISABLED (OFF)",
            1 => "FACTORY (OEM SPEC)",
            2 => "ALWAYS ON",
            _ => "FACTORY"
        };

        // Stability Control (0 - 100%)
        private int _stabilityControl = 0;
        public int StabilityControl
        {
            get => _stabilityControl;
            set
            {
                if (SetField(ref _stabilityControl, value))
                    OnPropertyChanged(nameof(DisplayStabilityControl));
            }
        }
        public string DisplayStabilityControl => $"{StabilityControl}%";

        // Toggles
        private bool _autoClutch = true;
        public bool AutoClutch
        {
            get => _autoClutch;
            set => SetField(ref _autoClutch, value);
        }

        private bool _autoBlip = true;
        public bool AutoBlip
        {
            get => _autoBlip;
            set => SetField(ref _autoBlip, value);
        }

        private bool _autoShifter = false;
        public bool AutoShifter
        {
            get => _autoShifter;
            set => SetField(ref _autoShifter, value);
        }

        private bool _autoBrake = false;
        public bool AutoBrake
        {
            get => _autoBrake;
            set => SetField(ref _autoBrake, value);
        }

        private bool _idealLine = false;
        public bool IdealLine
        {
            get => _idealLine;
            set => SetField(ref _idealLine, value);
        }

        private bool _tyreBlankets = true;
        public bool TyreBlankets
        {
            get => _tyreBlankets;
            set => SetField(ref _tyreBlankets, value);
        }

        // Damage & Wear
        private int _damage = 0; // 0 = Off, 100 = 100%
        public int Damage
        {
            get => _damage;
            set
            {
                if (SetField(ref _damage, value))
                    OnPropertyChanged(nameof(DisplayDamage));
            }
        }
        public string DisplayDamage => Damage == 0 ? "OFF (0%)" : $"{Damage}%";

        private int _tyreWear = 0; // 0 = Off, 100 = 1x
        public int TyreWear
        {
            get => _tyreWear;
            set
            {
                if (SetField(ref _tyreWear, value))
                    OnPropertyChanged(nameof(DisplayTyreWear));
            }
        }
        public string DisplayTyreWear => TyreWear == 0 ? "OFF (0x)" : $"{TyreWear}%";

        private int _fuelRate = 1; // 0 = Off, 1 = 1x
        public int FuelRate
        {
            get => _fuelRate;
            set
            {
                if (SetField(ref _fuelRate, value))
                    OnPropertyChanged(nameof(DisplayFuelRate));
            }
        }
        public string DisplayFuelRate => FuelRate == 0 ? "OFF (0x)" : $"{FuelRate}x";

        private int _slipstreamMulti = 1;
        public int SlipstreamMulti
        {
            get => _slipstreamMulti;
            set
            {
                if (SetField(ref _slipstreamMulti, value))
                    OnPropertyChanged(nameof(DisplaySlipstreamMulti));
            }
        }
        public string DisplaySlipstreamMulti => $"{SlipstreamMulti}x";
    }
}
