using System.IO;
using UGCF.Utils;
using UnityEngine;

namespace UGCF.Manager
{
    public partial class NodeManager
    {
        public static Node CurrentNode;//当前新打开的Node，包括FloatNode
        const string DefaultNodeDirectoryPath = "UIResources/Node";

        /// <summary> 打开一个不记录也不影响任何流程的Node，打开时播放入场动画 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directoryPath">文件夹路径</param>
        /// <returns></returns>
        public static T OpenFloatNode<T>(string directoryPath = DefaultNodeDirectoryPath) where T : Node
        {
            return OpenNode<T>(true, false, directoryPath);
        }

        /// <summary> 打开一个不记录也不影响任何流程的Node，打开时播放入场动画 </summary>
        /// <param name="nodePath">相对Node文件夹的路径名</param>
        /// <param name="directoryPath">文件夹路径</param>
        /// <returns></returns>
        public static Node OpenFloatNode(string nodePath, string directoryPath = DefaultNodeDirectoryPath)
        {
            return OpenNode(nodePath, true, false, directoryPath);
        }

        /// <summary>
        /// 打开指定类型Node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isAutoPlayEnter">是否播放入场动画</param>
        /// <param name="isCloseLastNode">打开后关闭上一个Node，然后执行最后一个Node的Close</param>
        /// <param name="directoryPath">文件夹路径</param>
        /// <returns></returns>
        public static T OpenNode<T>(bool isAutoPlayEnter = true, bool isCloseLastNode = true, string directoryPath = DefaultNodeDirectoryPath) where T : Node
        {
            return (T)OpenNode(typeof(T).ToString(), isAutoPlayEnter, isCloseLastNode, directoryPath);
        }

        /// <summary>
        /// 打开指定路径名的Node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodePath">相对Node文件夹的路径名</param>
        /// <param name="isAutoPlayEnter">是否播放入场动画</param>
        /// <param name="isCloseLastNode">打开后关闭上一个Node，然后执行最后一个Node的Close</param>
        /// <param name="directoryPath">文件夹路径</param>
        /// <returns></returns>
        public static Node OpenNode(string nodePath, bool isAutoPlayEnter = true, bool isCloseLastNode = true, string directoryPath = DefaultNodeDirectoryPath)
        {
            return OpenNodeAc(nodePath, directoryPath, isAutoPlayEnter, isCloseLastNode);
        }

        static Node OpenNodeAc(string nodePath, string directoryPath, bool isAutoPlayEnter, bool isCloseLastNode)
        {
            Node newNode = CreateNode(nodePath, directoryPath);
            if (newNode)
            {
                if (newNode.AnimationMian && newNode.AnimationMian.IsSwitchAnimPlaying)
                    return newNode;
                newNode.transform.SetAsLastSibling();
                newNode.Open();
                //Node的背景音乐的加载
                //AudioManager.Instance.PlayMusic(Path.GetFileNameWithoutExtension(nodePath), directoryPath + "/" + nodePath);

                if (isAutoPlayEnter)
                    newNode.PlayEnterAnimation(() => RefreshCurrentNode(isCloseLastNode, newNode, CurrentNode));
                else
                    RefreshCurrentNode(isCloseLastNode, newNode, CurrentNode);
            }
            return newNode;
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
                UIUtils.AttachAndReset(go, PageManager.Instance.CurrentPage.transform);
                node = go.GetComponent<Node>();
                if (node)
                {
                    node.SetSpriteAB(ab);
                    node.NodePath = nodePath;
                    node.DirectoryPath = directoryPath;
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

        static void RefreshCurrentNode(bool isCloseLastNode, Node newNode, Node currentNode)
        {
            if (isCloseLastNode && currentNode)
                currentNode.Close(false);
            CurrentNode = newNode;
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
                Node[] nodes = UGCFMain.Instance.RootCanvas.GetComponentsInChildren<Node>(true);
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
            Node[] nodes = UGCFMain.Instance.RootCanvas.GetComponentsInChildren<Node>(includeInactive);
            if (nodes.Length > 0)
            {
                if (isContainSelf || nodes[nodes.Length - 1] != CurrentNode)
                    return nodes[nodes.Length - 1];
                else
                    return nodes.Length > 1 ? nodes[nodes.Length - 2] : null;
            }
            else
                return null;
        }

        public static Node GetLastButOneNode(bool includeInactive = true, bool isContainSelf = true)
        {
            Node[] nodes = UGCFMain.Instance.RootCanvas.GetComponentsInChildren<Node>(includeInactive);
            if (nodes.Length > 1)
            {
                if (isContainSelf || nodes[nodes.Length - 2] != CurrentNode)
                    return nodes[nodes.Length - 2];
                else
                    return nodes.Length > 2 ? nodes[nodes.Length - 3] : null;
            }
            else
                return null;
        }

        public static void CloseNode<T>(bool isInitiativeClose = true) where T : Node
        {
            T t = GetNode<T>();
            if (t && t.gameObject) t.Close(isInitiativeClose);
        }

        public static void CloseNode(string nodeName, bool isInitiativeClose = true)
        {
            Node node = GetNode(nodeName);
            if (node && node.gameObject) node.Close(isInitiativeClose);
        }

        /// <summary> 切换page时不销毁当前node </summary>
        public static void DontDestroyAtChangePage(Node node)
        {
            node.transform.SetParent(UGCFMain.Instance.RootCanvas);
            node.transform.SetAsLastSibling();
        }
    }
}