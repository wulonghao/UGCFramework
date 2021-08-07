﻿using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using UGCF.Manager;
using UGCF.UGUIExtend;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace UGCF.HotUpdate
{
    public class ILRuntimeUtils
    {
        public static AppDomain appdomain;
        static AssetBundle hotfixAB;

        /// <summary>
        /// 加载热更补丁
        /// </summary>
        public static void LoadHotFixWithInit()
        {
            if (hotfixAB) hotfixAB.Unload(true);
            hotfixAB = BundleManager.Instance.GetBundle(ConstantUtils.CommonResourcesFolderName + "/HotFix");
            if (hotfixAB)
            {
                appdomain = new AppDomain();
                TextAsset taHotFix = hotfixAB.LoadAsset<TextAsset>("ugcfhotfix");
                if (!taHotFix) return;
                MemoryStream ms = new MemoryStream(taHotFix.bytes);
                if (ms != null)
                {
                    TextAsset taHotFixPdb = hotfixAB.LoadAsset<TextAsset>("ugcfhotfixpdb");
                    if (!taHotFixPdb) return;
                    MemoryStream msp = new MemoryStream(taHotFixPdb.bytes);
                    if (msp != null)
                    {
                        appdomain.LoadAssembly(ms, msp, new PdbReaderProvider());
                    }
                }
                InitializeILRuntime();
            }
        }

        static void InitializeILRuntime()
        {
            SetupCLRRedirectionAddComponent();
            SetupCLRRedirectionGetComponent();
            appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            appdomain.RegisterCrossBindingAdaptor(new PageAdapter());
            appdomain.RegisterCrossBindingAdaptor(new NodeAdapter());

            JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
            RegisterDelegate();
        }

        static void RegisterDelegate()
        {
            appdomain.DelegateManager.RegisterMethodDelegate<object>();
            appdomain.DelegateManager.RegisterMethodDelegate<string>();
            appdomain.DelegateManager.RegisterMethodDelegate<char>();
            appdomain.DelegateManager.RegisterMethodDelegate<bool>();
            appdomain.DelegateManager.RegisterMethodDelegate<int>();
            appdomain.DelegateManager.RegisterMethodDelegate<long>();
            appdomain.DelegateManager.RegisterMethodDelegate<float>();
            appdomain.DelegateManager.RegisterMethodDelegate<double>();
            appdomain.DelegateManager.RegisterMethodDelegate<Sprite>();
            appdomain.DelegateManager.RegisterMethodDelegate<Texture2D>();
            appdomain.DelegateManager.RegisterMethodDelegate<Transform>();
            appdomain.DelegateManager.RegisterMethodDelegate<RectTransform>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject, PointerEventData>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Networking.UnityWebRequest>();

            appdomain.DelegateManager.RegisterDelegateConvertor<UGUIEventListener.UGUIDelegate>((action) => new UGUIEventListener.UGUIDelegate((Action<GameObject>)action));
            appdomain.DelegateManager.RegisterDelegateConvertor<UGUIEventListener.UGUIDelegateData>((action) => new UGUIEventListener.UGUIDelegateData((Action<GameObject, PointerEventData>)action));
            appdomain.DelegateManager.RegisterDelegateConvertor<ScrollRectChildCenter.ScrollRectItemChangeEvent>((action) => new ScrollRectChildCenter.ScrollRectItemChangeEvent((Action<GameObject>)action));
            appdomain.DelegateManager.RegisterDelegateConvertor<PanelCenterScrollRect.ScrollRectItemChangeEvent>((action) => new PanelCenterScrollRect.ScrollRectItemChangeEvent((Action<GameObject>)action));
        }

        /// <summary> unity主工程获取物体上挂载的热更DLL中的Component </summary>
        /// <param name="type">类型</param>
        /// <param name="go">物体</param>
        /// <returns></returns>
        public static MonoBehaviourAdapter.Adaptor GetComponent(ILType type, GameObject go)
        {
            var arr = go.GetComponents<MonoBehaviourAdapter.Adaptor>();
            for (int i = 0; i < arr.Length; i++)
            {
                var instance = arr[i];
                if (instance.ILInstance != null && instance.ILInstance.Type == type)
                {
                    return instance;
                }
            }
            return null;
        }

        #region ...绑定/注册相关函数
        unsafe static void SetupCLRRedirectionAddComponent()
        {
            var arr = typeof(GameObject).GetMethods();
            foreach (var i in arr)
            {
                if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
                {
                    appdomain.RegisterCLRMethodRedirection(i, AddComponent);
                }
            }
        }

        unsafe static void SetupCLRRedirectionGetComponent()
        {
            var arr = typeof(GameObject).GetMethods();
            foreach (var i in arr)
            {
                if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
                {
                    appdomain.RegisterCLRMethodRedirection(i, GetComponent);
                }
            }
        }

        unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            //成员方法的第一个参数为this
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            //AddComponent应该有且只有1个泛型参数
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;
                if (type is CLRType)
                {
                    //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                    res = instance.AddComponent(type.TypeForCLR);
                }
                else
                {
                    //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                    var ilInstance = new ILTypeInstance(type as ILType, false);//手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
                                                                               //接下来创建Adapter实例
                    var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                    //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                    clrInstance.ILInstance = ilInstance;
                    clrInstance.AppDomain = __domain;
                    //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                    ilInstance.CLRInstance = clrInstance;

                    res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance

                    clrInstance.Awake();//因为Unity调用这个方法时还没准备好所以这里补调一次
                    clrInstance.OnEnable();//因为Unity调用这个方法时还没准备好所以这里补调一次
                }

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }

        unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            //成员方法的第一个参数为this
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            //GetComponent应该有且只有1个泛型参数
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res = null;
                if (type is CLRType)
                {
                    //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                    res = instance.GetComponent(type.TypeForCLR);
                }
                else
                {
                    //因为所有DLL里面的MonoBehaviour实际都是这个Component，所以我们只能全取出来遍历查找
                    var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                    for (int i = 0; i < clrInstances.Length; i++)
                    {
                        var clrInstance = clrInstances[i];
                        if (clrInstance.ILInstance != null)//ILInstance为null, 表示是无效的MonoBehaviour，要略过
                        {
                            if (clrInstance.ILInstance.Type == type)
                            {
                                res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance
                                break;
                            }
                        }
                    }
                }

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }
        #endregion
    }
}