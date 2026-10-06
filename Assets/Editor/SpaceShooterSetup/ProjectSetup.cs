using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>Project-wide settings the game depends on: physics layers and which layers interact.</summary>
    public static class ProjectSetup
    {
        public static void Apply()
        {
            int player = EnsureLayer(GameLayers.PlayerName, 6);
            int enemy = EnsureLayer(GameLayers.EnemyName, 7);
            if (player < 0 || enemy < 0) return;

            // Only player-versus-enemy overlaps matter. Skipping enemy-versus-enemy checks saves a
            // lot of physics work when the screen is full.
            Physics2D.IgnoreLayerCollision(enemy, enemy, true);
            Physics2D.IgnoreLayerCollision(player, player, true);
            Physics2D.IgnoreLayerCollision(player, enemy, false);

            AssetDatabase.SaveAssets();
        }

        /// <summary>Returns the index of the named layer, creating it in a free slot if needed.</summary>
        static int EnsureLayer(string name, int preferredSlot)
        {
            int existing = LayerMask.NameToLayer(name);
            if (existing >= 0) return existing;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError("[Space Shooter setup] Could not open the Tags and Layers settings.");
                return -1;
            }

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            int slot = -1;
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(preferredSlot).stringValue))
            {
                slot = preferredSlot;
            }
            else
            {
                for (int i = 8; i < layers.arraySize; i++)
                {
                    if (!string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) continue;
                    slot = i;
                    break;
                }
            }

            if (slot < 0)
            {
                Debug.LogError($"[Space Shooter setup] No free physics layer for '{name}'.");
                return -1;
            }

            layers.GetArrayElementAtIndex(slot).stringValue = name;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            return slot;
        }
    }
}
