# ⚡ AcVelocity (Velocity // Nexus)

> High-bandwidth Assetto Corsa Launcher & Custom Shaders Patch (CSP) Manager built with WPF & .NET 8 featuring a cyberpunk dark glassmorphism interface.

---

## 🌟 Key Features

- **🕹️ Simulation Launch Deck**: Instant dispatch for Practice, Hotlap, Race, and Drift sessions with autonomous `race.ini` dynamic generation.
- **🎛️ Presets Matrix**: One-click saving and switching between graphics, CSP, and FFB profiles.
- **🎮 Controls & FFB Configuration**: Wheel/gamepad axis calibration, Force Feedback gain, filter, and curb effects management.
- **⚡ Custom Shaders Patch (CSP)**: Comprehensive configuration for Lighting FX, Weather FX, Smart Shadows, Extra FX, and direct ZIP package deployment.
- **🌐 Online Matrix**: Dedicated server browsing and direct simulation joining.
- **⚙️ Graphics Core**: Resolution, MSAA, Anisotropic filtering, and FidelityFX FSR tuning.
- **🔊 Audio & Driving Assists**: Sound subsystem balancing, tyre wear acceleration, and mechanical damage rates.
- **📁 System Diagnostics & Cache Cleaner**: Shader cache cleanup, Content Manager synchronization, and health checks.
- **🎨 Skins & Liveries**: Interactive livery preview and skin ribbon.
- **📦 Mods Depot**: Mod directory explorer and archive installer.

---

## 🛠️ Architecture & Tech Stack

- **Framework**: .NET 8.0 Windows Desktop (WPF)
- **Language**: C# 12
- **UI Architecture**: MVVM pattern, XAML Glassmorphism, RangeObservableCollection for virtualized lists
- **AC Interop**: INI parser/generator for official AC configuration files (`race.ini`, `video.ini`, `audio.ini`, `controls.ini`) and CSP `extension/config/` modules.

---

## 🚀 Building & Running

### Prerequisites
- Windows 10/11 64-bit
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Assetto Corsa installed via Steam

### Build & Run
```powershell
# Clone the repository
git clone https://github.com/nobisaurus19/AcVelocity.git
cd AcVelocity

# Build in Release mode
dotnet build -c Release

# Run the launcher
dotnet run -c Release
```

### Publish Self-Contained Executable
```powershell
dotnet publish -c Release -o ./publish
```
