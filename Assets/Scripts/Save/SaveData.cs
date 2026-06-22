using System;
using Haptic.Core;

namespace Haptic.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public int unlockedLevel = 1;
        public float[] bestTimes = new float[10];
        public GameSettings settings = new GameSettings();

        public void Normalize(int levelCount)
        {
            unlockedLevel = Math.Max(1, Math.Min(levelCount, unlockedLevel));
            if (bestTimes == null || bestTimes.Length != levelCount)
            {
                var resized = new float[levelCount];
                if (bestTimes != null)
                    Array.Copy(bestTimes, resized, Math.Min(bestTimes.Length, resized.Length));
                bestTimes = resized;
            }

            settings ??= new GameSettings();
        }
    }
}

