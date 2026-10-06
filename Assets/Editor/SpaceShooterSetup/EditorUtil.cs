using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>Small helpers shared by the setup tools.</summary>
    public static class EditorUtil
    {
        /// <summary>Creates a project folder (and any missing parents), e.g. "Assets/Art/Sprites".</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>
        /// Loads the ScriptableObject at a path, or creates it if it doesn't exist yet.
        /// Reusing the existing asset keeps references to it intact when the game is rebuilt.
        /// </summary>
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>Saves a scene object as a prefab asset and removes the temporary object.</summary>
        public static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>
        /// Sets a serialized field by name - the scripted equivalent of typing a value into the
        /// Inspector. Works on private [SerializeField] fields. Arrays and lists are supported.
        /// </summary>
        public static void Set(Object target, string field, object value)
        {
            if (target == null)
            {
                Debug.LogError($"[Space Shooter setup] Tried to set '{field}' on a missing object.");
                return;
            }

            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[Space Shooter setup] {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }

            Assign(property, value, target.GetType().Name + "." + field);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Sets several fields at once: Set(target, "speed", 5f, "damage", 10f).</summary>
        public static void SetMany(Object target, params object[] fieldValuePairs)
        {
            for (int i = 0; i + 1 < fieldValuePairs.Length; i += 2)
            {
                Set(target, (string)fieldValuePairs[i], fieldValuePairs[i + 1]);
            }
        }

        public static void Assign(SerializedProperty property, object value, string label)
        {
            switch (value)
            {
                case null:
                    property.objectReferenceValue = null;
                    break;
                case bool b:
                    property.boolValue = b;
                    break;
                case int i:
                    if (property.propertyType == SerializedPropertyType.Float) property.floatValue = i;
                    else property.intValue = i;
                    break;
                case float f:
                    if (property.propertyType == SerializedPropertyType.Integer) property.intValue = Mathf.RoundToInt(f);
                    else property.floatValue = f;
                    break;
                case string s:
                    property.stringValue = s;
                    break;
                case Color c:
                    property.colorValue = c;
                    break;
                case Vector2 v2:
                    property.vector2Value = v2;
                    break;
                case Vector3 v3:
                    property.vector3Value = v3;
                    break;
                case Vector2Int v2i:
                    property.vector2IntValue = v2i;
                    break;
                case System.Enum e:
                    property.intValue = System.Convert.ToInt32(e);
                    break;
                case Object o:
                    property.objectReferenceValue = o;
                    break;
                case IList list:
                    property.arraySize = list.Count;
                    for (int index = 0; index < list.Count; index++)
                    {
                        Assign(property.GetArrayElementAtIndex(index), list[index], label + "[" + index + "]");
                    }
                    break;
                default:
                    Debug.LogError($"[Space Shooter setup] Don't know how to assign a {value.GetType().Name} to {label}.");
                    break;
            }
        }
    }
}
