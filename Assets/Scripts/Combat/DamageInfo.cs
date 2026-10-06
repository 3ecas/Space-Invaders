using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Everything a target might want to know about a hit.</summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly Vector2 Point;
        public readonly Vector2 Direction;
        public readonly Team Source;

        public DamageInfo(float amount, Vector2 point, Vector2 direction, Team source)
        {
            Amount = amount;
            Point = point;
            Direction = direction;
            Source = source;
        }
    }
}
