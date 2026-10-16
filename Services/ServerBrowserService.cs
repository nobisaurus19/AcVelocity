using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class ServerBrowserService
    {
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(4) };
        private static readonly HttpClient _downloadHttp = new() { Timeout = TimeSpan.FromMinutes(15) };

        static ServerBrowserService()
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("AcVelocity-Nexus/2.5");
            _downloadHttp.DefaultRequestHeaders.UserAgent.ParseAdd("AcVelocity-Nexus/2.5");
        }

        public async Task<List<ServerInfo>> LoadServersAsync()
        {
            var list = new List<ServerInfo>();
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var cmOnlineDir = Path.Combine(localAppData, "AcTools Content Manager", "Online Servers");

            var candidateFiles = new[]
            {
                Path.Combine(cmOnlineDir, "Favourites.txt"),
                Path.Combine(cmOnlineDir, "Recent.txt")
            };

            var parsedAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in candidateFiles)
            {
                if (!File.Exists(file)) continue;

                try
                {
                    var lines = await File.ReadAllLinesAsync(file);
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                        // Format: IP:PORT;NAME or IP:PORT
                        var parts = trimmed.Split(';', 2);
                        var addr = parts[0].Trim();
                        var name = parts.Length > 1 ? parts[1].Trim() : "AC Server";

                        if (parsedAddresses.Contains(addr)) continue;
                        parsedAddresses.Add(addr);

                        var hostPort = addr.Split(':');
                        if (hostPort.Length == 2 && int.TryParse(hostPort[1], out int port))
                        {
                            var s = new ServerInfo
                            {
                                Name = name,
                                Ip = hostPort[0],
                                Port = port,
                                HttpPort = port
                            };
                            list.Add(s);
                        }
                    }
                }
                catch { }
            }

            // Fallback default popular community servers if empty
            if (list.Count == 0)
            {
                list.Add(new ServerInfo { Name = "No Hesi | Public 1 | Heavy Traffic | SRP", Ip = "15.235.228.96", Port = 62001, HttpPort = 63001, Track = "Shuto Expressway", Clients = 14, MaxClients = 57, PingMs = 38 });
                list.Add(new ServerInfo { Name = "No Hesi | Public 2 | Heavy Traffic | SRP", Ip = "15.235.228.96", Port = 62002, HttpPort = 63002, Track = "Shuto Expressway", Clients = 19, MaxClients = 57, PingMs = 42 });
                list.Add(new ServerInfo { Name = "SRP - Jamaica Traffic Main | Official", Ip = "43.229.79.254", Port = 9600, HttpPort = 8081, Track = "Shuto Expressway", Clients = 19, MaxClients = 30, PingMs = 45 });
            }

            return list;
        }

        public static (string CleanTrackId, string LayoutId, int RequiredCspBuild) ParseTrackDescriptor(string rawTrack)
        {
            if (string.IsNullOrWhiteSpace(rawTrack))
                return (string.Empty, string.Empty, 0);

            int reqCsp = 0;
            var cspMatch = Regex.Match(rawTrack, @"csp/(\d+)", RegexOptions.IgnoreCase);
            if (cspMatch.Success && int.TryParse(cspMatch.Groups[1].Value, out int parsedBuild))
            {
                reqCsp = parsedBuild;
            }

            // Extract actual track segment after last '/' or '\'
            var normalized = rawTrack.Replace('\\', '/');
            var lastSlash = normalized.LastIndexOf('/');
            var trackPart = lastSlash >= 0 ? normalized.Substring(lastSlash + 1) : normalized;

            // AC format for layout in /INFO is often track_id-layout_id
            // Wait: some tracks have hyphens in their folder name, so we also check if '-' splits layout
            string cleanTrack = trackPart;
            string layout = string.Empty;

            int dashIdx = trackPart.LastIndexOf('-');
            if (dashIdx > 0)
            {
                cleanTrack = trackPart.Substring(0, dashIdx);
                layout = trackPart.Substring(dashIdx + 1);
            }

            return (cleanTrack.Trim(), layout.Trim(), reqCsp);
        }

        public async Task QueryServerStatsAsync(ServerInfo server)
        {
            var sw = Stopwatch.StartNew();
            var slotTracker = new Dictionary<string, (int Total, int Free, string Skin)>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // 1. Try AssettoServer's rich /api/details endpoint first
                string? detailsJson = null;
                try
                {
                    detailsJson = await _http.GetStringAsync($"http://{server.Ip}:{server.HttpPort}/api/details");
                }
                catch { }

                if (!string.IsNullOrEmpty(detailsJson))
                {
                    sw.Stop();
                    server.PingMs = Math.Max(1, (int)sw.ElapsedMilliseconds);
                    server.IsOnline = true;

                    using var doc = JsonDocument.Parse(detailsJson);
                    var root = doc.RootElement;

                    ParseBasicInfoElement(server, root);

                    // Parse content download URLs (content.cars & content.track)
                    if (root.TryGetProperty("content", out var contentElem) && contentElem.ValueKind == JsonValueKind.Object)
                    {
                        if (contentElem.TryGetProperty("cars", out var carsContent) && carsContent.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in carsContent.EnumerateObject())
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Object &&
                                    prop.Value.TryGetProperty("url", out var urlElem) &&
                                    urlElem.ValueKind == JsonValueKind.String)
                                {
                                    var url = urlElem.GetString();
                                    if (!string.IsNullOrWhiteSpace(url))
                                    {
                                        server.CarDownloadUrls[prop.Name] = url;
                                    }
                                }
                            }
                        }

                        if (contentElem.TryGetProperty("track", out var trackContent) &&
                            trackContent.ValueKind == JsonValueKind.Object &&
                            trackContent.TryGetProperty("url", out var tUrlElem) &&
                            tUrlElem.ValueKind == JsonValueKind.String)
                        {
                            server.TrackDownloadUrl = tUrlElem.GetString();
                        }
                    }

                    // Parse players.Cars slots
                    if (root.TryGetProperty("players", out var playersElem) &&
                        playersElem.ValueKind == JsonValueKind.Object &&
                        playersElem.TryGetProperty("Cars", out var carsArr) &&
                        carsArr.ValueKind == JsonValueKind.Array)
                    {
                        ParseSlotArray(carsArr, slotTracker);
                    }
                }
                else
                {
                    // 2. Standard AC Server fallback: /INFO + /JSON|Velocity
                    var infoUrl = $"http://{server.Ip}:{server.HttpPort}/INFO";
                    var infoJson = await _http.GetStringAsync(infoUrl);
                    sw.Stop();

                    server.PingMs = Math.Max(1, (int)sw.ElapsedMilliseconds);
                    server.IsOnline = true;

                    using (var doc = JsonDocument.Parse(infoJson))
                    {
                        ParseBasicInfoElement(server, doc.RootElement);
                    }

                    try
                    {
                        var jsonEp = await _http.GetStringAsync($"http://{server.Ip}:{server.HttpPort}/JSON|Velocity");
                        using var jDoc = JsonDocument.Parse(jsonEp);
                        if (jDoc.RootElement.TryGetProperty("Cars", out var cArr) && cArr.ValueKind == JsonValueKind.Array)
                        {
                            ParseSlotArray(cArr, slotTracker);
                        }
                    }
                    catch { }
                }

                // Build Initial CarSlots from RequiredCarIds & slotTracker
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    server.CarSlots.Clear();
                    foreach (var carId in server.RequiredCarIds)
                    {
                        int total = 1, free = 1;
                        string skin = "";
                        if (slotTracker.TryGetValue(carId, out var st))
                        {
                            total = st.Total;
                            free = st.Free;
                            skin = st.Skin;
                        }

                        server.CarDownloadUrls.TryGetValue(carId, out var dlUrl);

                        server.CarSlots.Add(new ServerCarSlotInfo
                        {
                            CarId = carId,
                            DisplayName = PrettifyId(carId),
                            Skin = skin,
                            TotalSlots = total,
                            FreeSlots = free,
                            DownloadUrl = dlUrl
                        });
                    }
                });
            }
            catch
            {
                server.IsOnline = false;
                server.PingMs = 999;
            }
        }

        private void ParseBasicInfoElement(ServerInfo server, JsonElement root)
        {
            if (root.TryGetProperty("name", out var n) && !string.IsNullOrEmpty(n.GetString()))
                server.Name = n.GetString()!;

            if (root.TryGetProperty("port", out var pProp) && pProp.TryGetInt32(out int gamePort) && gamePort > 0)
                server.Port = gamePort;
            else if (root.TryGetProperty("tport", out var tpProp) && tpProp.TryGetInt32(out int tPort) && tPort > 0)
                server.Port = tPort;

            if (root.TryGetProperty("cport", out var cpProp) && cpProp.TryGetInt32(out int httpPort) && httpPort > 0)
                server.HttpPort = httpPort;

            if (root.TryGetProperty("pass", out var passProp) && (passProp.ValueKind == JsonValueKind.True || passProp.ValueKind == JsonValueKind.False))
                server.HasPassword = passProp.GetBoolean();

            if (root.TryGetProperty("clients", out var cl) && cl.TryGetInt32(out int clients))
                server.Clients = clients;

            if (root.TryGetProperty("maxclients", out var mcl) && mcl.TryGetInt32(out int maxClients))
                server.MaxClients = maxClients;

            if (root.TryGetProperty("track", out var tr) && tr.ValueKind == JsonValueKind.String)
            {
                var rawTrack = tr.GetString() ?? "";
                server.RawTrackId = rawTrack;
                var (cleanTrack, layout, reqCsp) = ParseTrackDescriptor(rawTrack);
                server.CleanTrackId = cleanTrack;
                server.TrackLayoutId = layout;
                server.RequiredCspBuild = reqCsp;
                server.Track = string.IsNullOrEmpty(layout) ? cleanTrack : $"{cleanTrack} ({layout})";
            }

            if (root.TryGetProperty("cars", out var carsProp) && carsProp.ValueKind == JsonValueKind.Array)
            {
                server.RequiredCarIds.Clear();
                foreach (var c in carsProp.EnumerateArray())
                {
                    var cid = c.GetString();
                    if (!string.IsNullOrWhiteSpace(cid) && !server.RequiredCarIds.Contains(cid, StringComparer.OrdinalIgnoreCase))
                    {
                        server.RequiredCarIds.Add(cid.Trim());
                    }
                }
            }
        }

        private void ParseSlotArray(JsonElement carsArr, Dictionary<string, (int Total, int Free, string Skin)> slotTracker)
        {
            foreach (var item in carsArr.EnumerateArray())
            {
                var model = item.TryGetProperty("Model", out var mProp) ? mProp.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(model)) continue;

                bool isConnected = item.TryGetProperty("IsConnected", out var cProp) && cProp.ValueKind == JsonValueKind.True;
                string rawSkin = item.TryGetProperty("Skin", out var sProp) ? sProp.GetString() ?? "" : "";
                // Strip CSP extended skin suffix e.g. "01_yellow/ADAn" -> "01_yellow"
                string cleanSkin = rawSkin.Contains('/') ? rawSkin.Split('/')[0] : rawSkin;

                if (!slotTracker.TryGetValue(model, out var current))
                {
                    current = (0, 0, cleanSkin);
                }

                int total = current.Total + 1;
                int free = current.Free + (isConnected ? 0 : 1);
                string chosenSkin = (!isConnected && !string.IsNullOrEmpty(cleanSkin)) ? cleanSkin : current.Skin;

                slotTracker[model] = (total, free, chosenSkin);
            }
        }

        public void VerifyServerContent(
            ServerInfo server,
            IEnumerable<CarInfo> installedCars,
            IEnumerable<TrackInfo> installedTracks,
            CspSettings cspSettings,
            string acRoot)
        {
            if (server == null) return;

            var carDict = installedCars.ToDictionary(c => c.Id, c => c, StringComparer.OrdinalIgnoreCase);
            var trackDict = installedTracks.ToDictionary(t => t.Id, t => t, StringComparer.OrdinalIgnoreCase);

            // 1. Check CSP Requirement
            server.IsCspRequirementMet = server.RequiredCspBuild <= 0 ||
                (cspSettings.IsCspInstalled && cspSettings.MasterEnabled && cspSettings.CspBuild >= server.RequiredCspBuild);

            // 2. Check Track Installation
            if (!string.IsNullOrEmpty(server.CleanTrackId))
            {
                bool trackFound = trackDict.ContainsKey(server.CleanTrackId);
                if (!trackFound && !string.IsNullOrEmpty(acRoot))
                {
                    // Sometimes CleanTrackId had a hyphen that is part of the track folder name rather than layout
                    var fullRaw = server.RawTrackId.Replace('\\', '/');
                    var afterSlash = fullRaw.Contains('/') ? fullRaw.Substring(fullRaw.LastIndexOf('/') + 1) : fullRaw;

                    var directTrackDir = Path.Combine(acRoot, "content", "tracks", server.CleanTrackId);
                    var fullTrackDir = Path.Combine(acRoot, "content", "tracks", afterSlash);

                    if (Directory.Exists(directTrackDir))
                    {
                        trackFound = true;
                    }
                    else if (Directory.Exists(fullTrackDir))
                    {
                        trackFound = true;
                        server.CleanTrackId = afterSlash;
                        server.TrackLayoutId = string.Empty;
                    }
                }

                server.IsTrackInstalled = trackFound;
                if (trackDict.TryGetValue(server.CleanTrackId, out var matchedTrack))
                {
                    server.Track = matchedTrack.DisplayTitle;
                }
            }
            else
            {
                server.IsTrackInstalled = true;
            }

            // 3. Check Required Cars Installation
            int installedCount = 0;
            int missingCount = 0;
            int downloadableMissing = 0;
            var missingIds = new List<string>();

            foreach (var carId in server.RequiredCarIds)
            {
                bool isInstalled = carDict.ContainsKey(carId) ||
                    (!string.IsNullOrEmpty(acRoot) && Directory.Exists(Path.Combine(acRoot, "content", "cars", carId)));

                if (isInstalled)
                {
                    installedCount++;
                }
                else
                {
                    missingCount++;
                    missingIds.Add(carId);
                    if (server.CarDownloadUrls.ContainsKey(carId))
                    {
                        downloadableMissing++;
                    }
                }
            }

            if (!server.IsTrackInstalled && !string.IsNullOrEmpty(server.TrackDownloadUrl))
            {
                downloadableMissing++;
            }

            server.MissingCarIds = missingIds;
            server.InstalledCarCount = installedCount;
            server.MissingCarCount = missingCount;
            server.DownloadableMissingCount = downloadableMissing;

            // Update CarSlots metadata
            Application.Current?.Dispatcher.Invoke(() =>
            {
                foreach (var slot in server.CarSlots)
                {
                    if (carDict.TryGetValue(slot.CarId, out var cInfo))
                    {
                        slot.IsInstalled = true;
                        slot.DisplayName = cInfo.DisplayTitle;
                        slot.PreviewImagePath = cInfo.PreviewImagePath;
                        if (string.IsNullOrEmpty(slot.Skin))
                            slot.Skin = cInfo.SelectedSkin;
                    }
                    else
                    {
                        slot.IsInstalled = !string.IsNullOrEmpty(acRoot) &&
                            Directory.Exists(Path.Combine(acRoot, "content", "cars", slot.CarId));
                    }
                }
            });

            server.NotifyModStatusChanged();
        }

        public async Task<(int InstalledCount, int FailedCount, string Summary)> DownloadMissingServerModsAsync(
            ServerInfo server,
            string acRoot,
            ModInstallerService modInstaller,
            IProgress<(double Percent, string Status)>? progress = null)
        {
            if (server == null) return (0, 0, "No server selected.");
            if (string.IsNullOrEmpty(acRoot) || !Directory.Exists(acRoot)) return (0, 0, "Assetto Corsa root not found.");

            var missingWithUrls = server.MissingCarIds
                .Where(id => server.CarDownloadUrls.ContainsKey(id))
                .Select(id => (Id: id, Url: server.CarDownloadUrls[id], IsTrack: false))
                .ToList();

            if (!server.IsTrackInstalled && !string.IsNullOrEmpty(server.TrackDownloadUrl))
            {
                missingWithUrls.Insert(0, (Id: server.CleanTrackId, Url: server.TrackDownloadUrl!, IsTrack: true));
            }

            if (missingWithUrls.Count == 0)
            {
                return (0, 0, "No direct CDN download links provided by this server for the missing mods.");
            }

            int installed = 0;
            int failed = 0;
            int totalItems = missingWithUrls.Count;

            for (int i = 0; i < totalItems; i++)
            {
                var item = missingWithUrls[i];
                string ext = ".7z";
                try
                {
                    var uriPath = new Uri(item.Url).AbsolutePath;
                    var parsedExt = Path.GetExtension(uriPath);
                    if (!string.IsNullOrEmpty(parsedExt)) ext = parsedExt;
                }
                catch { }

                var tempArchive = Path.Combine(Path.GetTempPath(), $"AcOnlineMod_{item.Id}_{Guid.NewGuid():N}{ext}");

                try
                {
                    double basePct = (double)i / totalItems * 100.0;
                    double stepWeight = 100.0 / totalItems;

                    progress?.Report((basePct, $"[{i + 1}/{totalItems}] DOWNLOADING {item.Id.ToUpperInvariant()}..."));

                    using (var response = await _downloadHttp.GetAsync(item.Url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;

                        await using var stream = await response.Content.ReadAsStreamAsync();
                        await using var fs = new FileStream(tempArchive, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                        var buffer = new byte[81920];
                        long readTotal = 0;
                        int read;
                        DateTime lastUpdate = DateTime.MinValue;

                        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fs.WriteAsync(buffer, 0, read);
                            readTotal += read;

                            if ((DateTime.UtcNow - lastUpdate).TotalMilliseconds >= 120)
                            {
                                lastUpdate = DateTime.UtcNow;
                                double subPct = totalBytes > 0 ? (double)readTotal / totalBytes * 0.8 : 0.4;
                                double curPct = basePct + subPct * stepWeight;
                                double mb = readTotal / (1024.0 * 1024.0);
                                progress?.Report((curPct, $"[{i + 1}/{totalItems}] DOWNLOADING {item.Id}: {mb:F1} MB..."));
                            }
                        }
                    }

                    progress?.Report((basePct + 0.85 * stepWeight, $"[{i + 1}/{totalItems}] EXTRACTING {item.Id} TO CONTENT DEPOT..."));

                    var res = await modInstaller.InstallZipModAsync(tempArchive, acRoot, item.IsTrack ? null : item.Id);
                    if (res.Success)
                    {
                        installed++;
                    }
                    else
                    {
                        failed++;
                    }
                }
                catch
                {
                    failed++;
                }
                finally
                {
                    try { if (File.Exists(tempArchive)) File.Delete(tempArchive); } catch { }
                }
            }

            progress?.Report((100, $"COMPLETED: {installed} MODS INSTALLED" + (failed > 0 ? $", {failed} FAILED" : "")));
            return (installed, failed, $"Installed {installed}/{totalItems} missing server mods.");
        }

        private static string PrettifyId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            var parts = id.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : "")));
        }
    }
}
