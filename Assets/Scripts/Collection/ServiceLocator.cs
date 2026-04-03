using System;
using System.Collections.Generic;
using UnityEngine;

namespace Collection
{

    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        public static void Register<T>(T service) where T: class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type.Name} already registered. Overwriting.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
            }
        }
        
        /// <summary>
        /// Get a registered service
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            
            Debug.LogError($"Service of type {type.Name} not found!");
            return null;
        }
        
        /// <summary>
        /// Unregister a service (useful for cleanup)
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }
        
        /// <summary>
        /// Clear all services (useful when changing scenes)
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
        
    }
}