using UnityEngine;

namespace Haptic.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        AudioSource ambient;
        AudioSource effects;
        AudioClip moveClip;
        AudioClip errorClip;
        AudioClip keyClip;
        AudioClip successClip;
        bool enabledByUser = true;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ambient = gameObject.AddComponent<AudioSource>();
            effects = gameObject.AddComponent<AudioSource>();
            ambient.loop = true;
            ambient.volume = 0.16f;
            effects.volume = 0.28f;
            ambient.clip = GenerateAmbient();
            moveClip = GenerateTone("Step", 170f, 0.045f, 0.09f);
            errorClip = GenerateTone("Impact", 65f, 0.13f, 0.35f);
            keyClip = GenerateTone("Key", 620f, 0.18f, 0.24f);
            successClip = GenerateTone("Resolve", 390f, 0.45f, 0.30f, 1.5f);
            ambient.Play();
        }

        public void SetEnabled(bool value)
        {
            enabledByUser = value;
            ambient.mute = !value;
            effects.mute = !value;
        }

        public void Move() => Play(moveClip, 0.45f);
        public void Error() => Play(errorClip, 0.8f);
        public void Key() => Play(keyClip, 0.75f);
        public void Success() => Play(successClip, 1f);

        void Play(AudioClip clip, float volume)
        {
            if (enabledByUser)
                effects.PlayOneShot(clip, volume);
        }

        static AudioClip GenerateAmbient()
        {
            const int sampleRate = 24000;
            const int seconds = 6;
            float[] samples = new float[sampleRate * seconds];
            var random = new System.Random(1947);
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)sampleRate;
                float breath = 0.55f + 0.45f * Mathf.Sin(t * Mathf.PI / 3f);
                float drone = Mathf.Sin(t * Mathf.PI * 2f * 43f) * 0.042f;
                drone += Mathf.Sin(t * Mathf.PI * 2f * 64.5f) * 0.021f;
                float noise = ((float)random.NextDouble() * 2f - 1f) * 0.005f;
                samples[i] = (drone + noise) * breath;
            }
            var clip = AudioClip.Create("Procedural Void", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static AudioClip GenerateTone(string name, float frequency, float duration, float volume, float glide = 1f)
        {
            const int sampleRate = 24000;
            int count = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float normalized = i / (float)count;
                float envelope = Mathf.Sin(normalized * Mathf.PI);
                float currentFrequency = frequency * Mathf.Lerp(1f, glide, normalized);
                phase += Mathf.PI * 2f * currentFrequency / sampleRate;
                samples[i] = Mathf.Sin(phase) * envelope * volume;
            }
            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
