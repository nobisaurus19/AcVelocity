using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcVelocity.Models
{
    public class AcAudioSettings : INotifyPropertyChanged
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

        private double _masterVolume = 1.0;
        public double MasterVolume
        {
            get => _masterVolume;
            set
            {
                if (SetField(ref _masterVolume, value))
                    OnPropertyChanged(nameof(DisplayMasterVolume));
            }
        }

        private double _engineVolume = 0.5;
        public double EngineVolume
        {
            get => _engineVolume;
            set
            {
                if (SetField(ref _engineVolume, value))
                    OnPropertyChanged(nameof(DisplayEngineVolume));
            }
        }

        private double _opponentsVolume = 0.5;
        public double OpponentsVolume
        {
            get => _opponentsVolume;
            set
            {
                if (SetField(ref _opponentsVolume, value))
                    OnPropertyChanged(nameof(DisplayOpponentsVolume));
            }
        }

        private double _tyresVolume = 0.5;
        public double TyresVolume
        {
            get => _tyresVolume;
            set
            {
                if (SetField(ref _tyresVolume, value))
                    OnPropertyChanged(nameof(DisplayTyresVolume));
            }
        }

        private double _windVolume = 0.3;
        public double WindVolume
        {
            get => _windVolume;
            set
            {
                if (SetField(ref _windVolume, value))
                    OnPropertyChanged(nameof(DisplayWindVolume));
            }
        }

        private double _brakesVolume = 0.8;
        public double BrakesVolume
        {
            get => _brakesVolume;
            set
            {
                if (SetField(ref _brakesVolume, value))
                    OnPropertyChanged(nameof(DisplayBrakesVolume));
            }
        }

        private double _surfacesVolume = 0.4;
        public double SurfacesVolume
        {
            get => _surfacesVolume;
            set
            {
                if (SetField(ref _surfacesVolume, value))
                    OnPropertyChanged(nameof(DisplaySurfacesVolume));
            }
        }

        private double _musicVolume = 0.5;
        public double MusicVolume
        {
            get => _musicVolume;
            set
            {
                if (SetField(ref _musicVolume, value))
                    OnPropertyChanged(nameof(DisplayMusicVolume));
            }
        }

        private double _uiSoundsVolume = 0.7;
        public double UiSoundsVolume
        {
            get => _uiSoundsVolume;
            set
            {
                if (SetField(ref _uiSoundsVolume, value))
                    OnPropertyChanged(nameof(DisplayUiSoundsVolume));
            }
        }

        private double _weatherVolume = 0.5;
        public double WeatherVolume
        {
            get => _weatherVolume;
            set
            {
                if (SetField(ref _weatherVolume, value))
                    OnPropertyChanged(nameof(DisplayWeatherVolume));
            }
        }

        public string DisplayMasterVolume => $"{MasterVolume * 100:0}%";
        public string DisplayEngineVolume => $"{EngineVolume * 100:0}%";
        public string DisplayOpponentsVolume => $"{OpponentsVolume * 100:0}%";
        public string DisplayTyresVolume => $"{TyresVolume * 100:0}%";
        public string DisplayWindVolume => $"{WindVolume * 100:0}%";
        public string DisplayBrakesVolume => $"{BrakesVolume * 100:0}%";
        public string DisplaySurfacesVolume => $"{SurfacesVolume * 100:0}%";
        public string DisplayMusicVolume => $"{MusicVolume * 100:0}%";
        public string DisplayUiSoundsVolume => $"{UiSoundsVolume * 100:0}%";
        public string DisplayWeatherVolume => $"{WeatherVolume * 100:0}%";
    }
}
