using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Maps each <see cref="SfxId"/> to an audio clip and how it should be played.</summary>
    [CreateAssetMenu(menuName = "Space Shooter/Sfx Library", fileName = "SfxLibrary")]
    public sealed class SfxLibrary : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public SfxId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 0.8f;
            [Tooltip("A random pitch in this range is picked each time, so repeated sounds don't get tiring.")]
            public float pitchMin = 0.95f;
            public float pitchMax = 1.05f;
            [Tooltip("Shortest gap allowed between two plays of this sound. Stops dozens of simultaneous explosions from blasting the speakers.")]
            public float minInterval = 0.04f;
        }

        public Entry[] entries = new Entry[0];
    }
}
