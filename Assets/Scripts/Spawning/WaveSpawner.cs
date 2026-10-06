using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// The wave director. Instead of hand-written levels it gives each wave a "budget" that grows
    /// over time and spends it on groups of enemies from the roster, so the game never runs out of
    /// waves. Every few waves a boss turns up instead.
    /// </summary>
    public sealed class WaveSpawner : MonoBehaviour
    {
        [Header("Enemies")]
        [SerializeField] EnemyData[] roster;
        [SerializeField] EnemyData[] bosses;
        [Tooltip("A boss appears every this many waves. 0 = never.")]
        [SerializeField] int bossEveryWaves = 5;

        [Header("Wave size")]
        [Tooltip("Threat budget of wave 1. More budget = more (or tougher) enemies.")]
        [SerializeField] float startBudget = 24f;
        [SerializeField] float budgetPerWave = 10f;
        [Tooltip("Spawning pauses while this many enemies are already alive.")]
        [SerializeField] int maxEnemiesAlive = 140;

        [Header("Timing")]
        [SerializeField] float firstWaveDelay = 1.5f;
        [SerializeField] float timeBetweenWaves = 3f;
        [Tooltip("Shortest and longest pause between groups arriving.")]
        [SerializeField] Vector2 groupDelay = new Vector2(0.8f, 1.9f);
        [Tooltip("A wave ends after this long even if stragglers are still alive.")]
        [SerializeField] float maxWaveDuration = 55f;

        [Header("Difficulty growth per wave")]
        [SerializeField] float healthPerWave = 0.09f;
        [SerializeField] float speedPerWave = 0.012f;
        [SerializeField] float bulletSpeedPerWave = 0.02f;
        [Tooltip("Extra damage enemies deal per wave, as a fraction.")]
        [SerializeField] float damagePerWave = 0.035f;
        [SerializeField] float maxSpeedScale = 1.5f;
        [SerializeField] float maxBulletSpeedScale = 1.8f;
        [SerializeField] float maxDamageScale = 2.5f;
        [Tooltip("Extra boss health each time a boss comes back, as a fraction.")]
        [SerializeField] float bossHealthPerVisit = 0.75f;

        public int Wave { get; private set; }
        public bool BossActive { get; private set; }

        readonly List<EnemyData> candidates = new List<EnemyData>();
        Coroutine routine;
        int groupsArriving;

        void OnEnable() => GameEvents.GameStateChanged += OnGameStateChanged;

        void OnDisable() => GameEvents.GameStateChanged -= OnGameStateChanged;

        void OnGameStateChanged(GameState state)
        {
            if (state != GameState.GameOver) return;
            StopAllCoroutines();
            routine = null;
        }

        /// <summary>Starts sending waves. Called by the GameManager when a run begins.</summary>
        public void Begin()
        {
            if (routine == null) routine = StartCoroutine(Run());
        }

        public bool IsBossWave(int wave)
        {
            return bossEveryWaves > 0 && bosses != null && bosses.Length > 0 && wave % bossEveryWaves == 0;
        }

        IEnumerator Run()
        {
            yield return new WaitForSeconds(firstWaveDelay);

            while (true)
            {
                Wave++;
                bool boss = IsBossWave(Wave);

                GameEvents.RaiseWaveStarted(Wave, boss);
                AudioManager.Play(boss ? SfxId.BossWarning : SfxId.WaveStart);
                yield return new WaitForSeconds(boss ? 2.5f : 1f);

                if (boss) yield return BossWave();
                else yield return NormalWave();

                GameEvents.RaiseWaveCleared(Wave);
                // A burst of speed between waves: the "warp" to the next fight.
                WorldScroll.Boost(boss ? 6f : 3.5f, boss ? 2.5f : 1.2f);
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        // ---------------------------------------------------------------- normal waves

        IEnumerator NormalWave()
        {
            DifficultyScale scale = ScaleFor(Wave);
            float budget = startBudget + budgetPerWave * (Wave - 1);
            float startedAt = Time.time;
            groupsArriving = 0;

            // Later waves pack their groups closer together.
            float pace = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01((Wave - 1) / 20f));

            while (budget > 0f)
            {
                EnemyData data = PickEnemy(budget, float.MaxValue);
                if (data == null) break;

                int affordable = Mathf.Max(1, Mathf.FloorToInt(budget / data.threatCost));
                int count = Mathf.Clamp(Random.Range(data.groupSize.x, data.groupSize.y + 1), 1, affordable);
                budget -= count * data.threatCost;

                while (Enemy.Active.Count + count > maxEnemiesAlive) yield return null;

                StartCoroutine(SpawnGroup(data, count, scale));
                yield return new WaitForSeconds(Random.Range(groupDelay.x, groupDelay.y) * pace);
            }

            while ((groupsArriving > 0 || Enemy.WaveBlockers > 0) && Time.time - startedAt < maxWaveDuration)
            {
                yield return null;
            }
        }

        /// <summary>Weighted random pick among the enemies unlocked by now that fit the budget.</summary>
        EnemyData PickEnemy(float budget, float maxCost)
        {
            candidates.Clear();
            float totalWeight = 0f;

            if (roster != null)
            {
                foreach (EnemyData data in roster)
                {
                    if (data == null || data.prefab == null) continue;
                    if (data.firstWave > Wave || data.threatCost > budget || data.threatCost > maxCost) continue;
                    if (data.spawnWeight <= 0f) continue;
                    candidates.Add(data);
                    totalWeight += data.spawnWeight;
                }
            }

            if (candidates.Count == 0) return null;

            float roll = Random.value * totalWeight;
            foreach (EnemyData data in candidates)
            {
                roll -= data.spawnWeight;
                if (roll <= 0f) return data;
            }
            return candidates[candidates.Count - 1];
        }

        IEnumerator SpawnGroup(EnemyData data, int count, DifficultyScale scale)
        {
            groupsArriving++;

            float top = PlayArea.Top + 1.5f;
            float side = Random.value < 0.5f ? -1f : 1f;

            switch (data.pattern)
            {
                case SpawnPattern.Stream:
                {
                    int path = Random.Range(0, EnemyPaths.Count);
                    float x = PlayArea.RandomX(2.5f);
                    for (int i = 0; i < count; i++)
                    {
                        EnemySpawnInfo info = Info(i, count, side);
                        info.pathIndex = path;
                        info.phase = i * 0.45f;
                        Enemy.Spawn(data, new Vector2(x, top), info, scale);
                        yield return new WaitForSeconds(data.spawnInterval);
                    }
                    break;
                }

                case SpawnPattern.Line:
                {
                    float spacing = Mathf.Min(data.spacing, (PlayArea.Width - 4f) / Mathf.Max(1, count - 1));
                    float width = spacing * (count - 1);
                    float startX = Random.Range(PlayArea.Left + 2f, Mathf.Max(PlayArea.Left + 2f, PlayArea.Right - 2f - width));
                    float hoverY = PlayArea.Top - Random.Range(2.5f, 5.5f);
                    for (int i = 0; i < count; i++)
                    {
                        float x = startX + i * spacing;
                        EnemySpawnInfo info = Info(i, count, side);
                        info.anchor = new Vector2(x, hoverY - (i % 2) * 1.1f);
                        info.phase = i * 0.6f;
                        Enemy.Spawn(data, new Vector2(x, top + (i % 2) * 0.8f), info, scale);
                    }
                    break;
                }

                case SpawnPattern.VFormation:
                {
                    int ranks = count / 2;
                    float halfWidth = ranks * data.spacing;
                    float centerX = Random.Range(PlayArea.Left + 2f + halfWidth, Mathf.Max(PlayArea.Left + 2f + halfWidth, PlayArea.Right - 2f - halfWidth));
                    float hoverY = PlayArea.Top - Random.Range(2.5f, 4.5f);
                    for (int i = 0; i < count; i++)
                    {
                        // The leader flies in front; the rest alternate left and right behind it.
                        int rank = (i + 1) / 2;
                        float wing = i == 0 ? 0f : (i % 2 == 0 ? 1f : -1f);
                        float x = Mathf.Clamp(centerX + wing * rank * data.spacing, PlayArea.Left + 1f, PlayArea.Right - 1f);
                        EnemySpawnInfo info = Info(i, count, side);
                        info.anchor = new Vector2(x, hoverY + rank * 0.9f);
                        Enemy.Spawn(data, new Vector2(x, top + rank * data.spacing * 0.8f), info, scale);
                    }
                    break;
                }

                case SpawnPattern.Scatter:
                {
                    for (int i = 0; i < count; i++)
                    {
                        float x = PlayArea.RandomX(1.5f);
                        EnemySpawnInfo info = Info(i, count, side);
                        info.anchor = new Vector2(x, PlayArea.Top - Random.Range(2.5f, 6.5f));
                        info.phase = Random.value * 6.28f;
                        Enemy.Spawn(data, new Vector2(x, top + Random.Range(0f, 1.5f)), info, scale);
                        yield return new WaitForSeconds(data.spawnInterval * Random.Range(0.5f, 1.5f));
                    }
                    break;
                }

                case SpawnPattern.Sides:
                {
                    for (int i = 0; i < count; i++)
                    {
                        float from = i % 2 == 0 ? side : -side;
                        float x = from > 0f ? PlayArea.Right + 1.5f : PlayArea.Left - 1.5f;
                        float y = Random.Range(PlayArea.Center.y + 1f, PlayArea.Top - 1.5f);
                        EnemySpawnInfo info = Info(i, count, from);
                        info.direction = new Vector2(-from, -0.22f).normalized;
                        info.phase = Random.value * 6.28f;
                        Enemy.Spawn(data, new Vector2(x, y), info, scale);
                        yield return new WaitForSeconds(data.spawnInterval);
                    }
                    break;
                }
            }

            groupsArriving--;
        }

        static EnemySpawnInfo Info(int index, int count, float side)
        {
            EnemySpawnInfo info = EnemySpawnInfo.Default;
            info.index = index;
            info.groupSize = count;
            info.side = side;
            return info;
        }

        // ---------------------------------------------------------------- boss waves

        IEnumerator BossWave()
        {
            int visit = Mathf.Max(1, Wave / bossEveryWaves);
            EnemyData data = bosses[(visit - 1) % bosses.Length];
            if (data == null) yield break;

            DifficultyScale waveScale = ScaleFor(Wave);
            var bossScale = new DifficultyScale(1f + bossHealthPerVisit * (visit - 1), 1f, waveScale.BulletSpeed, waveScale.Damage);

            Enemy boss = Enemy.Spawn(data, new Vector2(PlayArea.Center.x, PlayArea.Top + 4f), EnemySpawnInfo.Default, bossScale);
            if (boss == null) yield break;

            BossActive = true;
            float escortTimer = 9f;

            while (boss.IsAlive)
            {
                // Small escort groups keep the pressure up (and the pickups coming) during the fight.
                escortTimer -= Time.deltaTime;
                if (escortTimer <= 0f)
                {
                    escortTimer = Random.Range(7f, 10f);
                    EnemyData escort = PickEnemy(float.MaxValue, 2f);
                    if (escort != null) StartCoroutine(SpawnGroup(escort, Random.Range(3, 6), waveScale));
                }
                yield return null;
            }

            BossActive = false;
            yield return new WaitForSeconds(1.5f);
        }

        DifficultyScale ScaleFor(int wave)
        {
            int steps = Mathf.Max(0, wave - 1);
            return new DifficultyScale(
                1f + healthPerWave * steps,
                Mathf.Min(maxSpeedScale, 1f + speedPerWave * steps),
                Mathf.Min(maxBulletSpeedScale, 1f + bulletSpeedPerWave * steps),
                Mathf.Min(maxDamageScale, 1f + damagePerWave * steps));
        }
    }
}
