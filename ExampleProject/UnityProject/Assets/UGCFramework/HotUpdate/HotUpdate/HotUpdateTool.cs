using UGCF.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UGCF.HotUpdate
{
    public static class HotUpdateTool
    {
        public static HotUpdateMonoBehaviour InitHotFixMono(HotUpdateMono hotUpdateMono, string hotUpdateClassFullName, List<HotUpdateField> hotUpdateFields)
        {
            if (string.IsNullOrEmpty(hotUpdateClassFullName))
                return null;
            HotUpdateMonoBehaviour hotUpdateInstance = null;
            Type targetType = Type.GetType(hotUpdateClassFullName);
            if (targetType != null)
                hotUpdateInstance = targetType.GetConstructor(new Type[0]).Invoke(null) as HotUpdateMonoBehaviour;
            if (hotUpdateInstance == null)
                return hotUpdateInstance;
            for (int i = 0; i < hotUpdateFields.Count; i++)
            {
                HotUpdateField hotUpdateField = hotUpdateFields[i];
                if (string.IsNullOrEmpty(hotUpdateField.fieldName))
                    continue;
                FieldInfo fieldInfo = targetType.GetFieldContainParent(hotUpdateField.fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    if (typeof(HotUpdateMonoBehaviour).IsAssignableFrom(fieldInfo.FieldType))
                        fieldInfo.SetValue(hotUpdateInstance, ((HotUpdateMono)hotUpdateField.valueUnityObj).HotUpdateInstance);
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        if (hotUpdateField.valueUnityObj != null)
                            fieldInfo.SetValue(hotUpdateInstance, hotUpdateField.valueUnityObj);
                    }
                    else
                        FieldSetValue(fieldInfo, hotUpdateInstance, hotUpdateField.valueStr);
                }
            }
            hotUpdateInstance.InitHotFixMonoBehaviour(hotUpdateMono);
            return hotUpdateInstance;
        }

        static void FieldSetValue(FieldInfo fieldInfo, object obj, string value)
        {
            if (fieldInfo.FieldType.Equals(typeof(string)))
                fieldInfo.SetValue(obj, value);
            else if (fieldInfo.FieldType.Equals(typeof(char)))
                fieldInfo.SetValue(obj, char.Parse(value));
            else if (fieldInfo.FieldType.Equals(typeof(long)))
                fieldInfo.SetValue(obj, long.Parse(value));
            else if (fieldInfo.FieldType.Equals(typeof(short)))
                fieldInfo.SetValue(obj, short.Parse(value));
            else if (fieldInfo.FieldType.Equals(typeof(int)))
                fieldInfo.SetValue(obj, int.Parse(value));
            else if (fieldInfo.FieldType.Equals(typeof(float)))
                fieldInfo.SetValue(obj, float.Parse(value));
            else if (fieldInfo.FieldType.Equals(typeof(double)))
                fieldInfo.SetValue(obj, double.Parse(value));
            else if (fieldInfo.FieldType.Equals(typeof(bool)))
                fieldInfo.SetValue(obj, bool.Parse(value));
            else if (fieldInfo.FieldType.Equals(typeof(Vector2)))
                fieldInfo.SetValue(obj, MiscUtils.GetVector2ByString(value));
            else if (fieldInfo.FieldType.Equals(typeof(Vector3)))
                fieldInfo.SetValue(obj, MiscUtils.GetVector3ByString(value));
            else if (fieldInfo.FieldType.Equals(typeof(Vector4)))
                fieldInfo.SetValue(obj, MiscUtils.GetVector4ByString(value));
            else if (typeof(Enum).IsAssignableFrom(fieldInfo.FieldType))
                fieldInfo.SetValue(obj, Enum.Parse(fieldInfo.FieldType, value));
            else
                fieldInfo.SetValue(obj, value);
        }

        /// <summary>
        /// 获取指定对象的指定私有字段的值
        /// </summary>
        /// <param name="instance">指定的对象</param>
        /// <param name="fieldName">指定的私有字段名</param>
        /// <returns>指定字段的值</returns>
        public static object GetPrivateField(object instance, string fieldName)
        {
            FieldInfo fi = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return fi.GetValue(instance);
        }

        /// <summary>
        /// 设置指定对象的指定私有字段的值
        /// </summary>
        /// <param name="instance">指定的对象</param>
        /// <param name="fieldName">指定的私有字段名</param>
        /// <param name="value">要设置的值</param>
        public static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo fi = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(instance, value);
        }

        /// <summary>
        /// 执行指定对象的指定私有函数
        /// </summary>
        /// <param name="instance">指定的对象</param>
        /// <param name="methodName">指定的私有函数名</param>
        /// <param name="value">函数的所有参数</param>
        /// <returns>函数的返回值</returns>
        public static object InvokePrivateMethod(object instance, string methodName, params object[] value)
        {
            MethodInfo mi = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return mi.Invoke(instance, value);
        }

        public static bool IsInheritTargetType(this Type type, string targetTypeFullName)
        {
            if (type.FullName == targetTypeFullName)
                return true;
            else if (type.BaseType != null && type.BaseType.FullName == targetTypeFullName)
                return true;
            else
            {
                if (type.BaseType == null)
                    return false;
                else
                    return type.BaseType.IsInheritTargetType(targetTypeFullName);
            }
        }

        public static FieldInfo GetFieldContainParent(this Type type, string fieldName, BindingFlags bindingFlags)
        {
            FieldInfo fieldInfo = type.GetField(fieldName, bindingFlags);
            if (fieldInfo == null && type.BaseType != null)
                return type.BaseType.GetFieldContainParent(fieldName, bindingFlags);
            else
                return fieldInfo;
        }

        public static List<FieldInfo> GetFieldsContainParent(this Type type, BindingFlags bindingFlags)
        {
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            if (type.BaseType != null)
                fieldInfos.AddRange(type.BaseType.GetFieldsContainParent(bindingFlags));
            fieldInfos.AddRange(type.GetFields(bindingFlags));
            List<FieldInfo> fieldInfo2s = new List<FieldInfo>();
            for (int i = 0; i < fieldInfos.Count; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                if (fieldInfo2s.FindIndex((fi) => { return fi.Name == fieldInfo.Name; }) < 0)
                    fieldInfo2s.Add(fieldInfo);
            }
            return fieldInfo2s;
        }

        #region ...Component
        public static T GetILComponent<T>(this GameObject gameObject)
        {
            HotUpdateMono[] monos = gameObject.GetComponents<HotUpdateMono>();
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    return (T)instance.HotUpdateInstance;
            }
            return default;
        }

        public static HotUpdateMonoBehaviour GetILComponent(this GameObject gameObject, string typeFullName)
        {
            HotUpdateMono[] monos = gameObject.GetComponents<HotUpdateMono>();
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (instance.HotUpdateInstance.GetType().IsInheritTargetType(typeFullName))
                    return (HotUpdateMonoBehaviour)instance.HotUpdateInstance;
            }
            return null;
        }

        public static T AddILComponent<T>(this GameObject gameObject)
        {
            HotUpdateMono hotFixMono = gameObject.AddComponent<HotUpdateMono>();
            return (T)hotFixMono.InitHotUpdateMono(typeof(T).FullName);
        }

        public static HotUpdateMonoBehaviour AddILComponent(this GameObject gameObject, string typeFullName)
        {
            HotUpdateMono hotFixMono = gameObject.AddComponent<HotUpdateMono>();
            return (HotUpdateMonoBehaviour)hotFixMono.InitHotUpdateMono(typeFullName);
        }

        public static T GetILComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false)
        {
            HotUpdateMono[] monos = gameObject.GetComponentsInChildren<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    return (T)instance.HotUpdateInstance;
            }
            return default;
        }

        public static T[] GetILComponentsInChildren<T>(this GameObject gameObject, bool includeInactive = false)
        {
            List<T> ts = new List<T>();
            HotUpdateMono[] monos = gameObject.GetComponentsInChildren<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    ts.Add((T)instance.HotUpdateInstance);
            }
            return ts.ToArray();
        }

        public static T GetILComponentInParent<T>(this GameObject gameObject, bool includeInactive = false)
        {
            HotUpdateMono[] monos = gameObject.GetComponentsInParent<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    return (T)instance.HotUpdateInstance;
            }
            return default;
        }

        public static T[] GetILComponentsInParent<T>(this GameObject gameObject, bool includeInactive = false)
        {
            List<T> ts = new List<T>();
            HotUpdateMono[] monos = gameObject.GetComponentsInParent<HotUpdateMono>(includeInactive);
            for (int i = 0; i < monos.Length; i++)
            {
                HotUpdateMono instance = monos[i];
                if (typeof(T).IsAssignableFrom(instance.HotUpdateInstance.GetType()))
                    ts.Add((T)instance.HotUpdateInstance);
            }
            return ts.ToArray();
        }
        #endregion
    }
}