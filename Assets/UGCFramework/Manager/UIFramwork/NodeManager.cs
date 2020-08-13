using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class NodeManager
{
    public static Node currentNode;//当前新打开的Node，包括FloatNode
    const string DefaultNodeDirectoryPath = "UIResources/Node";

    /// <summary> 打开一个不记录也不影响任何流程的Node，该Node将自动播放入场动画 </summary>
    public static T OpenFloatNode<T>(string directoryPath = DefaultNodeDirectoryPath) where T : Node
    {
        return OpenNode<T>(true, false, directoryPath);
    }

    /// <summary> 打开一个不记录也不影响任何流程的Node，该Node将自动播放入场动画 </summary>
    public static Node OpenFloatNode(string nodePath, string directoryPath = DefaultNodeDirectoryPath)
    {
        return OpenNode(nodePath, directoryPath, true, false);
    }

    /// <summary>
    /// 打开指定类型Node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isAutoPlayEnter">自动播放入场动画</param>
    /// <param name="isCloseLastNode">打开后关闭上一个Node，同时将上一个Node的记录从历史中删除</param>
    /// <param name="isAddToRecord">添加到历史记录中</param>
    /// <param name="directoryPath">文件夹路径</param>
    /// <returns></returns>
    public static T OpenNode<T>(bool isAutoPlayEnter = true, bool isCloseLastNode = false, string directoryPath = DefaultNodeDirectoryPath) where T : Node
    {
        return (T)OpenNode(typeof(T).ToString(), directoryPath, isAutoPlayEnter, isCloseLastNode);
    }

    /// <summary>
    /// 打开指定路径名的Node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nodePath">相对Node文件夹的路径名</param>
    /// <param name="isCloseLastNode">打开后关闭上一个Node，同时将上一个Node的记录从历史中删除</param>
    /// <param name="isAddToRecord">添加到历史记录中</param>
    /// <param name="directoryPath">文件夹路径</param>
    /// <returns></returns>
    public static Node OpenNode(string nodePath, string directoryPath = DefaultNodeDirectoryPath, bool isAutoPlayEnter = true, bool isCloseLastNode = false)
    {
        return OpenNodeAc(nodePath, directoryPath, isAutoPlayEnter, isCloseLastNode);
    }

    static Node OpenNodeAc(string nodePath, string directoryPath, bool isAutoPlayEnter, bool isCloseLastNode)
    {
        Node node = CreateNode(nodePath, directoryPath);
        if (node)
        {
            if (node.animationMian && node.animationMian.isSwitchAnimPlaying)
                return node;
            node.transform.SetAsLastSibling();
            node.Open();
            //Node的背景音乐的加载
            //AudioManager.Instance.PlayMusic(Path.GetFileNameWithoutExtension(nodePath), directoryPath + "/" + nodePath);

            if (isAutoPlayEnter)
                node.PlayAnimation(true, () => CloseLastNodeAndRefreshData(isCloseLastNode, node, currentNode));
            else
                CloseLastNodeAndRefreshData(isCloseLastNode, node, currentNode);
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
            MiscUtils.AttachAndReset(go, PageManager.Instance.currentPage.transform);
            node = go.GetComponent<Node>();
            if (node)
            {
                node.SetSpriteAB(ab);
                node.nodePath = nodePath;
                node.directoryPath = directoryPath;
                node.Init();
            }
            else
            {
                LogUtils.Log("找不到预制体上包含的Node组件: " + nodeName);
                return null;
            }
        }
        return node;
    }

    static void CloseLastNodeAndRefreshData(bool isCloseLastNode, Node currentNode, Node lastNode)
    {
        if (isCloseLastNode && lastNode)
            lastNode.Close(false);
        NodeManager.currentNode = currentNode;
    }

    /// <summary>
    /// 获取指定Node(已存在),不存在则返回null
    /// </summary>
    /// <param name="nodeName">node文件名或相对路径名</param>
    /// <returns></returns>
    public static Node GetNode(string nodeName)
    {
        if (nodeName.IndexOf(',') >= 0)
            nodeName = nodeName.Split(',')[1];
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

    public static Node GetLastNode(bool includeInactive = true, bool isContainSelf = true)
    {
        Node[] nodes = PageManager.Instance.GetComponentsInChildren<Node>(includeInactive);
        if (nodes.Length > 0)
        {
            if (isContainSelf || nodes[nodes.Length - 1] != currentNode)
                return nodes[nodes.Length - 1];
            else
                return nodes.Length > 1 ? nodes[nodes.Length - 2] : null;
        }
        else
            return null;
    }

    public static Node GetLastButOneNode(bool includeInactive = true, bool isContainSelf = true)
    {
        Node[] nodes = PageManager.Instance.GetComponentsInChildren<Node>(includeInactive);
        if (nodes.Length > 1)
        {
            if (isContainSelf || nodes[nodes.Length - 2] != currentNode)
                return nodes[nodes.Length - 2];
            else
                return nodes.Length > 2 ? nodes[nodes.Length - 3] : null;
        }
        else
            return null;
    }

    public static void CloseNode<T>(bool isActiveClose = true) where T : Node
    {
        T t = GetNode<T>();
        if (t && t.gameObject) t.Close(isActiveClose);
    }

    public static void CloseNode(string nodeName, bool isActiveClose = true)
    {
        Node node = GetNode(nodeName);
        if (node && node.gameObject) node.Close(isActiveClose);
    }

    /// <summary> 切换page时不销毁当前node </summary>
    public static void DontDestroyAtChangePage(Node node)
    {
        node.transform.SetParent(PageManager.Instance.transform);
        node.transform.SetAsLastSibling();
    }
}