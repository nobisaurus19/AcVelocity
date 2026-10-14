using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcVelocity.Models
{
    public class CarSkinInfo
    {
        public string Name { get; set; } = string.Empty;
        public string PreviewImagePath { get; set; } = string.Empty;
        public string LiveryImagePath { get; set; } = string.Empty;
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Default" : Name.Replace('_', ' ').ToUpperInvariant();
    }

    public class CarInfo : INotifyPropertyChanged
    {
        private string _selectedSkin = string.Empty;
        private string? _previewImagePath;

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Class { get; set; } = "STREET";
        public string Author { get; set; } = string.Empty;
        public bool IsMod { get; set; }
        public string OriginBadge => IsMod ? "MOD" : "KUNOS";

        public string Bhp { get; set; } = "--";
        public string Torque { get; set; } = "--";
        public string Weight { get; set; } = "--";
        public string TopSpeed { get; set; } = "--";
        public string Acceleration { get; set; } = "--";
        public string PowerToWeight { get; set; } = "--";
        public string Description { get; set; } = string.Empty;
        public string? BadgeImagePath { get; set; }
        
        public List<string> Tags { get; set; } = new();
        public List<string> Skins { get; set; } = new();
        public ObservableCollection<CarSkinInfo> SkinList { get; set; } = new();

        public string? PreviewImagePath
        {
            get => _previewImagePath;
            set { SetField(ref _previewImagePath, value); }
        }

        public string SelectedSkin
        {
            get => _selectedSkin;
            set { SetField(ref _selectedSkin, value); }
        }

        public string DisplayTitle => string.IsNullOrWhiteSpace(Name) ? Id : Name;
        public string DisplayBrand => string.IsNullOrWhiteSpace(Brand) ? "UNKNOWN" : Brand.ToUpperInvariant();
        public string DisplayClass => string.IsNullOrWhiteSpace(Class) ? "RACE" : Class.ToUpperInvariant();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
