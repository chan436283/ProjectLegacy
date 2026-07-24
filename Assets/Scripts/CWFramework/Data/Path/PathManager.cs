using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace CWFramework
{
    public class PathManager
    {
        public static readonly string targetPath = "Resources/Data/PathMetaData.json";
        public static readonly HashSet<string> ignoreExtensionSet = new() { ".meta", ".shader", ".cginc", ".text" };
        public static readonly HashSet<string> ignoreDirectorySet = new() { "VETASOFT 2DxFX", "ParadoxNotion" };
        private static Dictionary<string, string> database = new();

        public static string ToUnixPath(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string ToLeafPath(string filePath)
        {
            string relPath = ToUnixPath(filePath);
            string leafPath = relPath.Trim('/');
            string extension = Path.GetExtension(filePath);
            leafPath = leafPath[..^extension.Length];
            return leafPath;
        }

        public static string Lookup(string fileNameWithExtension)
        {
            if (string.IsNullOrEmpty(fileNameWithExtension))
            {
                return null;
            }

            if (database.Count == 0)
            {
                Load();
            }

            if (database.TryGetValue(fileNameWithExtension, out string path))
            {
                return path;
            }
            else
            {
                Log.Warning(LogTags.Resource, "주소를 읽어올 수 없습니다. 파일이 등록되어있지 않습니다. 파일 이름: {0}", fileNameWithExtension);
            }

            return null;
        }

        public static void Load()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Data/PathMetadata");

            if (textAsset == null)
            {
                Log.Error("PathMetadata 파일을 불러오지 못했습니다.");
            }
            else
            {
                try
                {
                    database = JsonUtility.FromJson<XSerialization<string, string>>(textAsset.text).ToDictionary();
                    if (database.Count > 0)
                    {
                        Log.Info(LogTags.Resource, "'PathMetadata' 파일을 불러옵니다. 등록된 파일 수: {0}", database.Count);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }

        public static bool CheckLoaded()
        {
            return database != null && database.Count > 0 && File.Exists(Path.Combine(Application.dataPath, targetPath));
        }

        public static void UpdatePathMetadata()
        {
            database.Clear();

            string[] dirPaths = Directory.GetDirectories(Application.dataPath, "Resources", SearchOption.AllDirectories);

            foreach (string dir in dirPaths)
            {
                string[] filePaths = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                foreach (string filePath in filePaths)
                {
                    AddPath(filePath, dir.Length);
                }
            }

            string path = Path.Combine(Application.dataPath, targetPath);
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string text = JsonUtility.ToJson(new XSerialization<string, string>(database));
            WriteFile(path, text);
        }

        private static void AddPath(string filePath, int directoryLength)
        {
            if (CheckIgnore(filePath))
            {
                return;
            }

            string relPath = ToUnixPath(filePath.Remove(0, directoryLength));
            string fileName = Path.GetFileName(filePath);

            if (fileName.StartsWith("."))
            {
                return;
            }

            if (!database.ContainsKey(fileName))
            {
                string leafPath = relPath.Trim('/');
                string extension = Path.GetExtension(filePath);
                leafPath = leafPath[..^extension.Length];
                database.Add(fileName, leafPath);
            }
            else
            {
                Log.Warning(LogTags.Resource, "같은 이름을 가진 파일이 이미 등록되어있습니다. 파일 이름: {0}", fileName);
            }
        }

        private static bool CheckIgnore(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (ignoreExtensionSet.Contains(extension))
            {
                return true;
            }

            foreach (string ignoreDirectory in ignoreDirectorySet)
            {
                if (filePath.Contains(ignoreDirectory))
                {
                    return true;
                }
            }

            return false;
        }

        private static async void WriteFile(string filename, string text)
        {
            try
            {
                using StreamWriter file = new StreamWriter(filename, false, Encoding.UTF8);
                await file.WriteAsync(text);
                string content = $"Compeleted Write Path MetaData File. count: {database.Count}";
                DisplayDialog("Notice", content);
            }
            catch (UnauthorizedAccessException)
            {
                DisplayDialog("Notice", "You have no write permission in that folder.");
            }
            catch (Exception e)
            {
                DisplayDialog("Notice", e.ToString());
            }
        }

        public static void DisplayDialog(string content)
        {
            DisplayDialog("Notice", content);
        }

        private static void DisplayDialog(string title, string content)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(title, content, "Ok");
#endif
        }

        private static string FindPathWithExtension(string name, string extension)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            return Lookup($"{name}{extension}");
        }

        public static string FindPrefabPath(string name) => FindPathWithExtension(name, ".prefab");

        public static string FindAtlasPath(string name) => FindPathWithExtension(name, ".spriteatlas");

        public static string FindImagePath(string name) => FindPathWithExtension(name, ".png");

        public static string FindMaterialPath(string name) => FindPathWithExtension(name, ".mat");

        public static string FindShaderPath(string name) => FindPathWithExtension(name, ".shader");

        public static string FindAssetPath(string name) => FindPathWithExtension(name, ".asset");

        public static string FindVideoPath(string name) => FindPathWithExtension(name, ".mp4");

        public static string[] FindAllAssetPath()
        {
            List<string> assetPaths = new();
            foreach (KeyValuePair<string, string> item in database)
            {
                if (item.Key.Contains(".asset"))
                {
                    assetPaths.Add(item.Value);
                }
            }
            return assetPaths.ToArray();
        }
    }
}