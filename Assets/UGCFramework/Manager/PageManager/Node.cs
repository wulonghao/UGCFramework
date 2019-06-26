using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 窗口关闭方式
/// </summary>
public enum NodeCloseMode
{
    Hide,
    Destroy
}
public class Node : HotFixBaseInheritMono
{
    /// <summary> 关闭按钮 </summary>
    public GameObject btnClose;
    /// <summary> 蒙层 </summary>
    public GameObject maskLayer;
    /// <summary> 点击蒙层是否关闭Node </summary>
    public bool isClickMaskClose = false;
    /// <summary> Node关闭方式 </summary>
    public NodeCloseMode windowCloseMode;
    [HideInInspector]
    public string nodePath, directoryPath;
    protected AssetBundle spriteAB;

    public virtual void Init()
    {
        if (isClickMaskClose && maskLayer)
            UGUIEventListener.Get(maskLayer).onClick += delegate { Close(); };
        if (btnClose)
            UGUIEventListener.Get(btnClose).onClick += delegate { Close(); };
        gameObject.SetActive(false);
        LogUtils.Log(name + "：Init");
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        TipManager.Instance.CloseAllTip();
        LogUtils.Log(name + "：Open");
    }

    public void SetSpriteAB(AssetBundle ab)
    {
        spriteAB = ab;
    }

    public AssetBundle GetSpriteAB()
    {
        return spriteAB;
    }

    public Sprite GetSpriteByDefaultAB(string spriteName)
    {
        return BundleManager.Instance.GetSprite(spriteName, spriteAB);
    }

    public JsonData GetJsonData(string jsonName)
    {
        return BundleManager.Instance.GetCommonJsonData(jsonName, directoryPath + "/" + nodePath);
    }

    public GameObject GetPrefab(string gameObjectName)
    {
        return BundleManager.Instance.GetGameObjectByUI(directoryPath + "/" + nodePath + "/" + gameObjectName + "/Prefab");
    }

    /// <summary>
    /// 关闭Node
    /// </summary>
    /// <param name="isOpenLast">是否打开上一个Node</param>
    public virtual void Close(bool isOpenLast = true)
    {
        if (NodeManager.nodeHistory.Count > 0)
        {
            if (NodeManager.nodeHistory[NodeManager.nodeHistory.Count - 1] == name)
                NodeManager.nodeHistory.RemoveAt(NodeManager.nodeHistory.Count - 1);
            if (NodeManager.nodeHistory.Count > 0)
                NodeManager.currentNodePath = NodeManager.nodeHistory[NodeManager.nodeHistory.Count - 1];
            else
                NodeManager.currentNodePath = null;
            if (isOpenLast && !string.IsNullOrEmpty(NodeManager.currentNodePath))
                NodeManager.OpenNode(NodeManager.currentNodePath, false, false);
        }
        else
            NodeManager.currentNodePath = null;
        if (!gameObject || !this)
            return;
        if (windowCloseMode == NodeCloseMode.Hide)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }

    public virtual void OnDestroy()
    {
        if (spriteAB)
            spriteAB.Unload(true);
        Resources.UnloadUnusedAssets();
    }
}