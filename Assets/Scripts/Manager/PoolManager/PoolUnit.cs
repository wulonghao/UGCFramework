using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PoolManagerNs
{
    public class PoolUnit : MonoBehaviour
    {
        protected float destroyTimer = 10;
        [HideInInspector]
        //单元状态对象
        public PoolUnitStatuType unitStatu;
        //父列表对象
        PoolUnitList<PoolUnit> parentList;

        /// <summary>
        /// 接受父列表对象的设置
        /// </summary>
        /// <param name="parentList">父列表对象</param>
        public virtual void SetParentList(object parentList)
        {
            this.parentList = parentList as PoolUnitList<PoolUnit>;
        }

        /// <summary>
        /// 归还自己，即将自己回收以便再利用
        /// </summary>
        public virtual void Restore()
        {
            if (parentList != null)
            {
                //idleTime = Time.realtimeSinceStartup;
                parentList.RestoreUnit(this);
            }
        }
    }

    public enum PoolUnitStatuType
    {
        /// <summary> 闲置状态 </summary>
        Idle,
        /// <summary> 工作状态 </summary>
        Work
    }
}