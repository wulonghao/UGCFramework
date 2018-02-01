using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PoolManagerNs
{
    /// <summary>
    /// 对象池管理器
    /// </summary>
    public static class PoolManager
    {
        private static Dictionary<string, PoolEntity> poolList = new Dictionary<string, PoolEntity>();

        public static T CreatePoolEntity<T>(string name, Transform parent = null) where T : PoolEntity
        {
            PoolEntity pool = null;
            poolList.TryGetValue(name, out pool);
            if (pool == null)
            {
                GameObject newGo = new GameObject(name);
                if (parent)
                    newGo.transform.SetParent(parent);
                pool = newGo.AddComponent<T>();
                poolList.Add(name, pool);
            }
            return pool as T;
        }

        public static T CreatePoolEntity<T>(GameObject go) where T : PoolEntity
        {
            PoolEntity pool = null;
            poolList.TryGetValue(go.name, out pool);
            if (pool == null)
            {
                pool = go.AddComponent<T>();
                poolList.Add(go.name, pool);
            }
            return pool as T;
        }

        public static void DestroyPool(string poolName)
        {
            PoolEntity singleton = null;
            poolList.TryGetValue(poolName, out singleton);
            if (singleton != null)
            {
                poolList.Remove(poolName);
                if (singleton)
                    GameObject.DestroyImmediate(singleton.gameObject);
            }
        }

        public static void Clear()
        {
            List<string> keys = new List<string>();
            keys.AddRange(poolList.Keys);
            foreach (var key in keys)
                DestroyPool(key);
        }
    }
}