using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PoolManagerNs
{
    public abstract class PoolBase<T> : MonoBehaviour where T : PoolUnit
    {
        /// <summary>
        /// 缓冲池，按类型存放各自分类列表
        /// </summary>
        private Dictionary<string, PoolUnitList<T>> poolTale = new Dictionary<string, PoolUnitList<T>>();

        /// <summary>
        /// 获取一个闲置的单元
        /// </summary>
        public TT GetIdleUnit<TT>(string key = null) where TT : T
        {
            PoolUnitList<T> list = GetTargetList<TT>(key);
            return list.GetIdleUnit<TT>() as TT;
        }

        /// <summary>
        /// 在缓冲池中获取指定单元类型的列表，
        /// 如果该单元类型不存在，则立刻创建。
        /// </summary>
        /// <returns>单元列表</returns>
        public PoolUnitList<T> GetTargetList<TT>(string key = null) where TT : T
        {
            if (string.IsNullOrEmpty(key))
                key = typeof(T).ToString();
            PoolUnitList<T> list = null;
            poolTale.TryGetValue(key, out list);
            if (list == null)
            {
                list = CreateNewUnitList<T>();
                poolTale.Add(key, list);
            }
            return list;
        }

        protected abstract PoolUnitList<TT> CreateNewUnitList<TT>() where TT : T;
    }
}