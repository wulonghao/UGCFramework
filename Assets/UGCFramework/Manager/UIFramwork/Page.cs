using LitJson;
using System;
using UnityEngine;

public partial class Page : HotFixBaseInheritMono
{
    /// <summary> 受适配影响的页面主体 </summary>
    public RectTransform main;
    [HideInInspector]
    public string resourceDirectory;
    protected AssetBundle spriteAB;

    public void InitData(AssetBundle ab, string resourceDirectory)
    {
        spriteAB = ab;
        this.resourceDirectory = resourceDirectory;
        gameObject.name = gameObject.name.Replace("(Clone)", "");
    }

    public virtual void Init()
    {
        LogUtils.Log(name + "：Init");
        gameObject.SetActive(false);
    }

    public virtual void Open()
    {
        LogUtils.Log(name + "：Open");
        TipManager.Instance.CloseAllTip();
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        LogUtils.Log(name + "：Close");
        gameObject.SetActive(false);
        AudioManager.Instance.StopMusic();
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
        return BundleManager.Instance.GetCommonJsonData(jsonName, resourceDirectory);
    }

    public T OpenFloatNode<T>() where T : Node
    {
        T t = NodeManager.OpenFloatNode<T>(resourceDirectory + "/Prefab");
        if (!t)
            t = NodeManager.OpenFloatNode<T>();
        return t;
    }

    public T OpenNode<T>(bool isAutoPlayEnter = true, bool isCloseLastNode = true) where T : Node
    {
        T t = NodeManager.OpenNode<T>(isAutoPlayEnter, isCloseLastNode, resourceDirectory + "/Prefab");
        if (!t)
            t = NodeManager.OpenNode<T>();
        return t;
    }

    public GameObject OpenPrefab(string gameObjectName)
    {
        return BundleManager.Instance.GetGameObjectByUI(resourceDirectory + "/Prefab/" + gameObjectName);
    }
}