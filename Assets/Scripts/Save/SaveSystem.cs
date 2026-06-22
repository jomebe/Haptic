using Haptic.Core;
using UnityEngine;

namespace Haptic.Save
{
    public sealed class SaveSystem
    {
        const string SaveKey = "haptic.save.v1";
        readonly int levelCount;

        public SaveData Data { get; private set; }
        public GameSettings Settings => Data.settings;

        public SaveSystem(int levelCount)
        {
            this.levelCount = levelCount;
            Load();
        }

        public void Load()
        {
            try
            {
                string json = PlayerPrefs.GetString(SaveKey, string.Empty);
                Data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                Data = new SaveData();
            }
            Data ??= new SaveData();
            Data.Normalize(levelCount);
        }

        public void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public void CompleteLevel(int levelIndex, float elapsed)
        {
            float current = Data.bestTimes[levelIndex];
            if (current <= 0f || elapsed < current)
                Data.bestTimes[levelIndex] = elapsed;
            Data.unlockedLevel = Mathf.Max(Data.unlockedLevel, Mathf.Min(levelCount, levelIndex + 2));
            Save();
        }

        public void ResetProgress()
        {
            GameSettings preserved = Data.settings;
            Data = new SaveData { settings = preserved };
            Data.Normalize(levelCount);
            Save();
        }
    }
}
