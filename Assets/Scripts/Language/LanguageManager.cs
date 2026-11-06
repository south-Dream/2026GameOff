using System;
using System.Collections.Generic;
using UnityEngine;
namespace PlaneShift
{
    /// <summary>
    /// 按语言与稳定标识读取词条
    /// </summary>
    public static class LanguageManager
    {
        private static LanguageData data;
        private static readonly Dictionary<string, string[]> entries = new Dictionary<string, string[]>();
        private static int index;
        public static event Action Changed;
        public static string Current => data != null ? data.languages[index] : throw new InvalidOperationException("LanguageManager is not initialized");
        /// <summary>
        /// 注入词表并设置初始语言
        /// </summary>
        public static void Init(LanguageData source, string code)
        {
            if (source == null) throw new InvalidOperationException("LanguageData is not configured");
            data = source; entries.Clear();
            foreach (var entry in source.entries) entries.Add(entry.id, entry.values);
            SetLanguage(code);
        }
        public static void SetLanguage(string code)
        {
            if (data == null) throw new InvalidOperationException("LanguageManager is not initialized");
            int nextIndex = Array.IndexOf(data.languages, code);
            if (nextIndex < 0) throw new ArgumentException("Unknown language: " + code);
            index = nextIndex;
            Changed?.Invoke();
        }
        public static string Get(string id, params object[] args)
        {
            return GetInLanguage(id, Current, args);
        }
        /// <summary>
        /// 查询指定语言 嵌套词条沿用同一语言
        /// </summary>
        public static string GetInLanguage(string id, string code, params object[] args)
        {
            if (data == null) throw new InvalidOperationException("LanguageManager is not initialized");
            int languageIndex = Array.IndexOf(data.languages, code);
            if (languageIndex < 0) throw new ArgumentException("Unknown language: " + code);
            if (!entries.TryGetValue(id, out var values)) throw new KeyNotFoundException("Missing language ID: " + id);
            object[] resolved = new object[args.Length];
            for (int i = 0; i < args.Length; i++) resolved[i] = args[i] is Word word ? GetInLanguage(word.id, code) : args[i];
            if (values.Length <= languageIndex || string.IsNullOrWhiteSpace(values[languageIndex])) throw new InvalidOperationException("Missing translation: " + id + " / " + code);
            return args.Length == 0 ? values[languageIndex] : string.Format(values[languageIndex], resolved);
        }
        /// <summary>
        /// 保存嵌套词条标识以便随语言切换
        /// </summary>
        public sealed class Word { public readonly string id; public Word(string id) { this.id = id; } }
    }
}
