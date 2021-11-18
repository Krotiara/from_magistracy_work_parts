using System;
using UnityEngine;

namespace CableWalker.Simulator.Tools.ObjectPool
{
    [DisallowMultipleComponent]
    public class PoolableObject : MonoBehaviour
    {
        public string Name;

        [NonSerialized]
        public bool Registered = false;
        
        private void Awake()
        {
            if (!Registered)
                ObjectPoolManager.Instance.RegisterObject(gameObject, Name);
        }

        /// <summary>
        /// Возвращает объект в пул.
        /// </summary>
        public void BackToPool()
        {
            gameObject.SetActive(false);
            transform.parent = ObjectPoolManager.Instance.transform;
        }
    }
}
