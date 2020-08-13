using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UGCF.UnityExtend;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class Node : HotFixBaseInheritMono
{
    /// <summary> 受适配影响的页面主体 </summary>
    public RectTransform main;
    /// <summary> 播放动画的主体 </summary>
    public UISwitchAnimation animationMian;
    /// <summary> 关闭按钮 </summary>
    public GameObject btnClose;
    /// <summary> 蒙层 </summary>
    public GameObject maskLayer;
    /// <summary> 点击蒙层是否关闭Node </summary>
    public bool isClickMaskClose = false;
    /// <summary> 是否响应设备键盘 </summary>
    public bool isRespondDeviceKeyboard = false;

    [HideInInspector]
    public string nodePath, directoryPath;
    protected AssetBundle spriteAB;

    public virtual void Init()
    {
        LogUtils.Log(name + "：Init");
        if (isClickMaskClose && maskLayer)
            UGUIEventListener.Get(maskLayer).onClick += delegate { Close(); };
        if (btnClose)
            UGUIEventListener.Get(btnClose).onClick += delegate { Close(); };
        gameObject.SetActive(false);
    }

    public virtual void Open()
    {
        LogUtils.Log(name + "：Open");
        gameObject.SetActive(true);
    }

    /// <summary>
    /// UI入场动画播放完毕后执行，无动画则立刻执行
    /// </summary>
    public virtual void EnterAnimationEndAction()
    {
        RequestData();
    }

    /// <summary>
    /// UI离场动画播放完毕后执行，无动画则立刻执行
    /// </summary>
    public virtual void ExitAnimationEndAction() { }

    /// <summary>
    /// 请求数据专用
    /// </summary>
    public virtual void RequestData() { }

    /// <summary>
    /// 播放入场或离场动画
    /// </summary>
    /// <param name="isEnter">true为入场动画，false为离场动画</param>
    /// <param name="isCloseAfterFinish">是否在动画播放完毕后执行Close，仅isEnter = false时有效</param>
    public bool PlayAnimation(bool isEnter, UnityAction callback = null)
    {
        if (isEnter)
        {
            callback += EnterAnimationEndAction;
            if (!animationMian)
            {
                callback?.Invoke();
                return true;
            }
            return animationMian.PlayEnterAnimation(callback);
        }
        else
        {
            callback += ExitAnimationEndAction;
            if (!animationMian)
            {
                callback?.Invoke();
                return true;
            }
            return animationMian.PlayExitAnimation(callback);
        }
    }

    /// <summary>
    /// 关闭Node
    /// </summary>
    /// <param name="isActiveClose">是否为主动关闭</param>
    public virtual void Close(bool isActiveClose = true)
    {
        LogUtils.Log(name + "：Close");
        if (!this)
            return;
        bool closeSuccess = false;
        if (isActiveClose && animationMian && animationMian.exitAnimationType != NodeSwitchAnimationType.None)
        {
            closeSuccess = PlayAnimation(false, () => CloseNode(isActiveClose));
        }
        else
        {
            CloseNode(isActiveClose);
            closeSuccess = true;
        }

        if (closeSuccess)
        {
            NodeManager.currentNode = NodeManager.GetLastNode(false, false);
        }
    }

    void CloseNode(bool isActiveClose)
    {
        DestroyImmediate(gameObject);
    }

    void OnDestroy()
    {
        if (spriteAB)
            spriteAB.Unload(true);
        Resources.UnloadUnusedAssets();
    }

    #region ...工具函数
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
    #endregion
}