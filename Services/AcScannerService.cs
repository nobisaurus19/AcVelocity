using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AcVelocity.Models;

namespace AcVelocity.Services
{
    public class AcScannerService
    {
        private static readonly HashSet<string> OfficialCarIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "abarth500", "abarth500_s1", "alfa_romeo_giulietta_qv", "alfa_romeo_giulietta_qv_le",
            "bmw_1m", "bmw_1m_s3", "bmw_m3_e30", "bmw_m3_e30_drift", "bmw_m3_e30_dtm", "bmw_m3_e30_gra",
            "bmw_m3_e30_s1", "bmw_m3_e92", "bmw_m3_e92_drift", "bmw_m3_e92_s1", "bmw_m3_gt2",
            "bmw_z4", "bmw_z4_drift", "bmw_z4_gt3", "bmw_z4_s1", "fc1_2015_brackley_gp",
            "ferrari_312t", "ferrari_458", "ferrari_458_gt2", "ferrari_458_s3", "ferrari_599xxevo",
            "ferrari_f40", "ferrari_f40_s3", "ferrari_laferrari", "ks_abarth500_assetto_corse",
            "ks_abarth_595ss", "ks_abarth_595ss_s1", "ks_abarth_595ss_s2", "ks_alfa_33_stradale",
            "ks_alfa_giulia_qv", "ks_alfa_giulia_veloce", "ks_alfa_mito_qv", "ks_alfa_romeo_155_v6",
            "ks_alfa_romeo_4c", "ks_alfa_romeo_gta", "ks_audi_a1s1", "ks_audi_r18_etron_quattro",
            "ks_audi_r8_lms", "ks_audi_r8_lms_2016", "ks_audi_r8_plus", "ks_audi_sport_quattro",
            "ks_audi_sport_quattro_s1", "ks_audi_sport_quattro_rally", "ks_audi_tt_cup", "ks_audi_tt_vln",
            "ks_audio_test", "ks_bmw_m235i_racing", "ks_bmw_m4", "ks_bmw_m4_akrapovic",
            "ks_corvette_c7_stingray", "ks_corvette_c7r", "ks_ferrari_250_gto", "ks_ferrari_288_gto",
            "ks_ferrari_312_67", "ks_ferrari_330_p4", "ks_ferrari_488_gt3", "ks_ferrari_488_gtb",
            "ks_ferrari_812_superfast", "ks_ferrari_f138", "ks_ferrari_f2004", "ks_ferrari_fxx_k",
            "ks_ferrari_Sf15t", "ks_ferrari_sf70h", "ks_ford_escort_mk1", "ks_ford_gt40",
            "ks_ford_mustang_2015", "ks_glickenhaus_scg003", "ks_lamborghini_aventador_sv",
            "ks_lamborghini_countach", "ks_lamborghini_countach_s1", "ks_lamborghini_gallardo_sl",
            "ks_lamborghini_gallardo_sl_s3", "ks_lamborghini_huracan_gt3", "ks_lamborghini_huracan_performante",
            "ks_lamborghini_huracan_st", "ks_lamborghini_miura_sv", "ks_lamborghini_sesto_elemento",
            "ks_lotus_25", "ks_lotus_3_eleven", "ks_lotus_72d", "ks_maserati_250f_12cyl",
            "ks_maserati_250f_6cyl", "ks_maserati_alfieri", "ks_maserati_gt_mc_gt4",
            "ks_maserati_levante", "ks_maserati_mc12_gt1", "ks_maserati_quattroporte",
            "ks_mazda_787b", "ks_mazda_miata", "ks_mazda_mx5_cup", "ks_mazda_mx5_nd",
            "ks_mazda_rx7_spirit_r", "ks_mazda_rx7_tuned", "ks_mclaren_570s", "ks_mclaren_650_gt3",
            "ks_mclaren_f1_gtr", "ks_mclaren_p1", "ks_mclaren_p1_gtr", "ks_mercedes_190_evo2",
            "ks_mercedes_amg_gt3", "ks_mercedes_c9", "ks_nissan_370z", "ks_nissan_gtr",
            "ks_nissan_gtr_gt3", "ks_nissan_skyline_r34", "ks_pagani_huayra_bc",
            "ks_porsche_718_boxster_s", "ks_porsche_718_boxster_s_pdk", "ks_porsche_718_cayman_s",
            "ks_porsche_718_spyder_rs", "ks_porsche_908_lh", "ks_porsche_911_carrera_rsr",
            "ks_porsche_911_gt1", "ks_porsche_911_gt3_cup_2017", "ks_porsche_911_gt3_r_2016",
            "ks_porsche_911_gt3_rs", "ks_porsche_911_r", "ks_porsche_911_rsr_2017",
            "ks_porsche_917_30", "ks_porsche_917_k", "ks_porsche_918_spyder",
            "ks_porsche_919_hybrid_2015", "ks_porsche_919_hybrid_2016", "ks_porsche_935_78_moby_dick",
            "ks_porsche_962c_longtail", "ks_porsche_962c_shorttail", "ks_porsche_991_carrera_s",
            "ks_porsche_991_turbo_s", "ks_porsche_cayenne", "ks_porsche_cayman_gt4_clubsport",
            "ks_porsche_cayman_gt4_std", "ks_porsche_macan", "ks_porsche_panamera",
            "ks_porsche_taycan_turbo_s", "ks_praga_r1", "ks_ruf_rt12r", "ks_ruf_rt12r_awd",
            "ks_toyota_ae86", "ks_toyota_ae86_drift", "ks_toyota_ae86_tuned", "ks_toyota_celica_st185",
            "ks_toyota_gt86", "ks_toyota_gt86_drift", "ks_toyota_supra_mkiv", "ks_toyota_supra_mkiv_drift",
            "ks_toyota_supra_mkiv_tuned", "ks_toyota_ts040", "ktm_xbow_r", "lotus_2_eleven",
            "lotus_2_eleven_gt4", "lotus_49", "lotus_98t", "lotus_elise_sc", "lotus_elise_sc_s1",
            "lotus_elise_sc_s2", "lotus_evora_gtc", "lotus_evora_gte", "lotus_evora_gte_carbon",
            "lotus_evora_gx", "lotus_evora_s", "lotus_evora_s_s2", "lotus_exige_240",
            "lotus_exige_240_s3", "lotus_exige_s", "lotus_exige_s_roadster", "lotus_exige_scura",
            "lotus_exige_v6_cup", "lotus_exos_125", "lotus_exos_125_s1", "mclaren_mp412c",
            "mclaren_mp412c_gt3", "mercedes_sls", "mercedes_sls_gt3", "p4-5_2011",
            "pagani_huayra", "pagani_zonda_r", "ruf_yellowbird", "shelby_cobra_427sc", "tatuusfa1"
        };

        private static readonly HashSet<string> OfficialTrackIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "brands_hatch", "ks_brands_hatch", "drift", "imola", "ks_barcelona",
            "ks_black_cat_county", "ks_drag", "drag", "ks_highlands", "ks_laguna_seca",
            "ks_monza66", "ks_nordschleife", "ks_nurburgring", "nurburgring",
            "ks_red_bull_ring", "ks_silverstone", "silverstone", "ks_silverstone1967",
            "ks_vallelunga", "vallelunga", "ks_zandvoort", "magione", "monza",
            "mugello", "spa", "trento-bondone"
        };

        public string DetectAssettoCorsaPath()
        {
            var defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\assettocorsa";
            if (Directory.Exists(defaultPath))
            {
                return defaultPath;
            }

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                var candidate = Path.Combine(drive.RootDirectory.FullName, @"SteamLibrary\steamapps\common\assettocorsa");
                if (Directory.Exists(candidate)) return candidate;

                var candidate2 = Path.Combine(drive.RootDirectory.FullName, @"Games\assettocorsa");
                if (Directory.Exists(candidate2)) return candidate2;
            }

            return string.Empty;
        }

        public async Task<List<CarInfo>> ScanCarsAsync(string acRoot)
        {
            return await Task.Run(() =>
            {
                var cars = new List<CarInfo>();
                var carsDir = Path.Combine(acRoot, "content", "cars");
                if (!Directory.Exists(carsDir)) return cars;

                var dirs = Directory.GetDirectories(carsDir);
                foreach (var dir in dirs)
                {
                    try
                    {
                        var carId = Path.GetFileName(dir);
                        var car = new CarInfo { Id = carId, Name = carId };

                        var uiJsonPath = Path.Combine(dir, "ui", "ui_car.json");
                        if (File.Exists(uiJsonPath))
                        {
                            try
                            {
                                var json = File.ReadAllText(uiJsonPath);
                                using var doc = JsonDocument.Parse(json);
                                var root = doc.RootElement;

                                if (root.TryGetProperty("name", out var pName))
                                    car.Name = pName.GetString() ?? carId;

                                if (root.TryGetProperty("brand", out var pBrand))
                                    car.Brand = pBrand.GetString() ?? string.Empty;

                                if (root.TryGetProperty("class", out var pClass))
                                    car.Class = pClass.GetString() ?? "STREET";

                                if (root.TryGetProperty("description", out var pDesc))
                                    car.Description = pDesc.GetString() ?? string.Empty;

                                if (root.TryGetProperty("specs", out var pSpecs) && pSpecs.ValueKind == JsonValueKind.Object)
                                {
                                    if (pSpecs.TryGetProperty("bhp", out var pBhp)) car.Bhp = pBhp.GetString() ?? "--";
                                    if (pSpecs.TryGetProperty("torque", out var pTorque)) car.Torque = pTorque.GetString() ?? "--";
                                    if (pSpecs.TryGetProperty("weight", out var pWeight)) car.Weight = pWeight.GetString() ?? "--";
                                    if (pSpecs.TryGetProperty("topspeed", out var pTop)) car.TopSpeed = pTop.GetString() ?? "--";
                                    if (pSpecs.TryGetProperty("acceleration", out var pAcc)) car.Acceleration = pAcc.GetString() ?? "--";
                                    if (pSpecs.TryGetProperty("pwratio", out var pPw)) car.PowerToWeight = pPw.GetString() ?? "--";
                                }

                                if (root.TryGetProperty("author", out var pAuthor))
                                    car.Author = pAuthor.GetString() ?? string.Empty;

                                if (root.TryGetProperty("tags", out var pTags) && pTags.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var tagEl in pTags.EnumerateArray())
                                    {
                                        var t = tagEl.GetString();
                                        if (!string.IsNullOrWhiteSpace(t)) car.Tags.Add(t.Trim());
                                    }
                                }
                            }
                            catch { }
                        }

                        // Determine if car is Kunos Official or Mod
                        bool isKunosCar = OfficialCarIds.Contains(carId);
                        if (!isKunosCar && carId.StartsWith("ks_", StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrWhiteSpace(car.Author) || car.Author.Contains("Kunos", StringComparison.OrdinalIgnoreCase))
                            {
                                isKunosCar = true;
                            }
                        }
                        car.IsMod = !isKunosCar;

                        var badgePath = Path.Combine(dir, "ui", "badge.png");
                        if (File.Exists(badgePath)) car.BadgeImagePath = badgePath;

                        var skinsDir = Path.Combine(dir, "skins");
                        if (Directory.Exists(skinsDir))
                        {
                            var skinFolders = Directory.GetDirectories(skinsDir);
                            foreach (var sf in skinFolders)
                            {
                                var sName = Path.GetFileName(sf);
                                car.Skins.Add(sName);

                                var skinInfo = new CarSkinInfo { Name = sName };
                                var previewFile = Path.Combine(sf, "preview.jpg");
                                if (File.Exists(previewFile))
                                {
                                    skinInfo.PreviewImagePath = previewFile;
                                    if (string.IsNullOrEmpty(car.PreviewImagePath))
                                    {
                                        car.PreviewImagePath = previewFile;
                                        car.SelectedSkin = sName;
                                    }
                                }

                                var liveryFile = Path.Combine(sf, "livery.png");
                                if (File.Exists(liveryFile)) skinInfo.LiveryImagePath = liveryFile;

                                car.SkinList.Add(skinInfo);
                            }

                            if (string.IsNullOrEmpty(car.SelectedSkin) && car.Skins.Count > 0)
                            {
                                car.SelectedSkin = car.Skins[0];
                            }
                        }

                        cars.Add(car);
                    }
                    catch { }
                }

                return cars.OrderBy(c => c.Brand).ThenBy(c => c.Name).ToList();
            });
        }

        public async Task<List<TrackInfo>> ScanTracksAsync(string acRoot)
        {
            return await Task.Run(() =>
            {
                var tracks = new List<TrackInfo>();
                var tracksDir = Path.Combine(acRoot, "content", "tracks");
                if (!Directory.Exists(tracksDir)) return tracks;

                var dirs = Directory.GetDirectories(tracksDir);
                foreach (var dir in dirs)
                {
                    try
                    {
                        var trackId = Path.GetFileName(dir);
                        var trackUiDir = Path.Combine(dir, "ui");
                        var uiJsonPath = Path.Combine(trackUiDir, "ui_track.json");

                        if (File.Exists(uiJsonPath))
                        {
                            var track = ParseTrackJson(uiJsonPath, trackId, string.Empty);
                            PopulateTrackMedia(track, trackUiDir);
                            tracks.Add(track);
                        }
                        else if (Directory.Exists(trackUiDir))
                        {
                            var subLayoutDirs = Directory.GetDirectories(trackUiDir);
                            foreach (var sld in subLayoutDirs)
                            {
                                var subConfig = Path.GetFileName(sld);
                                var subJson = Path.Combine(sld, "ui_track.json");
                                if (File.Exists(subJson))
                                {
                                    var track = ParseTrackJson(subJson, trackId, subConfig);
                                    PopulateTrackMedia(track, sld);
                                    tracks.Add(track);
                                }
                            }
                        }
                    }
                    catch { }
                }

                return tracks.OrderBy(t => t.Country).ThenBy(t => t.Name).ToList();
            });
        }

        private TrackInfo ParseTrackJson(string jsonPath, string trackId, string subConfig)
        {
            var track = new TrackInfo
            {
                Id = trackId,
                SubConfig = subConfig,
                Name = string.IsNullOrEmpty(subConfig) ? trackId : $"{trackId} ({subConfig})"
            };

            try
            {
                var json = File.ReadAllText(jsonPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("name", out var pName))
                    track.Name = pName.GetString() ?? track.Name;

                if (root.TryGetProperty("country", out var pCountry))
                    track.Country = pCountry.GetString() ?? string.Empty;

                if (root.TryGetProperty("city", out var pCity))
                    track.City = pCity.GetString() ?? string.Empty;

                if (root.TryGetProperty("length", out var pLen))
                    track.Length = pLen.GetString() ?? "--";

                if (root.TryGetProperty("width", out var pWidth))
                    track.Width = pWidth.GetString() ?? "--";

                if (root.TryGetProperty("pitboxes", out var pPit))
                    track.Pitboxes = pPit.GetString() ?? "--";

                if (root.TryGetProperty("run", out var pRun))
                    track.Run = pRun.GetString() ?? string.Empty;

                if (root.TryGetProperty("author", out var pAuthor))
                    track.Author = pAuthor.GetString() ?? string.Empty;

                if (root.TryGetProperty("description", out var pDesc))
                    track.Description = pDesc.GetString() ?? string.Empty;
            }
            catch { }

            // Determine if track is Kunos Official or Mod
            bool isKunosTrack = OfficialTrackIds.Contains(trackId);
            if (!isKunosTrack && trackId.StartsWith("ks_", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(track.Author) || track.Author.Contains("Kunos", StringComparison.OrdinalIgnoreCase))
                {
                    isKunosTrack = true;
                }
            }
            track.IsMod = !isKunosTrack;

            return track;
        }

        private void PopulateTrackMedia(TrackInfo track, string uiDir)
        {
            var preview = Path.Combine(uiDir, "preview.png");
            if (File.Exists(preview)) track.PreviewImagePath = preview;

            var outline = Path.Combine(uiDir, "outline.png");
            if (File.Exists(outline)) track.OutlineImagePath = outline;
        }
    }
}
