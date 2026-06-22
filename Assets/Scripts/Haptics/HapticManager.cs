using Haptic.Core;
using UnityEngine;

namespace Haptic.Haptics
{
    public sealed class HapticManager : MonoBehaviour
    {
        AndroidJavaObject vibrator;
        int androidApi;
        HapticIntensity intensity = HapticIntensity.Medium;
        float lastPulseTime = -10f;

        public bool IsAvailable { get; private set; }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                androidApi = version.GetStatic<int>("SDK_INT");
                IsAvailable = vibrator != null && vibrator.Call<bool>("hasVibrator");
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Advanced haptics unavailable: {exception.Message}");
                IsAvailable = true;
            }
#else
            IsAvailable = false;
#endif
        }

        public void SetIntensity(HapticIntensity value) => intensity = value;

        public void WallNearby()
        {
            Play(new long[] { 0, 22, 75, 22 }, new[] { 0, 90, 0, 90 }, false);
        }

        public void HitWall()
        {
            Play(new long[] { 0, 125 }, new[] { 0, 255 }, true);
        }

        public void CorrectDirection(int distance)
        {
            int pause = Mathf.Clamp(45 + distance * 18, 55, 260);
            Play(new long[] { 0, 28, pause, 58 }, new[] { 0, 80, 0, 180 }, false);
        }

        public void WrongDirection()
        {
            Play(new long[] { 0, 85 }, new[] { 0, 75 }, true);
        }

        public void GoalNearby(int distance)
        {
            int pause = Mathf.Clamp(35 + distance * 16, 45, 220);
            Play(new long[] { 0, 35, pause, 35 }, new[] { 0, 95, 0, 150 }, false);
        }

        public void TrapNearby()
        {
            Play(new long[] { 0, 70, 35, 20, 25, 95 }, new[] { 0, 230, 0, 120, 0, 255 }, false);
        }

        public void KeyNearby()
        {
            Play(new long[] { 0, 36, 65, 36 }, new[] { 0, 175, 0, 175 }, false);
        }

        public void KeyCollected()
        {
            Play(new long[] { 0, 30, 45, 30, 45, 75 }, new[] { 0, 130, 0, 170, 0, 230 }, true);
        }

        public void LockedExit()
        {
            Play(new long[] { 0, 90, 45, 55, 45, 30 }, new[] { 0, 230, 0, 150, 0, 80 }, true);
        }

        public void GoalReached()
        {
            Play(new long[] { 0, 35, 45, 65, 55, 140 }, new[] { 0, 90, 0, 160, 0, 255 }, true);
        }

        public void Failure()
        {
            Play(new long[] { 0, 170, 55, 145, 55, 115 }, new[] { 0, 255, 0, 190, 0, 125 }, true);
        }

        public void Stop()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { vibrator?.Call("cancel"); } catch { }
#endif
        }

        void Play(long[] timings, int[] amplitudes, bool immediate)
        {
            if (!immediate && Time.unscaledTime - lastPulseTime < 0.12f)
                return;
            lastPulseTime = Time.unscaledTime;

            float scale = intensity switch
            {
                HapticIntensity.Low => 0.42f,
                HapticIntensity.High => 1f,
                _ => 0.7f
            };
            for (int i = 0; i < amplitudes.Length; i++)
                if (amplitudes[i] > 0)
                    amplitudes[i] = Mathf.Clamp(Mathf.RoundToInt(amplitudes[i] * scale), 1, 255);

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (vibrator != null && androidApi >= 26)
                {
                    using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    using AndroidJavaObject effect = effectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, -1);
                    vibrator.Call("vibrate", effect);
                    return;
                }
                if (vibrator != null)
                {
                    vibrator.Call("vibrate", timings, -1);
                    return;
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Haptic pattern fallback: {exception.Message}");
            }
            Handheld.Vibrate();
#endif
        }
    }
}

