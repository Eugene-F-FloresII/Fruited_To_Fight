using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers 
{
    public class GameSceneManager : MonoBehaviour
    {
        void Start()
        {
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }
    
    }
}