using System;
using System.Diagnostics;
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
        public string? FindSevenZipExecutable()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var cm7z = Path.Combine(localAppData, "AcTools Content Manager", "Plugins", "7Zip", "7z.exe");
            if (File.Exists(cm7z)) return cm7z;

            var pf7z = @"C:\Program Files\7-Zip\7z.exe";
            if (File.Exists(pf7z)) return pf7z;

            var pfx86 = @"C:\Program Files (x86)\7-Zip\7z.exe";
            if (File.Exists(pfx86)) return pfx86;

            return null;
        }

        private void ExtractArchiveToDirectory(string archiveFilePath, string destinationDir)
        {
            var ext = Path.GetExtension(archiveFilePath).ToLowerInvariant();
            if (ext == ".zip")
            {
                ZipFile.ExtractToDirectory(archiveFilePath, destinationDir, overwriteFiles: true);
                return;
            }

            var sevenZip = FindSevenZipExecutable();
            if (sevenZip != null)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = sevenZip,
                    Arguments = $"x \"{archiveFilePath}\" -o\"{destinationDir}\" -y",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit(180_000);
                if (proc != null && proc.ExitCode == 0)
                {
                    return;
                }
            }

            // Fallback: attempt ZipFile in case it's a zip with a different extension
            ZipFile.ExtractToDirectory(archiveFilePath, destinationDir, overwriteFiles: true);
        }

        public async Task<ModInstallResult> InstallZipModAsync(string archiveFilePath, string acRoot, string? expectedCarId = null)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(archiveFilePath))
                {
                    return new ModInstallResult { Success = false, Message = "File does not exist." };
                }

                if (string.IsNullOrEmpty(acRoot) || !Directory.Exists(acRoot))
                {
                    return new ModInstallResult { Success = false, Message = "Invalid Assetto Corsa root path." };
                }

                var fileName = Path.GetFileNameWithoutExtension(archiveFilePath);
                var tempExtract = Path.Combine(Path.GetTempPath(), "AcMod_" + Guid.NewGuid().ToString("N"));

                try
                {
                    Directory.CreateDirectory(tempExtract);
                    ExtractArchiveToDirectory(archiveFilePath, tempExtract);

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
                            ModName = expectedCarId ?? fileName,
                            Message = $"Successfully integrated package '{expectedCarId ?? fileName}' into game content depot." 
                        };
                    }

                    // Pattern 2: Directly a car folder (contains 'data' or 'ui' or 'skins' or 'data.acd' or '.kn5')
                    bool isCar = Directory.GetDirectories(tempExtract, "skins", SearchOption.AllDirectories).Any() ||
                                 Directory.GetDirectories(tempExtract, "sfx", SearchOption.AllDirectories).Any() ||
                                 Directory.GetFiles(tempExtract, "data.acd", SearchOption.AllDirectories).Any() ||
                                 !string.IsNullOrEmpty(expectedCarId);

                    if (isCar)
                    {
                        var targetCars = Path.Combine(acRoot, "content", "cars");
                        var carSubDir = Directory.GetDirectories(tempExtract, "skins", SearchOption.AllDirectories).FirstOrDefault()
                                     ?? Directory.GetDirectories(tempExtract, "sfx", SearchOption.AllDirectories).FirstOrDefault();

                        string? carRoot = null;
                        if (carSubDir != null)
                        {
                            carRoot = Directory.GetParent(carSubDir)?.FullName;
                        }
                        else
                        {
                            var acdFile = Directory.GetFiles(tempExtract, "data.acd", SearchOption.AllDirectories).FirstOrDefault();
                            if (acdFile != null) carRoot = Path.GetDirectoryName(acdFile);
                        }

                        carRoot ??= tempExtract;

                        string carFolderName = !string.IsNullOrEmpty(expectedCarId)
                            ? expectedCarId
                            : (carRoot == tempExtract ? fileName : Path.GetFileName(carRoot));

                        var destDir = Path.Combine(targetCars, carFolderName);
                        CopyDirectory(carRoot, destDir);

                        return new ModInstallResult 
                        { 
                            Success = true, 
                            ModType = "VEHICLE", 
                            ModName = carFolderName,
                            Message = $"Vehicle '{carFolderName}' installed into hangar." 
                        };
                    }

                    // Pattern 3: Directly a track folder (contains 'models.ini' or 'surfaces.ini')
                    bool isTrack = Directory.GetFiles(tempExtract, "models*.ini", SearchOption.AllDirectories).Any() ||
                                  Directory.GetFiles(tempExtract, "surfaces.ini", SearchOption.AllDirectories).Any();

                    if (isTrack)
                    {
                        var targetTracks = Path.Combine(acRoot, "content", "tracks");
                        var modelsFile = Directory.GetFiles(tempExtract, "models*.ini", SearchOption.AllDirectories).FirstOrDefault();
                        var trackRoot = modelsFile != null ? Directory.GetParent(modelsFile)?.FullName : tempExtract;
                        string trackFolderName = trackRoot == tempExtract || trackRoot == null ? fileName : Path.GetFileName(trackRoot);
                        var destDir = Path.Combine(targetTracks, trackFolderName);

                        if (trackRoot != null)
                        {
                            CopyDirectory(trackRoot, destDir);
                            return new ModInstallResult 
                            { 
                                Success = true, 
                                ModType = "CIRCUIT", 
                                ModName = trackFolderName,
                                Message = $"Circuit '{trackFolderName}' installed into track matrix." 
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
