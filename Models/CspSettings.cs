using System;
using System.Collections.Generic;

namespace AcVelocity.Models
{
    public class CspSettings
    {
        public bool IsCspInstalled { get; set; } = false;
        public string CspVersion { get; set; } = "Not Detected";
        public int CspBuild { get; set; } = 0;
        public double DwriteSizeMb { get; set; } = 0.0;
        public bool MasterEnabled { get; set; } = true;

        // Dynamic Options
        public List<string> AvailableWeatherImplementations { get; set; } = new();
        public List<string> AvailableWeatherControllers { get; set; } = new();

        // Weather FX
        public bool WeatherFxEnabled { get; set; } = true;
        public string WeatherController { get; set; } = "pureCtrl static";
        public string WeatherImplementation { get; set; } = "Pure LCS";
        public double TimeProgressionMultiplier { get; set; } = 1.0;
        public bool MirageReflections { get; set; } = true;

        // Lighting FX
        public bool LightingFxEnabled { get; set; } = true;
        public double HeadlightsRangeMult { get; set; } = 1.0;
        public bool BouncedLight { get; set; } = true;
        public double BouncedLightMult { get; set; } = 0.25;
        public bool DynamicCarLights { get; set; } = true;
        public bool FullResShadows { get; set; } = true;
        public int CarsWithLights { get; set; } = 15;

        // Extra FX
        public bool ExtraFxEnabled { get; set; } = true;
        public bool SslrEnabled { get; set; } = true;
        public int SslrTracingQuality { get; set; } = 2; // 0=Off, 1=Low, 2=Med, 3=High, 4=HiZ
        public bool SsgiEnabled { get; set; } = true;
        public bool HbaoEnabled { get; set; } = true;
        public bool TaaEnabled { get; set; } = false;
        public bool MotionBlurEnabled { get; set; } = true;
        public double MotionBlurMult { get; set; } = 0.75;

        // Rain FX
        public bool RainFxEnabled { get; set; } = true;
        public bool ScreenDrops { get; set; } = true;
        public double DropsIntensity { get; set; } = 1.0;
        public double SplashMult { get; set; } = 0.15;

        // Immersion & Realism
        public bool NeckFxEnabled { get; set; } = true;
        public double NeckLookaheadGain { get; set; } = 0.2;
        public bool SmartMirrorEnabled { get; set; } = true;
        public bool GrassFxEnabled { get; set; } = true;
        public int GrassQuality { get; set; } = 4;
        public bool ParticlesFxEnabled { get; set; } = true;
        public bool BrakediscFxEnabled { get; set; } = true;
        public bool WindscreenFxEnabled { get; set; } = true;

        // Graphics Adjustments
        public bool GraphicsAdjustmentsEnabled { get; set; } = true;
        public double FfxCasSharpness { get; set; } = 0.2;
        public double LodMultiplier { get; set; } = 1.0;

        public string StatusDisplay => IsCspInstalled 
            ? (MasterEnabled ? $"CSP v{CspVersion} (BUILD {CspBuild}) // ONLINE" : $"CSP v{CspVersion} (BUILD {CspBuild}) // DISABLED")
            : "CUSTOM SHADERS PATCH NOT DETECTED";
    }
}
