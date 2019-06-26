using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NodeManager
{
    public static List<string> nodeHistory = new List<string>();
    public static string currentNodePath;//当前Node的资源相对路径，包括FloatNode
    const string DefaultNodeDirectoryPath = "UIResources/Node";

    /// <summary> 打开一个不记录也不影响任何其他Node的Node </summary>
    public static T OpenFloatNode<T>(string directoryPath = DefaultNodeDirectoryPath) where T : Node
    {
        return OpenNode<T>(false, false, directoryPath);
    }

    /// <summary>
    /// 打开指定类型Node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isAddToRecord"></param>
    /// <param name="closeLastNode"></param>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public static T OpenNode<T>(bool isAddToRecord = true, bool closeLastNode = true, string directoryPath = DefaultNodeDirectoryPath) where T : Node
    {
        return (T)OpenNode(typeof(T).ToString(), isAddToRecord, closeLastNode, directoryPath);
    }

    /// <summary>
    /// 打开指定路径名的Node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nodePath">相对Node文件夹的路径名</param>
    /// <param name="isAddToRecord"></param>
    /// <param name="closeLastNode"></param>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public static Node OpenNode(string nodePath, bool isAddToRecord = true, bool closeLastNode = true, string directoryPath = DefaultNodeDirectoryPath)
    {
        return OpenNodeAc(nodePath, directoryPath, isAddToRecord, closeLastNode);
    }

    static Node OpenNodeAc(string nodePath, string directoryPath, bool isAddToRecord, bool closeLastNode)
    {
        Node node = CreateNode(nodePath, directoryPath);
        if (node != null)
        {
            node.transform.SetAsLastSibling();
            node.Open();
            //Node的背景音乐的加载
            //AudioManager.Instance.PlayMusic(Path.GetFileNameWithoutExtension(nodePath), directoryPath + "/" + nodePath);

            if (isAddToRecord)
                RefreshHistory(nodePath);
            if (!string.IsNullOrEmpty(currentNodePath))
            {
                Node lastNode = GetNode(currentNodePath);
                if (lastNode && lastNode != node)
                    lastNode.Close();
            }
            currentNodePath = nodePath;
        }
        return node;
    }

    static Node CreateNode(string nodePath, string directoryPath)
    {
        if (string.IsNullOrEmpty(nodePath))
            return null;

        Node node = GetNode(nodePath);
        if (!node)
        {
            string nodeName = Path.GetFileNameWithoutExtension(nodePath);
            string path = directoryPath + "/" + nodePath;

            AssetBundle ab = null;
            GameObject go = BundleManager.Instance.GetGameObjectByUI(path);
            if (!go)
            {
                ab = BundleManager.Instance.GetSpriteBundle("DefaultLoad", path);
                go = BundleManager.Instance.GetGameObjectByUI(path + "/Prefab/" + nodeName);
                if (!go)
                {
                    LogUtils.LogError("找不到路径: " + path + "/Prefab/" + nodeName);
                    return null;
                }
            }
            node = go.GetComponent<Node>();
            if (node)
            {
                UIUtils.AttachAndReset(go, PageManager.Instance.currentPage.transform);
                node.SetSpriteAB(ab);
                node.nodePath = nodePath;
                node.directoryPath = directoryPath;
                node.Init();
            }
            else
            {
                LogUtils.LogError("找不到包含的Node组件: " + nodeName);
                return null;
            }
        }
        return node;
    }

    /// <summary>
    /// 刷新Node历史
    /// </summary>
    /// <param name="type"></param>
    static void RefreshHistory(string nodePath)
    {
        int index = nodeHistory.IndexOf(nodePath);
        if (index >= 0 && index < nodeHistory.Count - 1)
            nodeHistory.RemoveRange(index + 1, nodeHistory.Count - index - 1);
        else
            nodeHistory.Add(nodePath);
    }

    public static void ClearNodeHistory()
    {
        nodeHistory.Clear();
    }

    /// <summary>
    /// 获取指定Node(已存在),不存在则返回null
    /// </summary>
    /// <param name="nodeName">node文件名或相对路径名</param>
    /// <returns></returns>
    public static Node GetNode(string nodeName)
    {
        nodeName = Path.GetFileNameWithoutExtension(nodeName);
        if (!string.IsNullOrEmpty(nodeName))
        {
            Node[] nodes = PageManager.Instance.GetComponentsInChildren<Node>(true);
            foreach (Node n in nodes)
                if (nodeName.Equals(n.name))
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

    public static void CloseNode<T>(bool isOpenLast = false) where T : Node
    {
        T t = GetNode<T>();
        if (t) t.Close(isOpenLast);
    }

    public static void CloseNode(string nodeName, bool isOpenLast = false)
    {
        Node node = GetNode(nodeName);
        if (node) node.Close(isOpenLast);
    }

    /// <summary> 切换page时不销毁当前node </summary>
    public static void DontDestroyAtChangePage(Node node)
    {
        node.transform.SetParent(PageManager.Instance.transform);
        node.transform.SetAsLastSibling();
    }
}