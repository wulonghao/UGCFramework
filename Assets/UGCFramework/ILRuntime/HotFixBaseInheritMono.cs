using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotFixBaseInheritMono : MonoBehaviour
{
    public object instanceHotFix;
    ILType type;
    string hotFixTypeName;
    Dictionary<string, IMethod> iMethods = new Dictionary<string, IMethod>();

    void InitHotFix()
    {
        if (string.IsNullOrEmpty(hotFixTypeName))
            hotFixTypeName = GetTypeName();
        if (type == null && ILRuntimeUtils.appdomain.LoadedTypes != null && ILRuntimeUtils.appdomain.LoadedTypes.ContainsKey(hotFixTypeName))
        {
            type = ILRuntimeUtils.appdomain.LoadedTypes[hotFixTypeName] as ILType;
            instanceHotFix = ILRuntimeUtils.appdomain.Instantiate(hotFixTypeName);
            type.ReflectionType.GetField("instance").SetValue(instanceHotFix, this);
        }
    }

    protected void GetHotFixMethod(string name, int paramCount = 0)
    {
        if (instanceHotFix == null)
            InitHotFix();
        if (instanceHotFix != null)
        {
            string key = name + "_" + paramCount;
            IMethod im = type.GetMethod(name, paramCount);
            if (im != null && !iMethods.ContainsKey(name))
                iMethods.Add(key, im);
        }
    }

    /// <summary>
    /// 检测静态函数的热更是否存在
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns></returns>
    protected static bool CheckHotFixStaticMethod(string methodName, int paramCount = 0)
    {
        string typeName = GetTypeName();
        if (!ILRuntimeUtils.appdomain.LoadedTypes.ContainsKey(typeName))
            return false;
        ILType type = ILRuntimeUtils.appdomain.LoadedTypes[typeName] as ILType;
        if (type != null)
        {
            IMethod im = type.GetMethod(methodName);
            return im != null;
        }
        return false;
    }

    protected bool CheckHotFixMethod(string methodName, int paramCount = 0)
    {
        string key = methodName + "_" + paramCount;
        if (iMethods != null)
            return iMethods.ContainsKey(key);
        return false;
    }

    protected static object InvokeStaticHotFix(string methodName, params object[] p)
    {
        string typeName = GetTypeName();
        if (!ILRuntimeUtils.appdomain.LoadedTypes.ContainsKey(typeName))
            return false;
        ILType type = ILRuntimeUtils.appdomain.LoadedTypes[typeName] as ILType;
        if (type != null)
        {
            IMethod im = type.GetMethod(methodName);
            if (im != null)
                return ILRuntimeUtils.appdomain.Invoke(im, null, p);
        }
        return null;
    }

    protected object InvokeHotFix(string methodName, params object[] p)
    {
        string key;
        if (p != null)
            key = methodName + "_" + p.Length;
        else
            key = methodName + "_0";
        if (instanceHotFix != null && iMethods.ContainsKey(key))
            return ILRuntimeUtils.appdomain.Invoke(iMethods[key], instanceHotFix, p);
        return null;
    }

    static string GetTypeName()
    {
        return string.Format("DxzkHotFix.{0}HotFix", new System.Diagnostics.StackTrace().GetFrame(3).GetMethod().ReflectedType.Name);
    }
}