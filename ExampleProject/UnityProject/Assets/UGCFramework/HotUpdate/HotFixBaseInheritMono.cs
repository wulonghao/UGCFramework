using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;

namespace UGCF.HotUpdate
{
    public class HotFixBaseInheritMono : MonoBehaviour
    {
        private object instanceHotFix;
        private ILType type;
        private string hotFixTypeName;
        private Dictionary<string, IMethod> iMethods = new Dictionary<string, IMethod>();
        private const string HotTypeNameFormat = "HotFix.{0}HotFix";

        /// <summary>
        /// 初始化热更类，加载相关资源
        /// </summary>
        void InitHotFix()
        {
            hotFixTypeName = string.Format(HotTypeNameFormat, GetType().Name);
            if (type == null && ILRuntimeUtils.appdomain != null && ILRuntimeUtils.appdomain.LoadedTypes != null && ILRuntimeUtils.appdomain.LoadedTypes.ContainsKey(hotFixTypeName))
            {
                type = ILRuntimeUtils.appdomain.LoadedTypes[hotFixTypeName] as ILType;
                instanceHotFix = ILRuntimeUtils.appdomain.Instantiate(hotFixTypeName);
                type.ReflectionType.GetField("instance").SetValue(instanceHotFix, this);
            }
        }

        /// <summary>
        /// 检测静态函数的热更是否存在
        /// </summary>
        /// <returns></returns>
        protected static bool CheckHotFixStaticMethod(out string typeFullName, out string methodName)
        {
            MethodBase method = new StackFrame(1).GetMethod();
            methodName = method.Name;
            typeFullName = string.Format(HotTypeNameFormat, method.DeclaringType.Name);

            if (ILRuntimeUtils.appdomain == null || !ILRuntimeUtils.appdomain.LoadedTypes.ContainsKey(typeFullName))
                return false;
            if (ILRuntimeUtils.appdomain.LoadedTypes[typeFullName] is ILType type)
            {
                IMethod im = type.GetMethod(methodName);
                return im != null;
            }
            return false;
        }

        /// <summary>
        /// 检测非静态函数的热更是否存在
        /// </summary>
        /// <returns></returns>
        protected bool CheckHotFixMethod(out string methodName)
        {
            IMethod im = null;
            if (instanceHotFix == null)
                InitHotFix();
            if (instanceHotFix != null)
            {
                MethodBase method = new StackFrame(1).GetMethod();
                methodName = method.Name;
                int paramCount = method.GetParameters().Length;
                string key = method.Name + "_" + paramCount.ToString();
                if (iMethods.ContainsKey(key))
                    im = iMethods[key];
                else
                {
                    im = type.GetMethod(method.Name, paramCount);
                    iMethods.Add(key, im);
                }
            }
            else
                methodName = null;
            return im != null;
        }

        /// <summary>
        /// 执行静态函数的热更
        /// </summary>
        /// <param name="p">所有参数</param>
        /// <returns></returns>
        protected static object InvokeStaticHotFix(string typeFullName, string methodName, params object[] p)
        {
            if (ILRuntimeUtils.appdomain == null || !ILRuntimeUtils.appdomain.LoadedTypes.ContainsKey(typeFullName))
                return false;
            if (ILRuntimeUtils.appdomain.LoadedTypes[typeFullName] is ILType type)
            {
                IMethod im = type.GetMethod(methodName);
                if (im != null)
                    return ILRuntimeUtils.appdomain.Invoke(im, null, p);
            }
            return null;
        }

        /// <summary>
        /// 执行非静态函数的热更
        /// </summary>
        /// <param name="p">所有参数</param>
        /// <returns></returns>
        protected object InvokeHotFix(string methodName, params object[] p)
        {
            if (instanceHotFix != null)
            {
                string key;
                if (p != null)
                    key = methodName + "_" + p.Length.ToString();
                else
                    key = methodName + "_0";
                if (iMethods.ContainsKey(key))
                {
                    IMethod im = iMethods[key];
                    if (im != null)
                        return ILRuntimeUtils.appdomain.Invoke(im, instanceHotFix, p);
                }
            }
            return null;
        }
    }
}