using UnityEngine;

namespace SpaceShooter
{
    /// <summary>How a group of this enemy arrives on screen.</summary>
    public enum SpawnPattern
    {
        /// <summary>One after another along the same route - the classic snake.</summary>
        Stream = 0,
        /// <summary>Side by side in a row.</summary>
        Line = 1,
        /// <summary>A flying V with the leader in front.</summary>
        VFormation = 2,
        /// <summary>Random spots along the top, arriving a moment apart.</summary>
        Scatter = 3,
        /// <summary>Alternating from the left and right edges.</summary>
        Sides = 4
    }

    /// <summary>
    /// The numbers behind one enemy type. To add an enemy: build a prefab with an Enemy component,
    /// create one of these (Create > Space Shooter > Enemy), then add it to the WaveSpawner's roster.
    /// </summary>
    [CreateAssetMenu(menuName = "Space Shooter/Enemy", fileName = "Enemy")]
    public sealed class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Enemy";
        public Enemy prefab;

        [Header("Stats")]
        public float maxHealth = 20f;
        public float moveSpeed = 5f;
        public int scoreValue = 100;

        [Header("Ramming")]
        [Tooltip("Damage dealt when it touches the player.")]
        public float contactDamage = 20f;
        [Tooltip("Destroyed when it rams the player (true for small craft, false for big ships).")]
        public bool dieOnContact = true;

        [Header("Drops")]
        [Range(0f, 1f)] public float dropChance = 0.06f;
        [Tooltip("Pickups this enemy always leaves behind.")]
        public int guaranteedDrops;

        [Header("Wave director")]
        [Tooltip("First wave this enemy can appear in.")]
        public int firstWave = 1;
        [Tooltip("How much of a wave's budget one of these uses up. Tougher enemies should cost more.")]
        public float threatCost = 1f;
        [Tooltip("How often it gets picked compared to the other enemies.")]
        public float spawnWeight = 1f;
        public SpawnPattern pattern = SpawnPattern.Scatter;
        [Tooltip("Smallest and largest group size.")]
        public Vector2Int groupSize = new Vector2Int(3, 6);
        [Tooltip("Seconds between members of a group arriving.")]
        public float spawnInterval = 0.3f;
        [Tooltip("World units between members of a formation.")]
        public float spacing = 1.8f;
        [Tooltip("Must be destroyed (or leave the screen) before the wave counts as cleared.")]
        public bool blocksWaveClear = true;

        [Header("Death")]
        public GameObject deathEffect;
        public SfxId deathSfx = SfxId.ExplosionSmall;
        [Range(0f, 1f)] public float deathShake = 0.12f;
    }
}
