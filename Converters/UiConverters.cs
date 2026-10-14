using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AcVelocity.Converters
{
    public class StringToImageSourceConverter : IValueConverter
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> ImageCache = new(StringComparer.OrdinalIgnoreCase);

        public static BitmapImage? GetCachedImage(string path, int targetWidth = 300)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            if (ImageCache.TryGetValue(path, out var cached))
            {
                return cached;
            }

            if (!File.Exists(path)) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.DecodePixelWidth = targetWidth > 0 ? targetWidth : 300;
                bitmap.EndInit();
                bitmap.Freeze(); // Freezes bitmap so it can be safely accessed across any thread

                ImageCache[path] = bitmap;
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                int width = parameter != null && int.TryParse(parameter.ToString(), out int w) ? w : 260;
                return GetCachedImage(path, width);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNull = value == null || (value is string s && string.IsNullOrWhiteSpace(s));
            if (Invert) isNull = !isNull;
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringEqualsToBrushConverter : IValueConverter
    {
        public Brush MatchBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0xFF, 0x1E, 0x40));
        public Brush DefaultBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value.ToString() == parameter.ToString())
            {
                return MatchBrush;
            }
            return DefaultBrush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
