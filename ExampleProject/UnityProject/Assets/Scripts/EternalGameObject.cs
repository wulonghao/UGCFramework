using System.Collections;
using System.IO;
using UGCF.HotUpdate;
using UGCF.Manager;
using UGCF.Utils;
using UnityEngine;

public class EternalGameObject : HotFixBaseInheritMono
{
    public static AssetBundle commonBasicspriteAb, commonButtonAb, fontDefault, imageColorChangeAb, imageColorChangeVAb;
    [SerializeField] bool isNeverSleep = true;


    void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = isNeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        Application.logMessageReceived += LogUtils.LogToFile;
    }

    void Start()
    {
        ILRuntimeUtils.LoadHotFixWithInit();
        LogUtils.Log(Application.persistentDataPath);
        LoadNecessaryBundle();
        PageManager.OpenPage<StartPage>();
    }

    /// <summary> 加载必要资源 </summary>
    public static void LoadNecessaryBundle()
    {
        if (TryInvokeStaticHotFix(out object ob, null))
            return;
        commonBasicspriteAb = LoadBundle(commonBasicspriteAb, "Basicsprite", 1);
        commonButtonAb = LoadBundle(commonButtonAb, "Button", 1);

        fontDefault = LoadBundle(fontDefault, "Default", 2);

        imageColorChangeAb = LoadBundle(imageColorChangeAb, "imagecolorchange", 3);
        imageColorChangeVAb = LoadBundle(imageColorChangeVAb, "imagecolorchangev", 3);
    }

    /// <summary>
    /// 加载公共Bundle，若已加载，则Unload后重新加载
    /// </summary>
    /// <param name="assetBundle">存储Bundle引用的Object</param>
    /// <param name="bundleName">bundle的相对路径名</param>
    /// <param name="type">bundle类型 1-sprite, 2-Font, 3-Material, 4-RenderTexture</param>
    static AssetBundle LoadBundle(AssetBundle assetBundle, string bundleName, int type)
    {
        if (TryInvokeStaticHotFix(out object ob, assetBundle, bundleName, type))
            return (AssetBundle)ob;

        if (assetBundle)
            assetBundle.Unload(false);
        switch (type)
        {
            case 1://公共图片
                assetBundle = BundleManager.Instance.GetSpriteBundle("Common/" + bundleName);
                break;
            case 2://公共字体
                assetBundle = BundleManager.Instance.GetBundle("Font/" + bundleName);
                break;
            case 3://公共材质
                assetBundle = BundleManager.Instance.GetBundle("Shader/" + bundleName);
                if (assetBundle)
                {
                    Material material = assetBundle.LoadAsset<Material>(bundleName);
                    material.shader = Shader.Find(material.shader.name);
                }
                break;
            case 4://RenderTexture
                assetBundle = BundleManager.Instance.GetBundle("RenderTexture/" + bundleName);
                break;
        }
        return assetBundle;
    }
}