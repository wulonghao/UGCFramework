using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;

namespace UGCF.HotUpdate
{
    public class HotFixBaseInheritMono : MonoBehaviour
    {
        private const string HotTypeNameFormat = "HotFix.{0}HotFix";
        private object instanceHotFix;
        private ILType type;
        private string hotFixTypeName;
        private Dictionary<string, IMethod> iMethods = new Dictionary<string, IMethod>();

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
                FieldInfo fieldInfo = type.ReflectionType.GetField("instance");
                if (fieldInfo != null)
                    fieldInfo.SetValue(instanceHotFix, this);
            }
        }

        protected bool TryInvokeHotFix(out object returnObject, params object[] p)
        {
            returnObject = null;
            if (ILRuntimeUtils.appdomain == null)
                return false;
            if (instanceHotFix == null)
                InitHotFix();
            if (instanceHotFix != null)
            {
                MethodBase method = new StackFrame(1).GetMethod();
                string methodName = method.Name;
                int paramCount = method.GetParameters().Length;
                string key = methodName + "_" + paramCount.ToString();
                IMethod im;
                if (iMethods.ContainsKey(key))
                    im = iMethods[key];
                else
                {
                    im = type.GetMethod(methodName, paramCount);
                    iMethods.Add(key, im);
                }
                if (im != null)
                {
                    returnObject = ILRuntimeUtils.appdomain.Invoke(im, instanceHotFix, p);
                    return true;
                }
            }
            return false;
        }

        protected static bool TryInvokeStaticHotFix(out object returnObject, params object[] p)
        {
            returnObject = null;
            if (ILRuntimeUtils.appdomain == null)
                return false;
            MethodBase method = new StackFrame(1).GetMethod();
            if (!method.IsStatic)
                return false;
            string typeFullName = string.Format(HotTypeNameFormat, method.DeclaringType.Name);
            if (!ILRuntimeUtils.appdomain.LoadedTypes.ContainsKey(typeFullName))
                return false;

            if (ILRuntimeUtils.appdomain.LoadedTypes[typeFullName] is ILType type)
            {
                IMethod im = type.GetMethod(method.Name, method.GetParameters().Length);
                if (im != null)
                {
                    returnObject = ILRuntimeUtils.appdomain.Invoke(im, null, p);
                    return true;
                }
            }
            return false;
        }
    }
}