using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AcVelocity.Services;

namespace AcVelocity.Models
{
    public class NexusPreset
    {
        public string Name { get; set; } = "Default Profile";
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "⚡";

        public AcVideoSettings Video { get; set; } = new();
        public CspSettings Csp { get; set; } = new();
        public FfbSettings Ffb { get; set; } = new();
    }
}
