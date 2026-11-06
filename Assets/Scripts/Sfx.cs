using UnityEngine;

namespace TokyoRunner
{
    /// <summary>All sounds and the synthwave music are generated in code - no audio files needed.</summary>
    public static class Sfx
    {
        const int SR = 22050;
        public static AudioClip Coin, Jump, Slide, Crash, Music;
        static AudioSource src;

        public static void Init(GameObject host)
        {
            src = host.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            src.playOnAwake = false;

            Coin = Make("coin", 0.25f, t =>
            {
                float f = t < 0.07f ? 880f : 1320f;
                float sq = Mathf.Sin(6.2831853f * f * t) >= 0f ? 1f : -1f;
                return sq * Mathf.Exp(-t * 10f) * 0.3f;
            });
            Jump = Make("jump", 0.2f, t => Mathf.Sin(6.2831853f * (280f * t + 1500f * t * t)) * Mathf.Exp(-t * 6f) * 0.4f);
            Slide = Make("slide", 0.35f, t =>
            {
                float e = 1f - t / 0.35f;
                return (Random.value * 2f - 1f) * e * e * 0.22f;
            });
            Crash = Make("crash", 0.7f, t =>
            {
                float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 6f) * 0.5f;
                float boom = Mathf.Sin(6.2831853f * (120f * t - 90f * t * t)) * Mathf.Exp(-t * 4f) * 0.6f;
                return noise + boom;
            });
            Music = MakeMusic();
        }

        public static void Play(AudioClip c, float vol)
        {
            if (src != null && c != null) src.PlayOneShot(c, vol);
        }

        static AudioClip Make(string name, float dur, System.Func<float, float> f)
        {
            int n = (int)(dur * SR);
            float[] d = new float[n];
            for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f((float)i / SR), -1f, 1f);
            AudioClip c = AudioClip.Create(name, n, 1, SR, false);
            c.SetData(d, 0);
            return c;
        }

        // ---------- music: Am - F - C - G, dark synthwave ----------
        static void Note(float[] buf, int start, float freq, float dur, float vol, int wave, float decay)
        {
            int n = (int)(dur * SR);
            for (int i = 0; i < n; i++)
            {
                int idx = start + i;
                if (idx >= buf.Length) break;
                float t = (float)i / SR;
                float ph = (freq * t) % 1f;
                float s = wave == 0 ? Mathf.Sin(ph * 6.2831853f) : (wave == 1 ? (ph * 2f - 1f) : (ph < 0.5f ? 1f : -1f));
                float env = Mathf.Exp(-t * decay) * Mathf.Min(1f, t * 200f);
                buf[idx] += s * env * vol;
            }
        }

        static void Kick(float[] b, int s)
        {
            int n = (int)(0.22f * SR);
            float ph = 0f;
            for (int i = 0; i < n && s + i < b.Length; i++)
            {
                float t = (float)i / SR;
                float f = 45f + 110f * Mathf.Exp(-t * 28f);
                ph += f / SR;
                b[s + i] += Mathf.Sin(ph * 6.2831853f) * Mathf.Exp(-t * 14f) * 0.55f;
            }
        }

        static void Hat(float[] b, int s)
        {
            int n = (int)(0.05f * SR);
            for (int i = 0; i < n && s + i < b.Length; i++)
            {
                float t = (float)i / SR;
                b[s + i] += (Random.value * 2f - 1f) * Mathf.Exp(-t * 90f) * 0.12f;
            }
        }

        static AudioClip MakeMusic()
        {
            float bpm = 108f;
            float beat = 60f / bpm;
            int bars = 8;
            int total = (int)(bars * 4 * beat * SR);
            float[] buf = new float[total];

            float[] bass = { 110f, 87.31f, 130.81f, 98f };
            float[][] chords = {
                new float[] { 220f, 261.63f, 329.63f, 440f },
                new float[] { 174.61f, 220f, 261.63f, 349.23f },
                new float[] { 261.63f, 329.63f, 392f, 523.25f },
                new float[] { 196f, 246.94f, 293.66f, 392f }
            };
            int[] arp = { 0, 1, 2, 3, 2, 1, 2, 3 };

            for (int bar = 0; bar < bars; bar++)
            {
                int ch = (bar / 2) % 4;
                for (int step = 0; step < 16; step++)
                {
                    float tStart = (bar * 4 + step * 0.25f) * beat;
                    int s0 = (int)(tStart * SR);
                    Note(buf, s0, chords[ch][arp[step % 8]], 0.18f, 0.06f, 2, 12f);
                    if (step % 2 == 0) Note(buf, s0, bass[ch], 0.22f, 0.16f, 1, 5f);
                    if (step % 4 == 0) Kick(buf, s0);
                    if (step % 4 == 2) Hat(buf, s0);
                }
            }

            float peak = 0.001f;
            for (int i = 0; i < buf.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(buf[i]));
            float g = 0.8f / peak;
            for (int i = 0; i < buf.Length; i++) buf[i] *= g;

            AudioClip c = AudioClip.Create("tokyo_night_theme", total, 1, SR, false);
            c.SetData(buf, 0);
            return c;
        }
    }
}
