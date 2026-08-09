#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace FTF.Editor
{
    /// <summary>
    /// Custom Unity Editor menu shortcuts for navigating Fruited To Fight scenes.
    /// Adds top menu items under 'FTF'.
    /// </summary>
    public static class FTFSceneMenu
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string UIScenePath = "Assets/Scenes/UI.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        /// <summary>
        /// Opens the Gameplay scene in Unity Editor.
        /// </summary>
        [MenuItem("FTF/Open Gameplay Scene", false, 1)]
        private static void OpenGameplayScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            }
        }

        /// <summary>
        /// Opens the UI scene in Unity Editor.
        /// </summary>
        [MenuItem("FTF/Open UI Scene", false, 2)]
        private static void OpenUIScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(UIScenePath, OpenSceneMode.Single);
            }
        }

        /// <summary>
        /// Opens the Main Menu scene in Unity Editor.
        /// </summary>
        [MenuItem("FTF/Open Main Menu Scene", false, 3)]
        private static void OpenMainMenuScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            }
        }
    }
}
#endif
