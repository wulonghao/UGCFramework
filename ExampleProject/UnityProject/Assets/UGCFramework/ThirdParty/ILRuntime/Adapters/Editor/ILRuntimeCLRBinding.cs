#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using UGCF.HotUpdate;
using System.IO;

[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    [MenuItem("ILRuntime/Generate CLR Binding Code")]
    static void GenerateCLRBinding()
    {
        List<Type> types = new List<Type>
        {
            typeof(int),
            typeof(short),
            typeof(float),
            typeof(long),
            typeof(object),
            typeof(string),
            typeof(Array),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Quaternion),
            typeof(GameObject),
            typeof(UnityEngine.Object),
            typeof(Transform),
            typeof(RectTransform),
            typeof(Time),
            typeof(Debug),
            //所有DLL内的类型的真实C#类型都是ILTypeInstance
            typeof(List<ILRuntime.Runtime.Intepreter.ILTypeInstance>)
        };
        ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, "Assets/UGCFramework/ThirdParty/ILRuntime/Generated");
    }

    [MenuItem("ILRuntime/通过自动分析热更DLL生成CLR绑定")]
    static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        using (FileStream fs = new FileStream("Assets/Editor Default Resources/AssetBundle/CommonResources/HotFix/HotFix.bytes", FileMode.Open, FileAccess.Read))
        {
            domain.LoadAssembly(fs);
            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, "Assets/UGCFramework/ThirdParty/ILRuntime/Generated");
        }

        AssetDatabase.Refresh();
    }

    static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
        domain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        domain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        domain.RegisterCrossBindingAdaptor(new PageAdapter());
        domain.RegisterCrossBindingAdaptor(new NodeAdapter());
    }
}
#endif
