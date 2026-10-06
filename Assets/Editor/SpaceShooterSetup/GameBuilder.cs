using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// The entry point of the setup tools (menu: Tools > Space Shooter).
    /// "Rebuild Game" regenerates the placeholder art, sounds, prefabs, data assets and the game
    /// scene from scratch. It is how the project was first put together, and it is safe to run
    /// again - but it OVERWRITES those generated files, so any changes you made to them by hand
    /// are lost. Your own scripts, scenes and assets are never touched.
    /// </summary>
    public static class GameBuilder
    {
        [MenuItem("Tools/Space Shooter/Rebuild Game (overwrites generated assets)", priority = 100)]
        static void RebuildFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Space Shooter", "Stop Play mode first.", "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Rebuild Space Shooter?",
                "This regenerates the sprites, sounds, prefabs, data assets and the Game scene.\n\n" +
                "Any changes you made to those generated files will be overwritten.",
                "Rebuild", "Cancel");
            if (!confirmed) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Rebuild();
        }

        [MenuItem("Tools/Space Shooter/Open Game Scene", priority = 0)]
        static void OpenGameScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(SceneFactory.ScenePath);
            }
        }

        // Held in a static field while building so Unity doesn't unload the assets mid-build.
        static GameAssets building;
        static Dictionary<SfxId, AudioClip> buildingClips;

        /// <summary>Runs every build step in order. Returns a short summary.</summary>
        public static string Rebuild()
        {
            try
            {
                // Open the empty scene first: switching scenes makes Unity unload assets nobody is
                // using yet, so it must happen before the build starts collecting references.
                // Prefabs are assembled in this scene as temporary objects, then the game is built in it.
                Step("Preparing", 0.01f);
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                building = new GameAssets();

                Step("Project settings", 0.03f);
                ProjectSetup.Apply();

                Step("Drawing sprites", 0.08f);
                SpriteFactory.BuildAll();

                Step("Synthesising sounds", 0.45f);
                buildingClips = SfxFactory.BuildAll();

                Step("Creating data assets", 0.55f);
                DataFactory.CreateAssets(building);

                Step("Building prefabs", 0.62f);
                PrefabFactory.BuildAll(building);

                Step("Filling in data", 0.8f);
                DataFactory.Fill(building, buildingClips);
                AssetDatabase.SaveAssets();

                Step("Building the scene", 0.88f);
                SceneFactory.Build(building, scene);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                building = null;
                buildingClips = null;
                EditorUtility.ClearProgressBar();
            }

            const string summary = "Space Shooter built. Open Assets/Scenes/Game.unity and press Play.";
            Debug.Log(summary);
            return summary;
        }

        static void Step(string label, float progress)
        {
            EditorUtility.DisplayProgressBar("Building Space Shooter", label + "...", progress);
        }
    }
}
