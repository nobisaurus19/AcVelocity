using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class ContentManagerUtilityService
    {
        public (bool IsValid, List<string> Issues) CheckCarIntegrity(CarInfo car, string acRoot)
        {
            var issues = new List<string>();
            if (string.IsNullOrEmpty(acRoot) || !Directory.Exists(acRoot))
            {
                issues.Add("Assetto Corsa directory not found.");
                return (false, issues);
            }

            var carDir = Path.Combine(acRoot, "content", "cars", car.Id);
            if (!Directory.Exists(carDir))
            {
                issues.Add("Car directory missing.");
                return (false, issues);
            }

            // Check Physics Data
            var dataAcd = Path.Combine(carDir, "data.acd");
            var dataFolder = Path.Combine(carDir, "data");
            if (!File.Exists(dataAcd) && !Directory.Exists(dataFolder))
            {
                issues.Add("Missing physics definitions (data.acd / data folder).");
            }

            // Check Sound Bank
            var sfxBank = Path.Combine(acRoot, "content", "sfx", $"{car.Id}.bank");
            var localBank = Path.Combine(carDir, "sfx", $"{car.Id}.bank");
            var sfxGuid = Path.Combine(acRoot, "content", "sfx", "GUIDs.txt");

            if (!File.Exists(sfxBank) && !File.Exists(localBank))
            {
                issues.Add("Audio soundbank missing (silent engine).");
            }

            // Check Skins
            var skinsDir = Path.Combine(carDir, "skins");
            if (!Directory.Exists(skinsDir) || Directory.GetDirectories(skinsDir).Length == 0)
            {
                issues.Add("No liveries/skins found in skins directory.");
            }

            return (issues.Count == 0, issues);
        }

        public async Task<(bool Success, string Message)> ToggleCarModAsync(CarInfo car, bool enable, string acRoot)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(acRoot) || !Directory.Exists(acRoot))
                    return (false, "AC root path invalid.");

                var carsDir = Path.Combine(acRoot, "content", "cars");
                var activeDir = Path.Combine(carsDir, car.Id);
                var disabledDir = Path.Combine(carsDir, car.Id + ".disabled");

                try
                {
                    if (enable)
                    {
                        if (Directory.Exists(disabledDir))
                        {
                            Directory.Move(disabledDir, activeDir);
                            return (true, $"Vehicle '{car.DisplayTitle}' activated.");
                        }
                    }
                    else
                    {
                        if (Directory.Exists(activeDir))
                        {
                            Directory.Move(activeDir, disabledDir);
                            return (true, $"Vehicle '{car.DisplayTitle}' deactivated (.disabled).");
                        }
                    }
                    return (false, "Directory not found in expected state.");
                }
                catch (Exception ex)
                {
                    return (false, $"Error toggling mod: {ex.Message}");
                }
            });
        }
    }
}
