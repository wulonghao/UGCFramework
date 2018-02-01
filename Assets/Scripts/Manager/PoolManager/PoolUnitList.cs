using PoolManagerNs;
using System.Collections.Generic;
using UnityEngine;
namespace PoolManagerNs
{
    public class PoolUnitList<T> where T : PoolUnit
    {
        object template;
        List<T> idleList;
        List<T> workList;
        int m_createdNum = 0;
        PoolEntity pool;

        public PoolUnitList()
        {
            idleList = new List<T>();
            workList = new List<T>();
        }

        /// <summary>
        /// 设置预制体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        public void SetPrefab(object prefab)
        {
            this.template = prefab;
        }

        public void SetPool(PoolEntity pool)
        {
            this.pool = pool;
        }

        /// <summary>
        /// 从列表尾部获取一个闲置的单元，如果不存在则创建一个新的
        /// </summary>
        /// <returns>闲置单元</returns>
        public TT GetIdleUnit<TT>() where TT : T
        {
            TT unit = null;
            if (idleList.Count > 0)
            {
                while (idleList.Count > 0 && idleList[idleList.Count - 1] == null)
                    idleList.RemoveAt(idleList.Count - 1);
                if (idleList.Count > 0)
                {
                    unit = (TT)idleList[idleList.Count - 1];
                    idleList.RemoveAt(idleList.Count - 1);
                }
            }
            if (unit == null)
            {
                unit = CreateNewUnit<TT>();
                unit.SetParentList(this);
                m_createdNum++;
            }
            workList.Add(unit);
            unit.unitStatu = PoolUnitStatuType.Work;
            OnUnitChangePool(unit);
            return unit;
        }

        /// <summary>
        /// 闲置某个工作中的单元
        /// </summary>
        /// <param name="unit">单元</param>
        public void RestoreUnit(T unit)
        {
            if (unit != null && unit.unitStatu == PoolUnitStatuType.Work)
            {
                workList.Remove(unit);
                idleList.Add(unit);
                unit.unitStatu = PoolUnitStatuType.Idle;
                OnUnitChangePool(unit);
            }
        }

        protected TT CreateNewUnit<TT>() where TT : T
        {
            GameObject result_go = null;
            if (template != null && template is GameObject)
                result_go = GameObject.Instantiate((GameObject)template);
            else
            {
                result_go = new GameObject();
                result_go.name = typeof(TT).ToString();
            }
            result_go.name = result_go.name + "_" + m_createdNum;
            TT comp = result_go.GetComponent<TT>();
            if (comp == null)
                comp = result_go.AddComponent<TT>();
            return comp;
        }

        protected void OnUnitChangePool(T unit)
        {
            if (pool != null)
            {
                pool.OnUnitChangePool(unit);
            }
        }
    }
}