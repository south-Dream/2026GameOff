using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
namespace PlaneShift
{
    /// <summary>
    /// 将 Excel 语言表导入运行时容器
    /// </summary>
    public class LanguageImporter : AssetPostprocessor
    {
        public const string Source = "Assets/Resources/Config/Language/Language.xlsx";
        public const string Target = "Assets/Resources/Config/Language/LanguageData.asset";
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] old)
        {
            if (imported.Contains(Source)) Import();
        }
        [MenuItem("PlaneShift/Import Languages")]
        public static void Import()
        {
            var rows = Read(Source);
            if (rows.Count < 2 || rows[0].Length < 2 || rows[0][0] != "Id") throw new InvalidDataException("Language header must start with Id");
            var codes = rows[0].Skip(1).ToArray();
            if (codes.Distinct().Count() != codes.Length || codes.Any(string.IsNullOrWhiteSpace)) throw new InvalidDataException("Duplicate or empty language code");
            var ids = new HashSet<string>();
            var entries = new List<LanguageData.Entry>();
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Length != codes.Length + 1 || row.Any(string.IsNullOrWhiteSpace) || !ids.Add(row[0])) throw new InvalidDataException("Incomplete or duplicate language row " + (i + 1));
                var placeholders = row.Skip(1).Select(Placeholders).ToArray();
                if (placeholders.Skip(1).Any(x => x != placeholders[0])) throw new InvalidDataException("Mismatched format arguments at row " + (i + 1) + ": " + row[0]);
                entries.Add(new LanguageData.Entry { id = row[0], values = row.Skip(1).ToArray() });
            }
            var asset = AssetDatabase.LoadAssetAtPath<LanguageData>(Target);
            if (asset == null) { asset = ScriptableObject.CreateInstance<LanguageData>(); AssetDatabase.CreateAsset(asset, Target); }
            asset.languages = codes; asset.entries = entries.ToArray();
            EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets();
        }
        private static string Placeholders(string value)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(value, @"(?<!\{)\{(\d+)(?:,[^{}:]+)?(?::[^{}]+)?\}(?!\})");
            var indices = matches.Cast<System.Text.RegularExpressions.Match>().Select(x => int.Parse(x.Groups[1].Value)).Distinct().OrderBy(x => x).ToArray();
            int count = indices.Length == 0 ? 0 : indices.Max() + 1;
            try { string.Format(value, Enumerable.Repeat<object>(0, count).ToArray()); }
            catch (FormatException error) { throw new InvalidDataException("Invalid language format: " + value, error); }
            return string.Join(",", indices);
        }
        public static List<string[]> Read(string path)
        {
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            using (var zip = ZipFile.OpenRead(path))
            {
                var shared = zip.GetEntry("xl/sharedStrings.xml");
                string[] strings = shared == null ? new string[0] : XDocument.Load(shared.Open()).Descendants(ns + "si").Select(x => string.Concat(x.Descendants(ns + "t").Select(t => t.Value))).ToArray();
                return XDocument.Load(zip.GetEntry("xl/worksheets/sheet1.xml").Open()).Descendants(ns + "row").Select(row =>
                {
                    var cells = new SortedDictionary<int, string>();
                    foreach (var c in row.Elements(ns + "c"))
                    {
                        string reference = (string)c.Attribute("r");
                        int column = 0;
                        foreach (char letter in reference.TakeWhile(char.IsLetter)) column = column * 26 + letter - 'A' + 1;
                        cells.Add(column - 1, (string)c.Attribute("t") == "s" ? strings[int.Parse(c.Element(ns + "v").Value)] : (string)c.Attribute("t") == "inlineStr" ? string.Concat(c.Descendants(ns + "t").Select(t => t.Value)) : c.Element(ns + "v")?.Value ?? "");
                    }
                    if (cells.Count == 0) return new string[0];
                    var values = new string[cells.Keys.Max() + 1];
                    foreach (var cell in cells) values[cell.Key] = cell.Value;
                    return values;
                }).ToList();
            }
        }
    }
}
