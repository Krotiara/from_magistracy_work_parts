using System;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Tools.ObjectPool
{
    [Serializable]
    public class ObjectPool
    {
        private readonly List<GameObject> pooledObjects;
        
        public string Name;
        public GameObject Prefab;
        public int StartCount;
        public int MaxCount;

        public ObjectPool()
        {
            pooledObjects = new List<GameObject>();

            for (var i = 0; i < StartCount; i++)
                Extend();
        }

        public bool Add(GameObject obj, bool deactivate = true)
        {
            if (pooledObjects.Count >= MaxCount)
                return false;
            
            if (deactivate)
                obj.SetActive(false);

            var poolableObject = obj.GetComponent<PoolableObject>();
            if (poolableObject == null)
            {
                poolableObject = obj.AddComponent<PoolableObject>();
                poolableObject.Registered = true;
            }
            
            pooledObjects.Add(obj);
            return true;
        }
        
        public GameObject Get()
        {
            foreach (var obj in pooledObjects)
                if (!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                    return obj;
                }
            
            Debug.LogWarning($"Pool {Name} was extended.");
            var newObj = Extend();
            newObj.SetActive(true);
            return newObj;
        }

        private GameObject Extend()
        {
            if (Prefab == null)
                return null;
            
            if (pooledObjects.Count + 1 > MaxCount)
                return null;
            
            var obj = GameObject.Instantiate(Prefab);
            Add(obj);
            return obj;
        }
    }
}
