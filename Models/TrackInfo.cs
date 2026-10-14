using System;

namespace AcVelocity.Models
{
    public class TrackInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Length { get; set; } = "--";
        public string Width { get; set; } = "--";
        public string Pitboxes { get; set; } = "--";
        public string Run { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PreviewImagePath { get; set; }
        public string? OutlineImagePath { get; set; }
        public string SubConfig { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool IsMod { get; set; }
        public string OriginBadge => IsMod ? "MOD" : "KUNOS";

        public string DisplayTitle => string.IsNullOrWhiteSpace(Name) ? Id : Name;
        public string DisplayLocation => string.IsNullOrWhiteSpace(Country) 
            ? "GLOBAL CIRCUIT" 
            : $"{City}, {Country}".Trim(' ', ',');
        public string DisplayLength => Length.Contains('m') ? Length : $"{Length} m";
    }
}
