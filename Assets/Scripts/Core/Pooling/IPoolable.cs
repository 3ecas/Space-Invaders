namespace SpaceShooter
{
    /// <summary>
    /// Implement on any component of a pooled prefab to reset its state each time it is reused.
    /// Pooled objects are recycled, so Awake/Start only run once - put per-life setup in OnSpawned.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
