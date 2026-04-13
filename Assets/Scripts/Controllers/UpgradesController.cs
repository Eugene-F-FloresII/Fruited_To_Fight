using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Managers;
using Obvious.Soap;
using PrimeTween;
using Shared.Enums;
using Shared.Events;

namespace Controllers
{
    public class UpgradesController : MonoBehaviour
    {
        [SerializeField] private IntVariable _seedCollected;
        
        private UpgradesManager _upgradesManager;
    }
    
}
