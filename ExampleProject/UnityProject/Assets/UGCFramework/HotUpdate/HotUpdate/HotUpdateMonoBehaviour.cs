using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;
using UnityObject = UnityEngine.Object;

namespace UGCF.HotUpdate
{
    //此类等同于unity中的MonoBehaviour，可以热更，请将此类放到热更的类库项目中
    public class HotUpdateMonoBehaviour
    {
        [HideInInspector] public HotUpdateMono hotFixInstance;
        public string name
        {
            get
            {
                if (hotFixInstance != null)
                    return hotFixInstance.name;
                return null;
            }
            set
            {
                if (hotFixInstance != null)
                    hotFixInstance.name = value;
            }
        }
        public GameObject gameObject
        {
            get
            {
                if (hotFixInstance != null)
                    return hotFixInstance.gameObject;
                return null;
            }
        }
        public Transform transform
        {
            get
            {
                if (hotFixInstance != null)
                    return hotFixInstance.transform;
                return null;
            }
        }

        RectTransform _rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null && hotFixInstance != null)
                    _rectTransform = hotFixInstance.GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
        public bool enabled
        {
            get
            {
                if (hotFixInstance != null)
                    return hotFixInstance.enabled;
                return false;
            }
            set
            {
                if (hotFixInstance != null)
                    hotFixInstance.enabled = value;
            }
        }

        public void InitHotFixMonoBehaviour(HotUpdateMono hotFixInstance)
        {
            this.hotFixInstance = hotFixInstance;
        }

        #region ...Component
        public T AddILComponent<T>() where T : HotUpdateMonoBehaviour
        {
            HotUpdateMono hotFixMono = gameObject.AddComponent<HotUpdateMono>();
            if (hotFixMono.InitHotUpdateMono(typeof(T).FullName) != null)
                return (T)hotFixMono.HotUpdateInstance;
            return null;
        }
        public T GetILComponent<T>()
        {
            HotUpdateMono[] monos = hotFixInstance.GetComponents<HotUpdateMono>();
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    return (T)instance.HotUpdateInstance;
            }
            return default;
        }
        public T GetILComponentInChildren<T>(bool includeInactive = false)
        {
            if (!hotFixInstance)
                return default;
            HotUpdateMono[] monos = hotFixInstance.GetComponentsInChildren<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    return (T)instance.HotUpdateInstance;
            }
            return default;
        }
        public T[] GetILComponentsInChildren<T>(bool includeInactive = false)
        {
            if (!hotFixInstance)
                return null;
            List<T> ts = new List<T>();
            HotUpdateMono[] monos = hotFixInstance.GetComponentsInChildren<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    ts.Add((T)instance.HotUpdateInstance);
            }
            return ts.ToArray();
        }
        public T GetILComponentInParent<T>(bool includeInactive = false)
        {
            if (!hotFixInstance)
                return default;
            HotUpdateMono[] monos = hotFixInstance.GetComponentsInParent<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    return (T)instance.HotUpdateInstance;
            }
            return default;
        }
        public T[] GetILComponentsInParent<T>(bool includeInactive = false)
        {
            if (!hotFixInstance)
                return null;
            List<T> ts = new List<T>();
            HotUpdateMono[] monos = hotFixInstance.GetComponentsInParent<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    ts.Add((T)instance.HotUpdateInstance);
            }
            return ts.ToArray();
        }
        #endregion

        #region ...Detroy
        public static void Destroy(HotUpdateMonoBehaviour hotFixMB)
        {
            if (hotFixMB && hotFixMB.hotFixInstance)
                UnityObject.Destroy(hotFixMB.hotFixInstance);
        }
        public static void Destroy(HotUpdateMonoBehaviour hotFixMB, [DefaultValue("0.0F")] float t)
        {
            if (hotFixMB && hotFixMB.hotFixInstance)
                UnityObject.Destroy(hotFixMB.hotFixInstance, t);
        }
        public static void DestroyImmediate(HotUpdateMonoBehaviour hotFixMB)
        {
            if (hotFixMB && hotFixMB.hotFixInstance)
                UnityObject.DestroyImmediate(hotFixMB.hotFixInstance);
        }
        public static void DestroyImmediate(HotUpdateMonoBehaviour hotFixMB, [DefaultValue("false")] bool allowDestroyingAssets)
        {
            if (hotFixMB && hotFixMB.hotFixInstance)
                UnityObject.DestroyImmediate(hotFixMB.hotFixInstance, allowDestroyingAssets);
        }
        public static void DontDestroyOnLoad(HotUpdateMonoBehaviour hotFixMB)
        {
            if (hotFixMB && hotFixMB.hotFixInstance)
                UnityObject.DontDestroyOnLoad(hotFixMB.hotFixInstance);
        }
        #endregion

        #region ...Instantiate
        public static HotUpdateMonoBehaviour Instantiate(HotUpdateMonoBehaviour original)
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance);
            return (HotUpdateMonoBehaviour)hotFixMono.HotUpdateInstance;
        }
        public static HotUpdateMonoBehaviour Instantiate(HotUpdateMonoBehaviour original, Transform parent)
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance, parent);
            return (HotUpdateMonoBehaviour)hotFixMono.HotUpdateInstance;
        }
        public static HotUpdateMonoBehaviour Instantiate(HotUpdateMonoBehaviour original, Vector3 position, Quaternion rotation)
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance, position, rotation);
            return (HotUpdateMonoBehaviour)hotFixMono.HotUpdateInstance;
        }
        public static HotUpdateMonoBehaviour Instantiate(HotUpdateMonoBehaviour original, Vector3 position, Quaternion rotation, Transform parent)
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance, position, rotation, parent);
            return (HotUpdateMonoBehaviour)hotFixMono.HotUpdateInstance;
        }
        public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : HotUpdateMonoBehaviour
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance, parent, worldPositionStays);
            return (T)hotFixMono.HotUpdateInstance;
        }
        public static T Instantiate<T>(T original, Transform parent) where T : HotUpdateMonoBehaviour
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance, parent);
            return (T)hotFixMono.HotUpdateInstance;
        }
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : HotUpdateMonoBehaviour
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance, position, rotation, parent);
            return (T)hotFixMono.HotUpdateInstance;
        }
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : HotUpdateMonoBehaviour
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance, position, rotation);
            return (T)hotFixMono.HotUpdateInstance;
        }
        public static T Instantiate<T>(T original) where T : HotUpdateMonoBehaviour
        {
            HotUpdateMono hotFixMono = UnityObject.Instantiate(original.hotFixInstance);
            return (T)hotFixMono.HotUpdateInstance;
        }
        #endregion

        #region ...Coroutine
        public Coroutine StartCoroutine(string methodName)
        {
            if (hotFixInstance)
                return hotFixInstance.StartCoroutine(methodName);
            return null;
        }
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (hotFixInstance)
                return hotFixInstance.StartCoroutine(routine);
            return null;
        }
        public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
        {
            if (hotFixInstance)
                return hotFixInstance.StartCoroutine(methodName, value);
            return null;
        }
        public void StopAllCoroutines()
        {
            if (hotFixInstance)
                hotFixInstance.StopAllCoroutines();
        }
        public void StopCoroutine(IEnumerator routine)
        {
            if (hotFixInstance)
                hotFixInstance.StopCoroutine(routine);
        }
        public void StopCoroutine(Coroutine routine)
        {
            if (hotFixInstance)
                hotFixInstance.StopCoroutine(routine);
        }
        public void StopCoroutine(string methodName)
        {
            if (hotFixInstance)
                hotFixInstance.StopCoroutine(methodName);
        }
        #endregion

        public static implicit operator bool(HotUpdateMonoBehaviour exists)
        {
            return exists != null;
        }
    }
}
