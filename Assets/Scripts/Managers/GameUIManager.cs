using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _userInterfaceGameObjects;
        private void Awake()
        {
            if (_userInterfaceGameObjects != null)
            {
                foreach (GameObject go in _userInterfaceGameObjects)
                {
                    DontDestroyOnLoad(go);
                }
            }
            else
            {
                Debug.LogError("No GameObjects in" + gameObject.name);
            }
            
        }
    }

}
