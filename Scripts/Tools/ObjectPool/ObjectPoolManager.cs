using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Tools.ObjectPool
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        public List<ObjectPool> InitPools;
        
        private Dictionary<string, ObjectPool> pools;

        protected override void Awake()
        {
            pools = new Dictionary<string, ObjectPool>();
        }

        private void Start()
        {
            if (InitPools == null)
                return;
            
            foreach (var pool in InitPools)
                if (pool.Prefab != null)
                    if (string.IsNullOrEmpty(pool.Name))
                        pools[pool.Prefab.name] = pool;
                    else
                        pools[pool.Name] = pool;
        }
        
        private void OnValidate()
        {
            if (InitPools == null)
                return;
            
            foreach (var pool in InitPools)
            {
                if (string.IsNullOrEmpty(pool.Name) && pool.Prefab != null)
                    pool.Name = pool.Prefab.name;

                if (pool.MaxCount < 0)
                    pool.MaxCount = 0;
                
                if (pool.StartCount < 0)
                    pool.StartCount = 0;
                
                if (pool.StartCount > pool.MaxCount)
                    pool.StartCount = pool.MaxCount;
            }
        }

        /// <summary>
        /// Получает свободный объект по имени.
        /// </summary>
        /// <param name="objectName">Имя или алиас объекта</param>
        /// <returns>Объект, если такой найден, иначе <see langword="null" /></returns>
        public GameObject GetObject(string objectName)
        {
            if (!pools.ContainsKey(objectName))
                return null;
            
            return pools[objectName].Get();
        }

        /// <summary>
        /// Создаёт новый пул для указанного префаба. Если пул уже существует, добавляет объект в него.
        /// </summary>
        /// <param name="obj">Добавляемый объект</param>
        /// <param name="name">Алиас имени объекта</param>
        /// <param name="deactivate">Нужно ли сделать объект неактивным после добавления</param>
        public void RegisterObject(GameObject obj, string name = null, bool deactivate = true)
        {
            if (string.IsNullOrEmpty(name)) 
                name = obj.name;
            
            if (!pools.ContainsKey(name))
                pools[name] = new ObjectPool();
            pools[name].Add(obj, deactivate);
        }
    }
}
