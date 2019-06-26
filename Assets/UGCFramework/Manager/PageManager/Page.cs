using LitJson;
using System;
using UnityEngine;

public class Page : HotFixBaseInheritMono
{
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
        gameObject.SetActive(false);
        LogUtils.Log(name + "：Init");
    }

    public virtual void Open()
    {
        TipManager.Instance.CloseAllTip();
        gameObject.SetActive(true);
        LogUtils.Log(name + "：Open");
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        AudioManager.Instance.StopMusic();
        LogUtils.Log(name + "：Close");
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

    public T OpenNode<T>(bool isAddToRecord = true, bool closeLastNode = true) where T : Node
    {
        T t = NodeManager.OpenNode<T>(isAddToRecord, closeLastNode, resourceDirectory + "/Prefab");
        if (!t)
            t = NodeManager.OpenNode<T>();
        return t;
    }

    public GameObject OpenPrefab(string gameObjectName)
    {
        return BundleManager.Instance.GetGameObjectByUI(resourceDirectory + "/Prefab/" + gameObjectName);
    }
}