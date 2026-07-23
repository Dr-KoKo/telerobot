using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public static class ProceduralCombatAudio
    {
        private const int SampleRate = 22050;

        public static AudioClip CreateTransient(string name, float frequency, float duration, float noiseMix)
        {
            var safeDuration = Mathf.Max(0.01f, duration);
            var sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * safeDuration));
            var samples = new float[sampleCount];
            var safeNoiseMix = Mathf.Clamp01(noiseMix);
            for (var index = 0; index < sampleCount; index++)
            {
                var time = index / (float)SampleRate;
                var progress = index / (float)sampleCount;
                var envelope = (1f - progress) * (1f - progress);
                var tone = Mathf.Sin(2f * Mathf.PI * frequency * time) * 0.72f +
                           Mathf.Sin(2f * Mathf.PI * frequency * 2.03f * time) * 0.28f;
                var noiseSeed = Mathf.Sin(index * 12.9898f) * 43758.5453f;
                var noise = (noiseSeed - Mathf.Floor(noiseSeed)) * 2f - 1f;
                samples[index] = Mathf.Lerp(tone, noise, safeNoiseMix) * envelope * 0.55f;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
