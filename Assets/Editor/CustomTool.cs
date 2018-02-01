using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomTool : Editor
{
    [MenuItem("自定义工具/清除缓存")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("自定义工具/置灰/添加置灰材质球")]
    static void AddImageToGrey()
    {
        GameObject[] gos = Selection.gameObjects;
        for (int i = 0; i < gos.Length; i++)
            gos[i].GetComponent<Image>().material = Resources.Load<Material>("Shader/ImageToGrey");
    }

    [MenuItem("自定义工具/设置选中物体所有的raycast为false")]
    static void ClearRaycast()
    {
        GameObject[] gos = Selection.gameObjects;
        for (int i = 0; i < gos.Length; i++)
        {
            Graphic[] gs = gos[i].GetComponentsInChildren<Graphic>();
            for (int j = 0; j < gs.Length; j++)
                gs[j].raycastTarget = false;
        }
    }

    static string fontName = "Arial";
    [MenuItem("自定义工具/字体/字体检查 %Q")]
    static void CheckFont()
    {
        Transform[] transs = Selection.transforms;
        for (int i = 0; i < transs.Length; i++)
        {
            Text[] texts = transs[i].GetComponentsInChildren<Text>(true);
            for (int j = 0; j < texts.Length; j++)
            {
                if (texts[j].font.name == fontName)
                {
                    Debug.LogWarning(texts[j].name);
                }
            }
        }
    }

    [MenuItem("自定义工具/输出选择文件夹下所有文件名")]
    static void PrintSelectAllFileName()
    {
        string s = "";
        Object[] gos = Selection.objects;
        for (int i = 0; i < gos.Length; i++)
        {
            s += gos[i].name + ",\n";
        }
        Debug.Log(s);
    }
}
