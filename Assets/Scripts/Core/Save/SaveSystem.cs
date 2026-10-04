using System;
using System.IO;

namespace RhythmArmy.Core.Save
{
    public static class SaveSystem
    {
        private static string _saveFilePath;

        public static string SaveFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_saveFilePath))
                {
                    string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RhythmArmy");
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    _saveFilePath = Path.Combine(dir, "savegame.json");
                }
                return _saveFilePath;
            }
            set
            {
                _saveFilePath = value;
            }
        }

        public static SaveData CreateInitialSave()
        {
            return SaveData.CreateDefault();
        }

        public static void Save(SaveData data, string customPath = null)
        {
            string path = customPath ?? SaveFilePath;
            string json = UnityEngineJsonHelper.ToJson(data);
            File.WriteAllText(path, json);
        }

        public static SaveData Load(string customPath = null)
        {
            string path = customPath ?? SaveFilePath;
            if (!File.Exists(path))
            {
                return SaveData.CreateDefault();
            }

            try
            {
                string json = File.ReadAllText(path);
                var loaded = UnityEngineJsonHelper.FromJson<SaveData>(json);
                return loaded ?? SaveData.CreateDefault();
            }
            catch
            {
                return SaveData.CreateDefault();
            }
        }
    }

    public static class UnityEngineJsonHelper
    {
        public static string ToJson(object obj)
        {
            // Simple robust JSON serializer for Unity/C# domain objects
            return MiniJson.Serialize(obj);
        }

        public static T FromJson<T>(string json)
        {
            return MiniJson.Deserialize<T>(json);
        }
    }
}
