#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using UGCF.Manager;

[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCrossBinding
{
    [MenuItem("ILRuntime/Generate Cross Binding Adapter")]
    static void GenerateCrossbindAdapter()
    {
        //由于跨域继承特殊性太多，自动生成无法实现完全无副作用生成，所以这里提供的代码自动生成主要是给大家生成个初始模版，简化大家的工作
        //大多数情况直接使用自动生成的模版即可，如果遇到问题可以手动去修改生成后的文件，因此这里需要大家自行处理是否覆盖的问题
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/ThirdParty/HotFix/Adapter/PageAdapter.cs"))
        {
            sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(Page), "ILRuntimeAdapter"));
        }
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/ThirdParty/HotFix/Adapter/NodeAdapter.cs"))
        {
            sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(Node), "ILRuntimeAdapter"));
        }
        AssetDatabase.Refresh();
    }
}
#endif
