using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using SAM.Game.Stats;
using KW_Steam_Achievements;
using APITypes = SAM.API.Types;

namespace HeadlessGame
{
    class Program
    {

        static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                {
                    return name;
                }
            }

            name = kv.AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            return defaultValue;
        }

        static bool LoadUserGameStatsSchema(SAM.API.Client client, ulong gameId, List<AchievementDefinition> achievementDefinitions, List<StatDefinition> statDefinitions)
        {
            string path;

            try
            {
                path = SAM.API.Steam.GetInstallPath();
                path = Path.Combine(path, "appcache");
                path = Path.Combine(path, "stats");
                path = Path.Combine(path, string.Format(
                    CultureInfo.InvariantCulture,
                    "UserGameStatsSchema_{0}.bin",
                    gameId));

                if (File.Exists(path) == false)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            var kv = KeyValue.LoadAsBinary(path);

            if (kv == null)
            {
                return false;
            }

            var currentLanguage = client.SteamApps008.GetCurrentGameLanguage();
            //var currentLanguage = "german";

            achievementDefinitions.Clear();
            statDefinitions.Clear();

            var stats = kv[gameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false ||
                stats.Children == null)
            {
                return false;
            }

            foreach (var stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

                var rawType = stat["type_int"].Valid
                                  ? stat["type_int"].AsInteger(0)
                                  : stat["type"].AsInteger(0);
                var type = (APITypes.UserStatType)rawType;
                switch (type)
                {
                    case APITypes.UserStatType.Invalid:
                        {
                            break;
                        }

                    case APITypes.UserStatType.Integer:
                        {
                            var id = stat["name"].AsString("");
                            string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            statDefinitions.Add(new IntegerStatDefinition()
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsInteger(int.MinValue),
                                MaxValue = stat["max"].AsInteger(int.MaxValue),
                                MaxChange = stat["maxchange"].AsInteger(0),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsInteger(0),
                                Permission = stat["permission"].AsInteger(0),
                            });
                            break;
                        }

                    case APITypes.UserStatType.Float:
                    case APITypes.UserStatType.AverageRate:
                        {
                            var id = stat["name"].AsString("");
                            string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            statDefinitions.Add(new FloatStatDefinition()
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsFloat(float.MinValue),
                                MaxValue = stat["max"].AsFloat(float.MaxValue),
                                MaxChange = stat["maxchange"].AsFloat(0.0f),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsFloat(0.0f),
                                Permission = stat["permission"].AsInteger(0),
                            });
                            break;
                        }

                    case APITypes.UserStatType.Achievements:
                    case APITypes.UserStatType.GroupAchievements:
                        {
                            if (stat.Children != null)
                            {
                                foreach (var bits in stat.Children.Where(
                                    b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                                {
                                    if (bits.Valid == false ||
                                        bits.Children == null)
                                    {
                                        continue;
                                    }

                                    foreach (var bit in bits.Children)
                                    {
                                        string id = bit["name"].AsString("");
                                        string name = GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                        string desc = GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                        achievementDefinitions.Add(new AchievementDefinition()
                                        {
                                            Id = id,
                                            Name = name,
                                            Description = desc,
                                            IconNormal = bit["display"]["icon"].AsString(""),
                                            IconLocked = bit["display"]["icon_gray"].AsString(""),
                                            IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                            Permission = bit["permission"].AsInteger(0),
                                        });
                                    }
                                }
                            }

                            break;
                        }

                    default:
                        {
                            throw new InvalidOperationException("invalid stat type");
                        }
                }
            }

            return true;
        }

        static void Main(string[] args)
        {
            long gameId = long.Parse(args[0]);
            using (var client = new SAM.API.Client())
            {
                client.Initialize(gameId);

                string name = client.SteamApps001.GetAppData((uint)gameId, "name");
                Console.Title = name;

                if(client.SteamUserStats.RequestCurrentStats() == false)
                {
                    Console.WriteLine("failed getting current stats");
                }

                //not used
                List<StatDefinition> statDefinitions = new List<StatDefinition>();

                //where all the achievements are
                List<AchievementDefinition> achievementDefinitions = new List<AchievementDefinition>();

                //fill the arrays
                if (LoadUserGameStatsSchema(client, (ulong)gameId, achievementDefinitions, statDefinitions) == false)
                {
                    //failed, do smthn i dunno
                    return;
                }

                //now loop over all the achievements and get them all
                foreach (AchievementDefinition info in achievementDefinitions)
                {
                    bool isAchieved;
                    if (client.SteamUserStats.GetAchievementState(info.Id, out isAchieved) == false)
                    {
                        Console.WriteLine("skipping: " + info.Id);
                        continue;
                    }

                    if (client.SteamUserStats.SetAchievement(info.Id, true) == false) //set em all to true babyyyyyy
                    {
                        Console.WriteLine("failed: " + info.Id);
                    }
                    if (client.SteamUserStats.StoreStats() == false)
                    {
                        Console.WriteLine("storing failed");
                    }
                }

            }
            //Console.ReadLine();
        }
    }
}
