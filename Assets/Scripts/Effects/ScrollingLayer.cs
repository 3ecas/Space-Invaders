using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Scrolls a stack of identical tiles downwards forever: when a tile drops off the bottom it
    /// jumps back to the top of the stack. Used for the nebula backdrops.
    /// </summary>
    public sealed class ScrollingLayer : MonoBehaviour
    {
        [Tooltip("Speed compared to the world scroll speed. Small = far away.")]
        [SerializeField] float speedFactor = 0.15f;
        [Tooltip("Height of one tile in world units. Must be at least as tall as the screen.")]
        [SerializeField] float tileHeight = 48f;
        [SerializeField] Transform[] tiles;

        void Update()
        {
            if (tiles == null || tiles.Length == 0) return;

            float distance = WorldScroll.Speed * speedFactor * Time.deltaTime;
            float stackHeight = tileHeight * tiles.Length;

            foreach (Transform tile in tiles)
            {
                if (tile == null) continue;
                Vector3 position = tile.localPosition;
                position.y -= distance;
                if (position.y <= -tileHeight) position.y += stackHeight;
                tile.localPosition = position;
            }
        }
    }
}
