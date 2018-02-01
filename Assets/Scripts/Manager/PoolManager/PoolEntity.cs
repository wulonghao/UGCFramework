using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PoolManagerNs
{
    public class PoolEntity : PoolBase<PoolUnit>
    {
        protected Transform _workParent;
        public Transform workParent { get { return _workParent; } }

        protected Transform _idleParent;
        public Transform idleParent { get { return _idleParent; } }

        void Awake()
        {
            if (_workParent == null)
                _workParent = UIUtils.CreateGameObject(transform, "work").transform;
            if (_idleParent == null)
            {
                _idleParent = UIUtils.CreateGameObject(transform, "idle").transform;
                _idleParent.gameObject.SetActive(false);
            }
        }

        public void OnUnitChangePool(PoolUnit unit)
        {
            if (unit != null)
            {
                if (unit.unitStatu == PoolUnitStatuType.Idle)
                    unit.transform.SetParent(_idleParent);
                else if (unit.unitStatu == PoolUnitStatuType.Work)
                    unit.transform.SetParent(_workParent);
            }
        }

        protected override PoolUnitList<T> CreateNewUnitList<T>()
        {
            PoolUnitList<T> list = new PoolUnitList<T>();
            list.SetPool(this);
            return list;
        }
    }
}