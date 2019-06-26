using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Text;

public partial class ReportAsset
{
    [MenuItem("自定义工具/打印脚本Static引用")]
    static void StaticRef()
    {
        //静态引用
        LoadAssembly("Assembly-CSharp-firstpass");
        LoadAssembly("Assembly-CSharp");

    }

    static void LoadAssembly(string name)
    {
        Assembly assembly = null;
        try
        {
            assembly = Assembly.Load(name);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
        finally
        {
            if (assembly != null)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    try
                    {
                        HashSet<string> assetPaths = new HashSet<string>();
                        FieldInfo[] listFieldInfo = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        foreach (FieldInfo fieldInfo in listFieldInfo)
                        {
                            if (!fieldInfo.FieldType.IsValueType)
                            {
                                SearchProperties(fieldInfo.GetValue(null), assetPaths);
                            }
                        }
                        if (assetPaths.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("{0}.cs\n", type.ToString());
                            foreach (string path in assetPaths)
                            {
                                sb.AppendFormat("\t{0}\n", path);
                            }
                            Debug.LogError(sb.ToString());
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex.Message);
                    }
                }
            }
        }
    }

    static HashSet<string> SearchProperties(object obj, HashSet<string> assetPaths)
    {
        if (obj != null)
        {
            if (obj is UnityEngine.Object)
            {
                UnityEngine.Object[] depen = EditorUtility.CollectDependencies(new UnityEngine.Object[] { obj as UnityEngine.Object });
                foreach (var item in depen)
                {
                    string assetPath = AssetDatabase.GetAssetPath(item);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        if (!assetPaths.Contains(assetPath))
                        {
                            assetPaths.Add(assetPath);
                        }
                    }
                }
            }
            else if (obj is IEnumerable)
            {
                foreach (object child in (obj as IEnumerable))
                {
                    SearchProperties(child, assetPaths);
                }
            }
            else if (obj is System.Object)
            {
                if (!obj.GetType().IsValueType)
                {
                    FieldInfo[] fieldInfos = obj.GetType().GetFields();
                    foreach (FieldInfo fieldInfo in fieldInfos)
                    {
                        object o = fieldInfo.GetValue(obj);
                        if (o != obj)
                        {
                            SearchProperties(fieldInfo.GetValue(obj), assetPaths);
                        }
                    }
                }
            }
        }
        return assetPaths;
    }

}