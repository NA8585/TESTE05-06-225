// SessionYamlParser.cs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using YamlDotNet.RepresentationModel;
using SuperBackendNR85IA.Models; // Assuming SectorInfo will be or is part of Models

namespace SuperBackendNR85IA.Services
{
    public class SessionYamlParser
    {
        private readonly ILogger<SessionYamlParser> _logger;
        private readonly HashSet<string> _loggedMissingKeys = new();
        private readonly Dictionary<long, (string yaml, (DriverInfo?, WeekendInfo?, SessionInfo?, SectorInfo?, List<DriverInfo>) data)> _cache = new();

        public SessionYamlParser(ILogger<SessionYamlParser> logger)
        {
            _logger = logger;
        }

        public (DriverInfo?, WeekendInfo?, SessionInfo?, SectorInfo?, List<DriverInfo>) ParseSessionInfo(string yaml, int playerCarIdx, int currentSessionNum, long sessionUniqueId)
        {
            _loggedMissingKeys.Clear();

            if (string.IsNullOrWhiteSpace(yaml))
                return (null, null, null, null, new List<DriverInfo>());

            if (_cache.TryGetValue(sessionUniqueId, out var entry) && entry.yaml == yaml)
                return entry.data;

            var result = ParseSessionInfoInternal(yaml, playerCarIdx, currentSessionNum);
            _cache[sessionUniqueId] = (yaml, result);
            return result;
        }

        private (DriverInfo?, WeekendInfo?, SessionInfo?, SectorInfo?, List<DriverInfo>) ParseSessionInfoInternal(string yaml, int playerCarIdx, int currentSessionNum)
        {
            var ys = new YamlStream();
            ys.Load(new StringReader(yaml));
            var root = (YamlMappingNode)ys.Documents[0].RootNode;

            var driver = ParsePlayerDriverInfo(root, playerCarIdx);
            var weekend = ParseWeekendInfo(root);
            var session = ParseCurrentSessionDetails(root, currentSessionNum);
            var sectors = ParseSectorInfo(root, currentSessionNum);
            var drivers = ParseAllDrivers(root);

            return (driver, weekend, session, sectors, drivers);
        }

        private DriverInfo? ParsePlayerDriverInfo(YamlMappingNode root, int idx)
        {
            if (!root.Children.ContainsKey(new YamlScalarNode("DriverInfo"))) return null;
            var driverNode = (YamlMappingNode)root.Children[new YamlScalarNode("DriverInfo")];
            if (!driverNode.Children.ContainsKey(new YamlScalarNode("Drivers")) || !(driverNode.Children[new YamlScalarNode("Drivers")] is YamlSequenceNode seq)) return null;
            
            if (idx < 0 || idx >= seq.Children.Count) return null;

            var node = (YamlMappingNode)seq.Children[idx];
            return new DriverInfo
            {
                CarIdx            = GetInt(node, "CarIdx"),
                UserName          = GetStr(node, "UserName"),
                TeamName          = GetStr(node, "TeamName"),
                UserID            = GetInt(node, "UserID"),
                TeamID            = GetInt(node, "TeamID"),
                CarNumber         = GetStr(node, "CarNumberRaw"),
                IRating           = GetInt(node, "IRating"),
                LicString         = GetStr(node, "LicString"),
                LicLevel          = GetInt(node, "LicLevel"),
                LicSubLevel       = GetInt(node, "LicSubLevel"),
                CarPath           = GetStr(node, "CarPath"),
                CarClassID        = GetInt(node, "CarClassID"),
                CarClassShortName = GetStr(node, "CarClassShortName"),
                CarClassRelSpeed  = GetFloat(node, "CarClassRelSpeed"),
                CarClassEstLapTime = GetFloat(node, "CarClassEstLapTime"),
                TireCompound      = GetTireCompound(node),
                TeamIncidentCount = GetInt(node, "TeamIncidentCount")
            };
        }

        private List<DriverInfo> ParseAllDrivers(YamlMappingNode root)
        {
            var list = new List<DriverInfo>();
            if (!root.Children.ContainsKey(new YamlScalarNode("DriverInfo"))) return list;
            var driverNode = (YamlMappingNode)root.Children[new YamlScalarNode("DriverInfo")];
            if (!driverNode.Children.ContainsKey(new YamlScalarNode("Drivers")) || !(driverNode.Children[new YamlScalarNode("Drivers")] is YamlSequenceNode seq)) return list;

            foreach (var child in seq.Children.OfType<YamlMappingNode>())
            {
                list.Add(new DriverInfo
                {
                    CarIdx            = GetInt(child, "CarIdx"),
                    UserName          = GetStr(child, "UserName"),
                    TeamName          = GetStr(child, "TeamName"),
                    UserID            = GetInt(child, "UserID"),
                    TeamID            = GetInt(child, "TeamID"),
                    CarNumber         = GetStr(child, "CarNumberRaw"),
                    IRating           = GetInt(child, "IRating"),
                    LicString         = GetStr(child, "LicString"),
                    LicLevel          = GetInt(child, "LicLevel"),
                    LicSubLevel       = GetInt(child, "LicSubLevel"),
                    CarPath           = GetStr(child, "CarPath"),
                    CarClassID        = GetInt(child, "CarClassID"),
                    CarClassShortName = GetStr(child, "CarClassShortName"),
                    CarClassRelSpeed  = GetFloat(child, "CarClassRelSpeed"),
                    CarClassEstLapTime = GetFloat(child, "CarClassEstLapTime"),
                    TireCompound      = GetTireCompound(child),
                    TeamIncidentCount = GetInt(child, "TeamIncidentCount")
                });
            }

            return list;
        }

        private WeekendInfo? ParseWeekendInfo(YamlMappingNode root)
        {
            if (!root.Children.ContainsKey(new YamlScalarNode("WeekendInfo"))) return null;
            var wNode = (YamlMappingNode)root.Children[new YamlScalarNode("WeekendInfo")];
            
            float trackLengthKm = 0;
            string trackLengthStr = GetStr(wNode, "TrackLength"); // Ex: "5.89 km"
            if (!string.IsNullOrEmpty(trackLengthStr))
            {
                trackLengthStr = trackLengthStr.Replace(" km", "").Replace(" mi", "").Trim();
                float.TryParse(trackLengthStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out trackLengthKm);
            }

            return new WeekendInfo
            {
                TrackName        = GetStr(wNode, "TrackName"),
                TrackDisplayName = GetStr(wNode, "TrackDisplayName"),
                TrackLengthKm    = trackLengthKm,
                TrackConfigName  = GetStr(wNode, "TrackConfigName"),
                SessionType      = GetStr(wNode, "SessionType"),
                Skies            = GetStr(wNode, "Skies"),
                WindSpeed        = GetFloatFromSpecialFormat(wNode, "WindSpeed"), // Ex: "12 kph"
                WindDir          = GetFloatFromSpecialFormat(wNode, "WindDir"),
                AirPressure      = GetFloatFromSpecialFormat(wNode, "AirPressure"), // Ex: "29.00 Hg" ou "101.2 kPa"
                RelativeHumidity = GetFloatFromSpecialFormat(wNode, "RelativeHumidity"), // Ex: "50.0 %"
                ChanceOfRain     = GetFloatFromSpecialFormat(wNode, "ChanceOfRain"),     // Ex: "0.0 %"
                ForecastType     = GetStr(wNode, "ForecastType"),
                TrackWindVel     = GetFloatFromSpecialFormat(wNode, "WindVel"),
                TrackAirTemp     = GetFloatFromSpecialFormat(wNode, "AirTemp"),
                TrackNumTurns    = GetStr(wNode, "TrackNumTurns"),
                NumCarClasses    = GetInt(wNode, "NumCarClasses")
            };
        }

        private SessionInfo? ParseCurrentSessionDetails(YamlMappingNode root, int curNum)
        {
            if (!root.Children.ContainsKey(new YamlScalarNode("SessionInfo"))) return null;
            var sNode = (YamlMappingNode)root.Children[new YamlScalarNode("SessionInfo")];
            if (!sNode.Children.ContainsKey(new YamlScalarNode("Sessions")) || !(sNode.Children[new YamlScalarNode("Sessions")] is YamlSequenceNode seq)) return null;

            var list = seq.Children
                          .OfType<YamlMappingNode>()
                          .Select(node =>
                          {
                              var sd = new SessionDetailFromYaml
                              {
                                  SessionNum = GetInt(node, "SessionNum"),
                                  SessionName = GetStr(node, "SessionName"),
                                  SessionType = GetStr(node, "SessionType"),
                                  SessionLaps = GetInt(node, "SessionLaps") // Parse SessionLaps aqui
                              };
                              if (node.Children.TryGetValue(new YamlScalarNode("ResultsPenalty"), out var rpNode) && rpNode is YamlMappingNode rpMap)
                              {
                                  sd.IncidentLimit = GetInt(rpMap, "IncidentLimit");
                              }
                              if (node.Children.TryGetValue(new YamlScalarNode("ResultsPositions"), out var rp) && rp is YamlSequenceNode rpSeq)
                              {
                                  sd.ResultsPositions = rpSeq.Children.OfType<YamlMappingNode>()
                                      .Select(p => new ResultPosition
                                      {
                                          Position = GetInt(p, "Position"),
                                      CarIdx = GetInt(p, "CarIdx"),
                                      FastestTime = GetFloat(p, "FastestTime"),
                                      LastTime = GetFloat(p, "LastTime"),
                                      Time = GetFloat(p, "Time"),
                                      Interval = GetFloat(p, "Interval"),
                                      OnPitRoad = GetBool(p, "OnPitRoad"),
                                      InGarage = GetBool(p, "InGarage"),
                                      PitStopCount = GetInt(p, "PitStopCount"),
                                      NewIRating = GetInt(p, "NewIRating")
                                      }).ToList();
                              }
                              return sd;
                          })
                          .ToList();

            var current = list.FirstOrDefault(x => x.SessionNum == curNum);
            
            int incidentLimitFromCurrentSession = current?.IncidentLimit ?? 0;
            // Fallback para IncidentLimit do WeekendOptions se não encontrado na sessão específica
            if (incidentLimitFromCurrentSession == 0 && root.Children.TryGetValue(new YamlScalarNode("WeekendInfo"), out var wiNode) && wiNode is YamlMappingNode wim)
            {
                 if (wim.Children.TryGetValue(new YamlScalarNode("WeekendOptions"), out var woNode) && woNode is YamlMappingNode wom)
                 {
                     incidentLimitFromCurrentSession = GetInt(wom, "IncidentLimit");
                 }
            }

            return new SessionInfo
            {
                SessionNum              = curNum,
                SessionName             = current?.SessionName,
                SessionType             = current?.SessionType,
                NumTrackSessions        = seq.Children.Count,
                AllSessionsFromYaml     = list,
                IncidentLimit           = incidentLimitFromCurrentSession,
                CurrentSessionTotalLaps = current?.SessionLaps ?? 0 // Popula com SessionLaps da sessão atual
            };
        }

        private SectorInfo? ParseSectorInfo(YamlMappingNode root, int sessionNum)
        {
            // Use TryGetValue with YamlScalarNode for keys, consistent with other parsing methods
            if (!root.Children.TryGetValue(new YamlScalarNode("SessionInfo"), out var sessionInfoNode) || !(sessionInfoNode is YamlMappingNode sessionNode)) return null;
            
            if (!sessionNode.Children.TryGetValue(new YamlScalarNode("Sessions"), out var sessionsSequenceNode) || !(sessionsSequenceNode is YamlSequenceNode sessionsSeq)) return null;
            
            var sessionData = sessionsSeq.Children
                .OfType<YamlMappingNode>()
                .FirstOrDefault(s => GetInt(s, "SessionNum") == sessionNum);

            if (sessionData == null) return null;

            return new SectorInfo
            {
                SectorCount    = GetInt(sessionData, "SectorCount"),
                SectorTimes    = GetFloatArray(sessionData, "SectorTimes"),
                BestSectorTimes = GetFloatArray(sessionData, "BestSectorTimes")
            };
        }

        // Helpers
        private string GetStr(YamlMappingNode n, string key)
        {
            if (n.Children.TryGetValue(new YamlScalarNode(key), out var v) && v is YamlScalarNode s)
            {
                return s.Value ?? string.Empty;
            }

            LogMissingKey(key);
            return string.Empty;
        }

        private int GetInt(YamlMappingNode n, string key)
        {
            if (n.Children.TryGetValue(new YamlScalarNode(key), out var v) && v is YamlScalarNode s)
            {
                if (string.Equals(s.Value, "unlimited", StringComparison.OrdinalIgnoreCase))
                    return 0;
                if (int.TryParse(s.Value, out var r))
                    return r;

                _logger.LogDebug("Unable to parse integer from '{Value}' for key '{Key}'.", s.Value, key);
                return 0;
            }

            LogMissingKey(key);
            return 0;
        }

        private float GetFloat(YamlMappingNode n, string key)
        {
            if (n.Children.TryGetValue(new YamlScalarNode(key), out var v) && v is YamlScalarNode s)
            {
                if (float.TryParse(s.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var r))
                    return r;

                _logger.LogDebug("Unable to parse float from '{Value}' for key '{Key}'.", s.Value, key);
                return 0f;
            }

            LogMissingKey(key);
            return 0f;
        }

        private float GetFloatFromSpecialFormat(YamlMappingNode n, string key)
        {
            string rawValue = GetStr(n, key); // GetStr logs missing key
            if (!string.IsNullOrEmpty(rawValue))
            {
                var match = Regex.Match(rawValue, @"[-+]?[0-9]*\.?[0-9]+");
                if (match.Success && float.TryParse(match.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result))
                {
                    return result;
                }

                _logger.LogDebug("Unable to parse float from '{Value}' for key '{Key}'.", rawValue, key);
            }

            return 0f;
        }

        private bool GetBool(YamlMappingNode n, string key)
        {
            if (n.Children.TryGetValue(new YamlScalarNode(key), out var v) && v is YamlScalarNode s)
            {
                if (bool.TryParse(s.Value, out var b))
                    return b;
                if (int.TryParse(s.Value, out var i))
                    return i != 0;

                _logger.LogDebug("Unable to parse boolean from '{Value}' for key '{Key}'.", s.Value, key);
                return false;
            }

            LogMissingKey(key);
            return false;
        }

        private string GetTireCompound(YamlMappingNode driverNode)
        {
            if (driverNode.Children.TryGetValue(new YamlScalarNode("CarSetup"), out var csNode) && csNode is YamlMappingNode csMap &&
                csMap.Children.TryGetValue(new YamlScalarNode("Tires"), out var tireNode) && tireNode is YamlMappingNode tireMap)
            {
                return GetStr(tireMap, "CompoundName");
            }
            return string.Empty;
        }

        // New helper method to parse an array of floats
        private float[] GetFloatArray(YamlMappingNode n, string key)
        {
            if (n.Children.TryGetValue(new YamlScalarNode(key), out var v) && v is YamlSequenceNode seq)
            {
                var floatList = new List<float>();
                foreach (var node in seq.Children)
                {
                    if (node is YamlScalarNode s && float.TryParse(s.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var r))
                    {
                        floatList.Add(r);
                    }
                    else
                    {
                        _logger.LogDebug("Unable to parse float array element '{Value}' for key '{Key}'.", (node as YamlScalarNode)?.Value, key);
                        floatList.Add(0f);
                    }
                }
                return floatList.ToArray();
            }

            LogMissingKey(key, "Expected key '{Key}' was not found or not a sequence.");
            return Array.Empty<float>();
        }

        private void LogMissingKey(string key, string? message = null)
        {
            if (_loggedMissingKeys.Add(key))
            {
                _logger.LogDebug(message ?? "Expected key '{Key}' was not found.", key);
            }
        }
    }
}
