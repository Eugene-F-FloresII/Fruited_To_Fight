using Shared.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        public void PlayGame()
        {
            Events_Game.OnSceneChange?.Invoke("Gameplay");
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            Application.OpenURL("about:blank");
#else
            Application.Quit();
#endif
            Debug.Log("Exiting Game");
        }
    }

}
