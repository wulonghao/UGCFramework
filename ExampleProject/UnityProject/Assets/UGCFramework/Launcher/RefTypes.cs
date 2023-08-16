using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefTypes
{
    /// <summary>
    /// 常见开销较高的泛型提前实例化
    /// </summary>
    public static void InstantiateAOTRef()
    {
        new Dictionary<string, string>();
        new Dictionary<string, object>();
        new Dictionary<string, int>();
        new Dictionary<string, float>();
        new Dictionary<string, Vector3>();

        new Dictionary<object, object>();
        new Dictionary<object, string>();
        new Dictionary<object, int>();
        new Dictionary<object, float>();
        new Dictionary<object, Vector3>();

        new Dictionary<int, int>();
        new Dictionary<int, string>();
        new Dictionary<int, object>();
        new Dictionary<int, float>();
        new Dictionary<int, Vector3>();

        new Dictionary<float, float>();
        new Dictionary<float, string>();
        new Dictionary<float, object>();
        new Dictionary<float, int>();
        new Dictionary<float, Vector3>();

        new Dictionary<Vector3, Vector3>();
        new Dictionary<Vector3, object>();
        new Dictionary<Vector3, string>();
        new Dictionary<Vector3, int>();
        new Dictionary<Vector3, float>();
    }
}
