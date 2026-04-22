using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName =  "Data/LoadingScreenTextConfig", fileName =  "LoadingScreenTextConfig")]
    public class LoadingScreenTextConfig : ScriptableObject
    {
       [TextArea] public List<string> Texts;
        
    }

}
