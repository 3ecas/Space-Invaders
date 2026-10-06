using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// Synthesises the game's sound effects from simple waveforms and noise, and saves them as
    /// WAV files in Assets/Audio/Sfx. Replace any WAV with your own sound (keep the file name)
    /// and the game will use it.
    /// </summary>
    public static class SfxFactory
    {
        public const string Folder = "Assets/Audio/Sfx";
        const int SampleRate = 44100;

        enum Wave { Sine, Square, Saw, Triangle }

        static System.Random random;

        public static Dictionary<SfxId, AudioClip> BuildAll()
        {
            random = new System.Random(20261005);
            EditorUtil.EnsureFolder(Folder);

            var names = new Dictionary<SfxId, string>();
            void Add(SfxId id, params float[][] layers)
            {
                string name = id.ToString();
                WriteWav($"{Folder}/{name}.wav", Mix(layers));
                names[id] = name;
            }

            // --- player weapons
            Add(SfxId.Blaster, Tone(Wave.Square, 900f, 240f, 0.11f, 0.5f), Tone(Wave.Sine, 1800f, 480f, 0.08f, 0.35f));
            Add(SfxId.Spread, Noise(0.14f, 0.8f, 4500f, 700f), Tone(Wave.Saw, 320f, 110f, 0.13f, 0.5f));
            Add(SfxId.Vulcan, Tone(Wave.Square, 560f, 300f, 0.05f, 0.5f), Noise(0.03f, 0.5f, 6000f, 3000f));
            Add(SfxId.Rail, Tone(Wave.Saw, 2600f, 160f, 0.38f, 0.6f, 1.6f), Tone(Wave.Sine, 95f, 45f, 0.34f, 0.8f), Noise(0.25f, 0.5f, 7000f, 500f));
            Add(SfxId.Missile, Noise(0.36f, 0.8f, 700f, 3800f, 1.4f, 0.05f), Tone(Wave.Sine, 170f, 330f, 0.3f, 0.35f, 1.5f, 0.03f));
            Add(SfxId.Plasma, Tone(Wave.Sine, 230f, 105f, 0.36f, 0.8f, 1.5f, 0.01f, 32f, 0.25f), Tone(Wave.Triangle, 460f, 210f, 0.3f, 0.4f));

            // --- combat
            Add(SfxId.EnemyShoot, Tone(Wave.Triangle, 540f, 290f, 0.13f, 0.7f));
            Add(SfxId.Hit, Noise(0.045f, 0.8f, 5500f, 2200f), Tone(Wave.Square, 320f, 200f, 0.03f, 0.3f));
            Add(SfxId.ExplosionSmall, Noise(0.30f, 0.9f, 3200f, 280f, 1.8f), Tone(Wave.Sine, 150f, 50f, 0.26f, 0.7f));
            Add(SfxId.ExplosionMedium, Noise(0.52f, 0.9f, 2600f, 180f, 1.7f), Tone(Wave.Sine, 115f, 40f, 0.46f, 0.8f));
            Add(SfxId.ExplosionLarge, Noise(1.15f, 0.9f, 2200f, 90f, 1.5f), Tone(Wave.Sine, 85f, 28f, 1.0f, 0.9f, 1.5f),
                Delay(Noise(0.6f, 0.6f, 1600f, 120f, 1.6f), 0.16f));

            // --- player
            Add(SfxId.PlayerHurt, Tone(Wave.Saw, 420f, 110f, 0.26f, 0.7f), Noise(0.10f, 0.6f, 3000f, 900f));
            Add(SfxId.PlayerDeath, Noise(1.4f, 0.9f, 2400f, 70f, 1.3f), Tone(Wave.Saw, 620f, 55f, 1.0f, 0.5f, 1.4f), Tone(Wave.Sine, 90f, 26f, 1.2f, 0.9f, 1.4f));
            Add(SfxId.ShieldUp, Tone(Wave.Sine, 300f, 1250f, 0.42f, 0.7f, 1.2f, 0.02f, 18f, 0.05f), Tone(Wave.Triangle, 600f, 2500f, 0.42f, 0.25f, 1.2f, 0.02f));
            Add(SfxId.ShieldBlock, Tone(Wave.Sine, 1450f, 900f, 0.10f, 0.7f), Tone(Wave.Triangle, 2150f, 1700f, 0.08f, 0.3f));
            Add(SfxId.ShieldDown, Tone(Wave.Sine, 920f, 190f, 0.36f, 0.7f, 1.4f));
            Add(SfxId.Dash, Noise(0.20f, 0.8f, 1400f, 6500f, 1.3f, 0.03f));
            Add(SfxId.Bomb, Noise(1.4f, 1f, 4200f, 70f, 1.2f), Tone(Wave.Sine, 125f, 24f, 1.25f, 1f, 1.3f), Tone(Wave.Sine, 220f, 2100f, 0.16f, 0.5f, 1f, 0.02f));
            Add(SfxId.WeaponSwitch, Tone(Wave.Square, 660f, 660f, 0.03f, 0.4f), Delay(Tone(Wave.Square, 990f, 990f, 0.045f, 0.4f), 0.04f));

            // --- pickups
            Add(SfxId.Pickup, Tone(Wave.Triangle, 660f, 660f, 0.07f, 0.7f, 1f), Delay(Tone(Wave.Triangle, 880f, 880f, 0.07f, 0.7f, 1f), 0.06f),
                Delay(Tone(Wave.Triangle, 1320f, 1320f, 0.16f, 0.7f), 0.12f));
            Add(SfxId.PowerUp, Tone(Wave.Square, 523f, 523f, 0.08f, 0.4f, 1f), Delay(Tone(Wave.Square, 659f, 659f, 0.08f, 0.4f, 1f), 0.07f),
                Delay(Tone(Wave.Square, 784f, 784f, 0.08f, 0.4f, 1f), 0.14f), Delay(Tone(Wave.Square, 1047f, 1047f, 0.26f, 0.4f), 0.21f));
            Add(SfxId.Heal, Tone(Wave.Sine, 440f, 880f, 0.28f, 0.7f, 1.3f, 0.02f), Tone(Wave.Sine, 660f, 1320f, 0.28f, 0.35f, 1.3f, 0.02f));

            // --- game flow
            Add(SfxId.WaveStart, Tone(Wave.Triangle, 440f, 440f, 0.13f, 0.7f, 1f), Delay(Tone(Wave.Triangle, 660f, 660f, 0.26f, 0.7f), 0.12f));
            Add(SfxId.BossWarning, Klaxon(), Delay(Klaxon(), 0.55f), Delay(Klaxon(), 1.1f));
            Add(SfxId.GameOver, Tone(Wave.Triangle, 660f, 660f, 0.22f, 0.7f, 1f), Delay(Tone(Wave.Triangle, 523f, 523f, 0.22f, 0.7f, 1f), 0.2f),
                Delay(Tone(Wave.Triangle, 440f, 440f, 0.22f, 0.7f, 1f), 0.4f), Delay(Tone(Wave.Triangle, 330f, 320f, 0.7f, 0.7f), 0.6f));
            Add(SfxId.Confirm, Tone(Wave.Triangle, 880f, 1320f, 0.12f, 0.7f));

            AssetDatabase.Refresh();

            var clips = new Dictionary<SfxId, AudioClip>();
            foreach (KeyValuePair<SfxId, string> pair in names)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{Folder}/{pair.Value}.wav");
                if (clip == null) Debug.LogError($"[Space Shooter setup] Sound '{pair.Value}' failed to import.");
                clips[pair.Key] = clip;
            }
            return clips;
        }

        static float[] Klaxon()
        {
            return Mix(Tone(Wave.Saw, 440f, 440f, 0.24f, 0.6f, 0.6f, 0.02f), Delay(Tone(Wave.Saw, 330f, 330f, 0.26f, 0.6f, 0.8f, 0.02f), 0.24f));
        }

        // ---------------------------------------------------------------- synthesis

        /// <summary>
        /// A pitched tone that slides from one frequency to another while fading out.
        /// A higher <paramref name="decay"/> makes it die away faster.
        /// </summary>
        static float[] Tone(Wave wave, float startHz, float endHz, float duration, float volume,
            float decay = 2f, float attack = 0.004f, float vibratoHz = 0f, float vibratoDepth = 0f)
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
            var samples = new float[count];
            double phase = 0.0;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float time = i / (float)SampleRate;

                float frequency = startHz * Mathf.Pow(endHz / startHz, t);
                if (vibratoHz > 0f) frequency *= 1f + vibratoDepth * Mathf.Sin(2f * Mathf.PI * vibratoHz * time);

                phase += frequency / SampleRate;
                float p = (float)(phase - System.Math.Floor(phase));

                float value;
                switch (wave)
                {
                    case Wave.Square: value = p < 0.5f ? 1f : -1f; break;
                    case Wave.Saw: value = 2f * p - 1f; break;
                    case Wave.Triangle: value = 1f - 4f * Mathf.Abs(p - 0.5f); break;
                    default: value = Mathf.Sin(2f * Mathf.PI * p); break;
                }

                samples[i] = value * Envelope(t, time, attack, decay) * volume;
            }
            return samples;
        }

        /// <summary>Filtered white noise. Sweeping the cutoff down gives explosions; sweeping it up gives whooshes.</summary>
        static float[] Noise(float duration, float volume, float cutoffStartHz, float cutoffEndHz, float decay = 2f, float attack = 0.002f)
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
            var samples = new float[count];
            float filtered = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float time = i / (float)SampleRate;

                // One-pole low-pass filter with a moving cutoff.
                float cutoff = cutoffStartHz * Mathf.Pow(cutoffEndHz / cutoffStartHz, t);
                float k = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SampleRate);
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += (white - filtered) * k;

                samples[i] = filtered * Envelope(t, time, attack, decay) * volume;
            }
            return samples;
        }

        static float Envelope(float t, float time, float attack, float decay)
        {
            float rise = attack > 0f ? Mathf.Clamp01(time / attack) : 1f;
            return rise * Mathf.Pow(1f - t, decay);
        }

        static float[] Delay(float[] samples, float seconds)
        {
            int offset = Mathf.RoundToInt(seconds * SampleRate);
            var result = new float[samples.Length + offset];
            System.Array.Copy(samples, 0, result, offset, samples.Length);
            return result;
        }

        /// <summary>Adds layers together and scales the result so the loudest peak sits just below clipping.</summary>
        static float[] Mix(params float[][] layers)
        {
            int length = 0;
            foreach (float[] layer in layers) length = Mathf.Max(length, layer.Length);

            var result = new float[length];
            foreach (float[] layer in layers)
            {
                for (int i = 0; i < layer.Length; i++) result[i] += layer[i];
            }

            float peak = 0f;
            for (int i = 0; i < length; i++) peak = Mathf.Max(peak, Mathf.Abs(result[i]));
            if (peak > 0.0001f)
            {
                float gain = 0.85f / peak;
                for (int i = 0; i < length; i++) result[i] *= gain;
            }
            return result;
        }

        // ---------------------------------------------------------------- file output

        static void WriteWav(string path, float[] samples)
        {
            int dataSize = samples.Length * 2;

            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);                   // size of the format block
                writer.Write((short)1);             // PCM
                writer.Write((short)1);             // mono
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);       // bytes per second
                writer.Write((short)2);             // bytes per sample
                writer.Write((short)16);            // bits per sample
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);

                foreach (float sample in samples)
                {
                    writer.Write((short)Mathf.Clamp(Mathf.RoundToInt(sample * 32767f), short.MinValue, short.MaxValue));
                }
            }
        }
    }
}
