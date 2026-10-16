using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class AcLauncherService
    {
        public string GetCfgDirectory()
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var cfgDir = Path.Combine(myDocs, "Assetto Corsa", "cfg");
            if (!Directory.Exists(cfgDir))
            {
                Directory.CreateDirectory(cfgDir);
            }
            return cfgDir;
        }

        public async Task<string> GenerateRaceIniAsync(
            CarInfo car, 
            TrackInfo track, 
            string sessionMode, 
            int aiCount, 
            int aiDifficulty, 
            int sunAngle,
            string weather,
            int trackGrip = 100,
            int ambientTemp = 24,
            int roadTemp = 30,
            int timeMultiplier = 1)
        {
            var cfgDir = GetCfgDirectory();
            var raceIniPath = Path.Combine(cfgDir, "race.ini");

            bool hasAi = (sessionMode == "RACE" || sessionMode == "PRACTICE") && aiCount > 0;
            if (sessionMode == "HOTLAP" || sessionMode == "DRIFT")
            {
                hasAi = false;
            }

            int totalCars = hasAi ? (aiCount + 1) : 1;

            var sb = new StringBuilder();
            sb.AppendLine("[HEADER]");
            sb.AppendLine("VERSION=2");
            sb.AppendLine();

            sb.AppendLine("[RACE]");
            sb.AppendLine($"TRACK={track.Id}");
            sb.AppendLine($"CONFIG_TRACK={track.SubConfig}");
            sb.AppendLine($"MODEL={car.Id}");
            sb.AppendLine($"CARS={totalCars}");
            sb.AppendLine($"AI_LEVEL={aiDifficulty}");
            sb.AppendLine("FIXED_SETUP=0");
            sb.AppendLine("PENALTIES=1");
            sb.AppendLine($"SKIN={car.SelectedSkin}");
            sb.AppendLine($"DRIFT_MODE={(sessionMode == "DRIFT" ? 1 : 0)}");
            sb.AppendLine();

            sb.AppendLine("[GHOST_CAR]");
            sb.AppendLine("RECORDING=0");
            sb.AppendLine("PLAYING=0");
            sb.AppendLine("SECONDS_ADVANTAGE=0");
            sb.AppendLine("LOAD=1");
            sb.AppendLine("FILE=");
            sb.AppendLine("ENABLED=0");
            sb.AppendLine();

            sb.AppendLine("[LIGHTING]");
            sb.AppendLine($"SUN_ANGLE={sunAngle}");
            sb.AppendLine($"TIME_OF_DAY_MULT={Math.Max(1, timeMultiplier)}");
            sb.AppendLine("CLOUD_SPEED=1");
            sb.AppendLine();

            sb.AppendLine("[TEMPERATURE]");
            sb.AppendLine($"AMBIENT={ambientTemp}");
            sb.AppendLine($"ROAD={roadTemp}");
            sb.AppendLine();

            sb.AppendLine("[GROOVE]");
            sb.AppendLine("VIRTUAL_LM=0.1");
            sb.AppendLine("MAX_LAPS=30");
            sb.AppendLine("STARTING_LAPS=1");
            sb.AppendLine();

            sb.AppendLine("[DYNAMIC_TRACK]");
            sb.AppendLine($"SESSION_START={trackGrip}");
            sb.AppendLine("RANDOMNESS=0");
            sb.AppendLine("LAP_GAIN=10");
            sb.AppendLine("SESSION_TRANSFER=100");
            sb.AppendLine();

            sb.AppendLine("[WEATHER]");
            sb.AppendLine($"NAME={weather}");
            sb.AppendLine();

            sb.AppendLine("[REMOTE]");
            sb.AppendLine("ACTIVE=0");
            sb.AppendLine("SERVER_IP=");
            sb.AppendLine("SERVER_PORT=");
            sb.AppendLine("NAME=");
            sb.AppendLine("TEAM=");
            sb.AppendLine("GUID=");
            sb.AppendLine("REQUESTED_CAR=");
            sb.AppendLine("PASSWORD=");
            sb.AppendLine();

            sb.AppendLine("[CAR_0]");
            sb.AppendLine("SETUP=");
            sb.AppendLine($"SKIN={car.SelectedSkin}");
            sb.AppendLine($"MODEL={car.Id}");
            sb.AppendLine("MODEL_CONFIG=");
            sb.AppendLine("BALLAST=0");
            sb.AppendLine("RESTRICTOR=0");
            sb.AppendLine("DRIVER_NAME=Player");
            sb.AppendLine("NATIONALITY=THA");
            sb.AppendLine("NATION_CODE=THA");
            sb.AppendLine();

            if (hasAi)
            {
                for (int i = 1; i <= aiCount; i++)
                {
                    string aiSkin = car.SelectedSkin;
                    if (car.Skins != null && car.Skins.Count > 0)
                    {
                        aiSkin = car.Skins[i % car.Skins.Count];
                    }

                    sb.AppendLine($"[CAR_{i}]");
                    sb.AppendLine("SETUP=");
                    sb.AppendLine($"SKIN={aiSkin}");
                    sb.AppendLine($"MODEL={car.Id}");
                    sb.AppendLine("MODEL_CONFIG=");
                    sb.AppendLine("BALLAST=0");
                    sb.AppendLine("RESTRICTOR=0");
                    sb.AppendLine($"DRIVER_NAME=AI_Pilot_{i:00}");
                    sb.AppendLine($"AI_LEVEL={aiDifficulty}");
                    sb.AppendLine();
                }
            }

            int sessionType = sessionMode switch
            {
                "RACE" => 3,       // TYPE_RACE
                "HOTLAP" => 4,     // TYPE_HOTLAP
                "DRIFT" => 6,      // TYPE_DRIFT
                _ => 1             // TYPE_PRACTICE
            };

            string spawnSet = sessionMode switch
            {
                "RACE" => "START",
                "HOTLAP" => "HOTLAP_START",
                "DRIFT" => "START",
                _ => "PIT"         // PRACTICE
            };

            sb.AppendLine("[SESSION_0]");
            sb.AppendLine($"NAME={sessionMode}");
            sb.AppendLine($"TYPE={sessionType}");
            if (sessionMode == "RACE")
            {
                sb.AppendLine("LAPS=5");
            }
            else
            {
                sb.AppendLine("DURATION_MINUTES=60");
            }
            sb.AppendLine($"SPAWN_SET={spawnSet}");
            sb.AppendLine();

            await File.WriteAllTextAsync(raceIniPath, sb.ToString(), Encoding.UTF8);
            return raceIniPath;
        }

        public async Task<string> GenerateOnlineRaceIniAsync(
            string serverIp,
            int serverPort,
            string password,
            CarInfo car,
            string pilotName,
            int serverHttpPort = 0,
            string serverName = "",
            string? carIdOverride = null,
            string? skinOverride = null)
        {
            var cfgDir = GetCfgDirectory();
            var raceIniPath = Path.Combine(cfgDir, "race.ini");

            string targetCarId = !string.IsNullOrWhiteSpace(carIdOverride) ? carIdOverride.Trim() : car.Id;
            string targetSkin = !string.IsNullOrWhiteSpace(skinOverride) ? skinOverride.Trim() : car.SelectedSkin;
            int httpPort = serverHttpPort > 0 ? serverHttpPort : serverPort;

            var sb = new StringBuilder();
            sb.AppendLine("[HEADER]");
            sb.AppendLine("VERSION=2");
            sb.AppendLine();

            sb.AppendLine("[RACE]");
            sb.AppendLine($"MODEL={targetCarId}");
            sb.AppendLine("CARS=1");
            sb.AppendLine($"SKIN={targetSkin}");
            sb.AppendLine();

            sb.AppendLine("[REMOTE]");
            sb.AppendLine("ACTIVE=1");
            sb.AppendLine($"SERVER_IP={serverIp.Trim()}");
            sb.AppendLine($"SERVER_PORT={serverPort}");
            sb.AppendLine($"SERVER_HTTP_PORT={httpPort}");
            if (!string.IsNullOrWhiteSpace(serverName))
            {
                sb.AppendLine($"SERVER_NAME={serverName.Trim()}");
            }
            sb.AppendLine($"NAME={pilotName.Trim()}");
            sb.AppendLine("TEAM=Velocity Racing");
            sb.AppendLine("GUID=");
            sb.AppendLine($"REQUESTED_CAR={targetCarId}");
            sb.AppendLine($"PASSWORD={password.Trim()}");
            sb.AppendLine();

            sb.AppendLine("[CAR_0]");
            sb.AppendLine("SETUP=");
            sb.AppendLine($"SKIN={targetSkin}");
            sb.AppendLine($"MODEL={targetCarId}");
            sb.AppendLine("MODEL_CONFIG=");
            sb.AppendLine($"DRIVER_NAME={pilotName.Trim()}");
            sb.AppendLine("NATIONALITY=THA");
            sb.AppendLine("NATION_CODE=THA");
            sb.AppendLine();

            await File.WriteAllTextAsync(raceIniPath, sb.ToString(), Encoding.UTF8);
            return raceIniPath;
        }

        public async Task<(bool success, string message)> LaunchGameAsync(string acRoot)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var acsExe = Path.Combine(acRoot, "acs.exe");
                    if (!File.Exists(acsExe))
                    {
                        acsExe = Path.Combine(acRoot, "AssettoCorsa.exe");
                    }

                    if (!File.Exists(acsExe))
                    {
                        return (false, $"Game binary not found at: {acRoot}\\acs.exe");
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = acsExe,
                        WorkingDirectory = acRoot,
                        UseShellExecute = true
                    };

                    var proc = Process.Start(psi);
                    if (proc != null)
                    {
                        return (true, $"Process initiated: [PID {proc.Id}] {Path.GetFileName(acsExe)}");
                    }

                    return (false, "Failed to start game process.");
                }
                catch (Exception ex)
                {
                    return (false, $"Execution error: {ex.Message}");
                }
            });
        }
    }
}
