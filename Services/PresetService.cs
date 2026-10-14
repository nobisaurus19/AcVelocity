using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class PresetService
    {
        private readonly string _presetsFolder;

        public PresetService()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            _presetsFolder = Path.Combine(appDir, "presets");
            if (!Directory.Exists(_presetsFolder))
            {
                Directory.CreateDirectory(_presetsFolder);
            }
        }

        public List<NexusPreset> GetBuiltInPresets()
        {
            return new List<NexusPreset>
            {
                new NexusPreset
                {
                    Name = "COMPETITIVE_MAX_FPS",
                    Description = "Low rendering overhead, motion blur disabled, max clarity for 240Hz/144Hz high-refresh eSports racing.",
                    Icon = "⚡",
                    Video = new AcVideoSettings { Msaa = 2, ShadowMapSize = 1024, Fxaa = false, VSync = false },
                    Csp = new CspSettings
                    {
                        MasterEnabled = true,
                        ExtraFxEnabled = false,
                        LightingFxEnabled = true,
                        HeadlightsRangeMult = 0.8,
                        FullResShadows = false,
                        CarsWithLights = 6,
                        GrassFxEnabled = false,
                        ParticlesFxEnabled = true,
                        NeckFxEnabled = false
                    },
                    Ffb = new FfbSettings { MasterGain = 0.95, Filter = 0.05, KerbsEffect = 0.1, RoadEffect = 0.05, SlipEffect = 0.05 }
                },
                new NexusPreset
                {
                    Name = "BALANCED_SMOOTH",
                    Description = "Optimal blend of crisp visuals, Pure LCS skies, medium Extra FX SSR reflections, and smooth 120-144 FPS.",
                    Icon = "⚖️",
                    Video = new AcVideoSettings { Msaa = 4, ShadowMapSize = 2048, Fxaa = true, VSync = false },
                    Csp = new CspSettings
                    {
                        MasterEnabled = true,
                        WeatherFxEnabled = true,
                        WeatherImplementation = "Pure LCS",
                        ExtraFxEnabled = true,
                        SslrEnabled = true,
                        SslrTracingQuality = 2,
                        HbaoEnabled = true,
                        TaaEnabled = false,
                        MotionBlurEnabled = false,
                        LightingFxEnabled = true,
                        HeadlightsRangeMult = 1.0,
                        BouncedLight = true,
                        CarsWithLights = 15,
                        GrassFxEnabled = true,
                        ParticlesFxEnabled = true,
                        NeckFxEnabled = true,
                        NeckLookaheadGain = 0.2
                    },
                    Ffb = new FfbSettings { MasterGain = 1.0, Filter = 0.0, KerbsEffect = 0.05, RoadEffect = 0.0, SlipEffect = 0.0 }
                },
                new NexusPreset
                {
                    Name = "CINEMATIC_PHOTOREAL",
                    Description = "Maximum photorealism: Full Extra FX (SSLR Hi-Z, SSGI, HBAO+, TAA), 4K Shadows, Pure LCS, Rain drops, and bounced light.",
                    Icon = "🎬",
                    Video = new AcVideoSettings { Msaa = 4, ShadowMapSize = 4096, Fxaa = true, VSync = false },
                    Csp = new CspSettings
                    {
                        MasterEnabled = true,
                        WeatherFxEnabled = true,
                        WeatherImplementation = "Pure LCS",
                        ExtraFxEnabled = true,
                        SslrEnabled = true,
                        SslrTracingQuality = 3,
                        SsgiEnabled = true,
                        HbaoEnabled = true,
                        TaaEnabled = true,
                        MotionBlurEnabled = true,
                        MotionBlurMult = 0.75,
                        LightingFxEnabled = true,
                        HeadlightsRangeMult = 1.2,
                        BouncedLight = true,
                        FullResShadows = true,
                        CarsWithLights = 24,
                        RainFxEnabled = true,
                        ScreenDrops = true,
                        DropsIntensity = 1.2,
                        GrassFxEnabled = true,
                        SmartMirrorEnabled = true,
                        ParticlesFxEnabled = true,
                        NeckFxEnabled = true,
                        NeckLookaheadGain = 0.25
                    },
                    Ffb = new FfbSettings { MasterGain = 1.0, Filter = 0.0, KerbsEffect = 0.15, RoadEffect = 0.1, SlipEffect = 0.1 }
                },
                new NexusPreset
                {
                    Name = "NIGHT_TOUGE_DRIFT",
                    Description = "Configured specifically for night runs & drifting: Intense headlight beams, smoke particles, Neck FX apex drift lookahead.",
                    Icon = "🌙",
                    Video = new AcVideoSettings { Msaa = 4, ShadowMapSize = 2048, Fxaa = true, VSync = false },
                    Csp = new CspSettings
                    {
                        MasterEnabled = true,
                        WeatherFxEnabled = true,
                        WeatherImplementation = "Pure LCS",
                        LightingFxEnabled = true,
                        HeadlightsRangeMult = 1.5,
                        BouncedLight = true,
                        CarsWithLights = 18,
                        ExtraFxEnabled = true,
                        SslrEnabled = true,
                        SslrTracingQuality = 2,
                        HbaoEnabled = true,
                        MotionBlurEnabled = true,
                        MotionBlurMult = 0.85,
                        ParticlesFxEnabled = true,
                        NeckFxEnabled = true,
                        NeckLookaheadGain = 0.55
                    },
                    Ffb = new FfbSettings { MasterGain = 1.05, Filter = 0.0, KerbsEffect = 0.0, RoadEffect = 0.0, SlipEffect = 0.15 }
                }
            };
        }

        public async Task SavePresetAsync(NexusPreset preset)
        {
            var fileName = $"{preset.Name.Replace(" ", "_")}.json";
            var filePath = Path.Combine(_presetsFolder, fileName);
            var json = JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<NexusPreset>> LoadCustomPresetsAsync()
        {
            var list = new List<NexusPreset>();
            if (!Directory.Exists(_presetsFolder)) return list;

            var files = Directory.GetFiles(_presetsFolder, "*.json");
            foreach (var f in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(f);
                    var p = JsonSerializer.Deserialize<NexusPreset>(json);
                    if (p != null) list.Add(p);
                }
                catch { }
            }
            return list;
        }
    }
}
