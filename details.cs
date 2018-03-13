using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace DetailChecker
{
    public abstract class PostInfo
    {
        public string SubLinking { get; set; }
        public string SubLinked { get; set; }
    }
    public class CrossPostLineInfo : PostInfo
    {
        public string SourcePost { get; set; }
        public string LinkedPost { get; set; }
        public CrossPostLineInfo(string line)
        {
            string[] linesections = line.Split(' ', '\t');
            SubLinking = linesections[0];
            SubLinked = linesections[1];
            SourcePost = linesections[2].Split('T')[0].Trim();
            LinkedPost = linesections[6].Split('T')[0].Trim();
        }
        public CrossPostLineInfo() { }
        public static explicit operator CrossPostLineInfo(string source)
        {
            return new CrossPostLineInfo
            {
                SourcePost = source
            };
        }
    }
    public class LabelInfo
    {
        public string SourcePost { get; set; }
        public string LinkedPost { get; set; }
        public bool Burst { get; private set; }
        public LabelInfo(string line)
        {
            string[] sections = line.Split('\t');
            Burst = sections[1] == "burst";
            var matches = new Regex("('[^']+')").Matches(sections[0]);
            int iterator = 0;
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                if (iterator == 0)
                    SourcePost = groups[iterator].Value.Trim('\'');
                else
                    LinkedPost = groups[iterator].Value.Trim('\'');
                iterator++;
            }
        }
        public override string ToString()
        {
            return $"{SourcePost}, {LinkedPost}, {Burst}";
        }
        private LabelInfo() { }
        public static explicit operator LabelInfo(string source)
        {
            return new LabelInfo
            {
                SourcePost = source
            };
        }
    }
    public static class Loader
    {
        public static List<CrossPostLineInfo> GetCrossLinksInfos(string file)
        {
            List<CrossPostLineInfo> infos = new List<CrossPostLineInfo>();
            foreach (var line in File.ReadAllLines(file))
            {
                infos.Add(new CrossPostLineInfo(line));
            }
            return infos;
        }
        public static List<LabelInfo> GetLabelInfos(string file)
        {
            var infos = new List<LabelInfo>();
            foreach (var line in File.ReadAllLines(file))
            {
                infos.Add(new LabelInfo(line));
            }
            return infos;
        }
    }
    public class Details
    {
        static void breakPoint() { }
        public static void Main()
        {
            List<LabelInfo> labelInfos = Loader.GetLabelInfos("./label_info.tsv");
            List<CrossPostLineInfo> crossPostInfos = Loader.GetCrossLinksInfos("./post_crosslinks_info.tsv");
            var common = crossPostInfos.GroupBy(s => s.SubLinking).Select(g => new { Linking = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).Where(g => g.Count >= 200);
            /*foreach(var value in common) {
                Console.WriteLine($"Sub {value.Linking} linked {value.Count}");
            }*/
            var burstLabels = labelInfos.Where(a => a.Burst).ToArray();
            var crossPostSourcePosts = crossPostInfos.Select(a => a.SourcePost).ToArray();
            List<CrossPostLineInfo> labeledCrossposts = new List<CrossPostLineInfo>();
            foreach (var label in burstLabels)
            {
                if (crossPostSourcePosts.Contains(label.SourcePost))
                {
                    labeledCrossposts.Add(crossPostInfos.Find(a => a.SourcePost == label.SourcePost));
                }
            }
            //var burstCrossposts = crossPostInfos.FindAll(a => burstLabels.Contains((LabelInfo)a.SourcePost, faster2));
            var inCommon = labeledCrossposts.GroupBy(s => s.SubLinking).Select(g => new { Linking = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).Where(g => g.Count >= 20);
            foreach (var temp in inCommon)
            {
                Console.WriteLine($"burst crossposts source {temp.Linking}, count: {temp.Count}");
            }
        }
    }
}