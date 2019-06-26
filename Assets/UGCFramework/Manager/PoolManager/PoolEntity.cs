using System.Collections.Generic;
using UnityEngine;

namespace PoolManager
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
                _workParent = CreateGameObject(transform, "work").transform;
            if (_idleParent == null)
            {
                _idleParent = CreateGameObject(transform, "idle").transform;
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

        GameObject CreateGameObject(Transform parent, string name = null, Vector3 position = default(Vector3), Vector3 angle = default(Vector3))
        {
            GameObject go = new GameObject();
            if (name == null)
                go.name = typeof(GameObject).ToString();
            else
                go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = angle;
            return go;
        }
    }
}