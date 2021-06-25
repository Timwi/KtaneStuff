using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Json;
using RT.Serialization;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Geometry;

namespace KtaneStuff
{
    internal class Missions
    {
        sealed class MissionInfo
        {
            public string Title;
            public string Category;
            public string Pid;
            public string Cid;
            public string UrlTag;

            public string[] ModuleIDs;

            public MissionInfo(string pid, string cid, string title, string category, string urltag)
            {
                Pid = pid;
                Cid = cid;
                Title = title;
                Category = category;
                UrlTag = urltag;
            }

            private MissionInfo() { }   // for Classify
        }

        public static readonly string _missionsJsonFile = @"D:\temp\KTANE\Missions\Missions.json";

        public static void Download()
        {
            var redownloadMainSheets = false;

            var spreadsheets = Ut.NewArray(
                (pid: "1yQDBEpu0dO7-CFllakfURm4NGGdQl6tN-39m6O0Q_Ow", skipSheets: 2, category: "solved"),
                (pid: "1k2LlhY-BBJQImEHo_S51L_okPiOee6xgdk5mkVwn2ZU", skipSheets: 1, category: "unsolved"),
                (pid: "1pzoatn2mX1gtKurxt1OBejbutTrKq0kqO9dNohnu33Q", skipSheets: 1, category: "tp"));

            var missions = redownloadMainSheets ? spreadsheets.ParallelSelectMany(spreadsheet => new HClient().Get($"https://spreadsheets.google.com/feeds/worksheets/{spreadsheet.pid}/public/full?alt=json").DataJson["feed"]["entry"].GetList().Skip(spreadsheet.skipSheets).Select(obj =>
            {
                string urltag = null;
                Match m;
                foreach (var lnk in obj["link"].GetList())
                    if ((m = Regex.Match(lnk["href"].GetString(), @"\?gid=(\d+)&")).Success)
                        urltag = m.Groups[1].Value;
                var id = obj["id"]["$t"].GetString();
                return new MissionInfo(spreadsheet.pid, cid: id.Substring(id.LastIndexOf('/') + 1), title: obj["title"]["$t"].GetString(), spreadsheet.category, urltag);
            })).ToArray() : ClassifyJson.DeserializeFile<MissionInfo[]>(_missionsJsonFile);

            if (redownloadMainSheets)
                ClassifyJson.SerializeToFile(missions, _missionsJsonFile);

            missions.ParallelForEach(8, mission =>
            {
                if (mission.ModuleIDs != null)
                    return;
                Console.WriteLine($"Downloading {mission.Title}");
                var result = new HClient().Get($"https://spreadsheets.google.com/feeds/cells/{mission.Pid}/{mission.Cid}/public/full?alt=json").DataJson;
                var moduleIds = new HashSet<string>();
                Match m;
                foreach (var obj in result["feed"]["entry"].GetList())
                    if ((obj["gs$cell"]["col"].GetIntLenient()) == 12 && (obj["gs$cell"]["row"].GetIntLenient()) >= 3 && (m = Regex.Match(obj["content"]["$t"].GetString(), @"^\[(.*)\] Count: \d+$", RegexOptions.Singleline)).Success)
                        foreach (var modId in m.Groups[1].Value.Split(','))
                            moduleIds.Add(modId.Trim());
                mission.ModuleIDs = moduleIds.Order().ToArray();
            });

            ClassifyJson.SerializeToFile(missions, _missionsJsonFile);
        }

        public static void FindMission()
        {
            var data = ClassifyJson.DeserializeFile<MissionInfo[]>(_missionsJsonFile);
            var moduleIds = JsonDict.Parse(File.ReadAllText(@"C:\Users\Timwi\AppData\LocalLow\Steel Crate Games\Keep Talking and Nobody Explodes\ModProfiles\Megum.json"))["EnabledList"].GetList().Select(v => v.GetString()).ToArray();
            foreach (var mission in data.OrderBy(m => m.Title))
                if (mission.ModuleIDs.All(m => moduleIds.Contains(m)))
                    ConsoleUtil.WriteLine($"{mission.Title} (<https://docs.google.com/spreadsheets/d/{mission.Pid}/edit#gid={mission.UrlTag}>)".Color(ConsoleColor.Green));
                else
                    ConsoleUtil.WriteLine($"{mission.Title} contains {mission.ModuleIDs.First(m => !moduleIds.Contains(m))}".Color(ConsoleColor.Red));
        }
    }
}