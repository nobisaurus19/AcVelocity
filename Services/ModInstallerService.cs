using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AcVelocity.Services
{
    public class ModInstallResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ModType { get; set; } = "UNKNOWN"; // "CAR", "TRACK", "GENERIC"
        public string ModName { get; set; } = string.Empty;
    }

    public class ModInstallerService
    {
        public async Task<ModInstallResult> InstallZipModAsync(string zipFilePath, string acRoot)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(zipFilePath))
                {
                    return new ModInstallResult { Success = false, Message = "File does not exist." };
                }

                if (string.IsNullOrEmpty(acRoot) || !Directory.Exists(acRoot))
                {
                    return new ModInstallResult { Success = false, Message = "Invalid Assetto Corsa root path." };
                }

                var fileName = Path.GetFileNameWithoutExtension(zipFilePath);
                var tempExtract = Path.Combine(Path.GetTempPath(), "AcMod_" + Guid.NewGuid().ToString("N"));

                try
                {
                    Directory.CreateDirectory(tempExtract);
                    ZipFile.ExtractToDirectory(zipFilePath, tempExtract);

                    // Inspect structure
                    // Pattern 1: Contains 'content/cars' or 'content/tracks'
                    var contentFolder = Directory.GetDirectories(tempExtract, "content", SearchOption.AllDirectories).FirstOrDefault();
                    if (contentFolder != null)
                    {
                        var targetContent = Path.Combine(acRoot, "content");
                        CopyDirectory(contentFolder, targetContent);
                        return new ModInstallResult 
                        { 
                            Success = true, 
                            ModType = "STANDARD CONTENT", 
                            ModName = fileName,
                            Message = $"Successfully integrated package '{fileName}' into game content depot." 
                        };
                    }

                    // Pattern 2: Directly a car folder (contains 'data' or 'ui' or 'skins' or 'body_shadow.ini')
                    bool isCar = Directory.GetDirectories(tempExtract, "skins", SearchOption.AllDirectories).Any() ||
                                 Directory.GetDirectories(tempExtract, "ui", SearchOption.AllDirectories).Any() ||
                                 Directory.GetFiles(tempExtract, "data.acd", SearchOption.AllDirectories).Any();

                    if (isCar)
                    {
                        var targetCars = Path.Combine(acRoot, "content", "cars");
                        // Find the folder that contains skins/ui
                        var carSubDir = Directory.GetDirectories(tempExtract, "skins", SearchOption.AllDirectories).FirstOrDefault();
                        var carRoot = carSubDir != null ? Directory.GetParent(carSubDir)?.FullName : tempExtract;
                        var destDir = Path.Combine(targetCars, Path.GetFileName(carRoot ?? fileName));

                        if (carRoot != null)
                        {
                            CopyDirectory(carRoot, destDir);
                            return new ModInstallResult 
                            { 
                                Success = true, 
                                ModType = "VEHICLE", 
                                ModName = Path.GetFileName(destDir),
                                Message = $"Vehicle '{Path.GetFileName(destDir)}' installed into hangar." 
                            };
                        }
                    }

                    // Pattern 3: Directly a track folder (contains 'models.ini' or 'surfaces.ini')
                    bool isTrack = Directory.GetFiles(tempExtract, "models*.ini", SearchOption.AllDirectories).Any() ||
                                  Directory.GetFiles(tempExtract, "surfaces.ini", SearchOption.AllDirectories).Any();

                    if (isTrack)
                    {
                        var targetTracks = Path.Combine(acRoot, "content", "tracks");
                        var modelsFile = Directory.GetFiles(tempExtract, "models*.ini", SearchOption.AllDirectories).FirstOrDefault();
                        var trackRoot = modelsFile != null ? Directory.GetParent(modelsFile)?.FullName : tempExtract;
                        var destDir = Path.Combine(targetTracks, Path.GetFileName(trackRoot ?? fileName));

                        if (trackRoot != null)
                        {
                            CopyDirectory(trackRoot, destDir);
                            return new ModInstallResult 
                            { 
                                Success = true, 
                                ModType = "CIRCUIT", 
                                ModName = Path.GetFileName(destDir),
                                Message = $"Circuit '{Path.GetFileName(destDir)}' installed into track matrix." 
                            };
                        }
                    }

                    // Fallback copy all to content
                    CopyDirectory(tempExtract, Path.Combine(acRoot, "content"));
                    return new ModInstallResult 
                    { 
                        Success = true, 
                        ModType = "GENERIC MOD", 
                        ModName = fileName,
                        Message = $"Extracted generic mod '{fileName}' into game directory." 
                    };
                }
                catch (Exception ex)
                {
                    return new ModInstallResult { Success = false, Message = $"Extraction error: {ex.Message}" };
                }
                finally
                {
                    try { if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true); } catch { }
                }
            });
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) return;

            Directory.CreateDirectory(destinationDir);

            foreach (var file in dir.GetFiles())
            {
                var targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (var subDir in dir.GetDirectories())
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }
    }
}
