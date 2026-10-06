using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Base class for the building blocks of an enemy (how it moves, how it shoots...).
    /// The <see cref="Enemy"/> on the same object drives these: Begin when it spawns,
    /// Tick every frame while alive, End when it dies or leaves.
    /// </summary>
    public abstract class EnemyBehaviour : MonoBehaviour
    {
        protected Enemy Owner { get; private set; }

        public void Begin(Enemy owner)
        {
            Owner = owner;
            OnBegin();
        }

        /// <summary>Reset state here - enemies are pooled, so this runs once per life.</summary>
        protected virtual void OnBegin() { }

        public virtual void Tick(float deltaTime) { }

        public virtual void End() { }
    }
}
