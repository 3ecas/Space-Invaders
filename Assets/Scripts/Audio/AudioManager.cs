using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Plays sound effects through a small ring of AudioSources.
    /// Anywhere in the game: <c>AudioManager.Play(SfxId.Blaster);</c>
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        const int TableSize = 64;

        static AudioManager instance;

        [SerializeField] SfxLibrary library;
        [SerializeField, Range(0f, 1f)] float masterVolume = 0.7f;
        [Tooltip("How many sounds can overlap before the oldest one is cut off.")]
        [SerializeField] int voices = 24;

        readonly SfxLibrary.Entry[] table = new SfxLibrary.Entry[TableSize];
        readonly float[] lastPlayed = new float[TableSize];
        AudioSource[] sources;
        int nextSource;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => instance = null;

        void Awake()
        {
            instance = this;

            sources = new AudioSource[Mathf.Max(1, voices)];
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                sources[i] = source;
            }

            for (int i = 0; i < lastPlayed.Length; i++) lastPlayed[i] = -10f;

            if (library == null) return;
            foreach (SfxLibrary.Entry entry in library.entries)
            {
                int index = (int)entry.id;
                if (index > 0 && index < TableSize) table[index] = entry;
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void Play(SfxId id, float volumeScale = 1f)
        {
            if (instance != null) instance.PlayInternal(id, volumeScale);
        }

        void PlayInternal(SfxId id, float volumeScale)
        {
            int index = (int)id;
            if (index <= 0 || index >= TableSize) return;

            SfxLibrary.Entry entry = table[index];
            if (entry == null || entry.clip == null) return;

            float now = Time.unscaledTime;
            if (now - lastPlayed[index] < entry.minInterval) return;
            lastPlayed[index] = now;

            AudioSource source = sources[nextSource];
            nextSource = (nextSource + 1) % sources.Length;

            source.clip = entry.clip;
            source.pitch = Random.Range(entry.pitchMin, entry.pitchMax);
            source.volume = entry.volume * volumeScale * masterVolume;
            source.Play();
        }
    }
}
