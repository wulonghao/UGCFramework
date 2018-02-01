using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XLua;

[Hotfix]
public class NodeManager
{
    public static string currentNodeName;

    public static Node OpenNode(string nodeName = null, string folderName = null, Action finishCallback = null, bool isCloseLastNode = true, bool isAddToRecord = true)
    {
        nodeName = (string.IsNullOrEmpty(folderName) ? "" : (folderName + "/")) + nodeName;
        return OpenNodeAc(nodeName, finishCallback, isCloseLastNode, isAddToRecord);
    }
    
    /// <summary> 打开一个不影响任何流程的Node </summary>
    public static T OpenFloatNode<T>() where T : Node
    {
        return OpenNode<T>(null, null, false, false);
    }

    public static T OpenNode<T>(string folderName = null, Action finishCallback = null, bool isCloseLastNode = true, bool isAddToRecord = true) where T : Node
    {
        string fileName = (string.IsNullOrEmpty(folderName) ? "" : (folderName + "/")) + GetNodeType<T>();
        return (T)OpenNodeAc(fileName, finishCallback, isCloseLastNode, isAddToRecord);
    }

    /// <summary>
    /// 打开指定类型Node
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="folderName"></param>
    /// <param name="finishCallback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    static Node OpenNodeAc(string fileName = null, Action finishCallback = null, bool isCloseLastNode = true, bool isAddToRecord = true)
    {
        Node node = CreateNode(fileName, finishCallback);
        if (node != null)
        {
            Node lastNode = GetNode(currentNodeName);
            if (isCloseLastNode && lastNode && lastNode != node)
            {
                node.lastNodeList.Add(lastNode.nodePath);
                lastNode.Close(false);
            }
            if (isAddToRecord)
                currentNodeName = node.name;
        }
        else
            UIUtils.Log("打开失败!");
        return node;
    }

    static Node CreateNode(string nodePath, Action finishCallback = null)
    {
        if (string.IsNullOrEmpty(nodePath))
            return null;

        Node node = GetNode(nodePath);
        if (node)
        {
            node.transform.SetAsLastSibling();
            node.Open();
        }
        else
        {
            AssetBundle ab = null;
            if (File.Exists(ConstantUtils.AssetBundleFolderPath + "/common/sprite/" + MiscUtils.GetFileName(nodePath)))
                ab = BundleManager.Instance.GetSpriteBundle(MiscUtils.GetFileName(nodePath));
            string path = "nodes/" + nodePath;
            GameObject go = BundleManager.Instance.GetGameObject(path, PageManager.Instance.CurrentPage.name);
            if (go)
            {
                node = go.GetComponent<Node>();
                node.SetSpriteAB(ab);
                node.nodePath = nodePath;
                node.Init();
                UIUtils.AttachAndReset(go, PageManager.Instance.CurrentPage.transform);
                go.transform.SetAsLastSibling();
                node.Open();
            }
            else
                Debug.Log("错误! 找不到路径: " + path);
        }
        if (finishCallback != null)
            finishCallback();
        return node;
    }

    /// <summary>
    /// 获取指定Node(已打开),不存在则返回null
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public static Node GetNode(string nodeName)
    {
        if (!string.IsNullOrEmpty(nodeName))
        {
            Node[] nodes = PageManager.Instance.GetComponentsInChildren<Node>(true);
            foreach (Node n in nodes)
                if (nodeName.IndexOf(n.name) >= 0)
                    return n;
        }
        return null;
    }

    /// <summary>
    /// 获取指定Node(已打开),不存在则返回null
    /// </summary>
    /// <typeparam name="T">Node</typeparam>
    /// <returns></returns>
    public static T GetNode<T>() where T : Node
    {
        return (T)GetNode(typeof(T).ToString());
    }

    static string GetNodeType<T>() where T : Node
    {
        return typeof(T).ToString();
    }

    public static void CloseNode<T>(bool isOpenLast = true) where T : Node
    {
        T t = GetNode<T>();
        if (t)
            t.Close(isOpenLast);
        else
            UIUtils.Log("指定Node:" + t + "不存在");
    }

    public static void CloseNode(string nodeName, bool isOpenLast = true)
    {
        Node node = GetNode(nodeName);
        if (node)
            node.Close(isOpenLast);
        else
            UIUtils.Log("指定Node:" + nodeName + "不存在");
    }

    /// <summary> 切换page时不销毁当前node </summary>
    public static void DontDestroyAtChangePage(Node node)
    {
        node.transform.SetParent(PageManager.Instance.transform);
    }
}