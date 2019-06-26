using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EternalGameObject : MonoBehaviour
{
    public static EternalGameObject Instance;
    public RectTransform rootCanvas;
    [HideInInspector]
    public static AssetBundle commonBasicspriteAb, commonButtonAb, imageColorChangeAb, imageColorChangeVAb;

    public bool isDebugLog = true;
    public bool isLocalVersion = true;
    public bool isNeverSleep = true;

    public static float canvasWidth;
    public static float canvasHeight;
    public static float canvasWidthScale;
    public static float canvasHeightScale;
    public static float screenWidthScale;
    public static float screenHeightScale;
    public static int pixelWidth;
    public static int pixelHeight;

    public AndroidBuildChannel androidBuildChannel;
    public IOSBuildChannel iOSBuildChannel;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = isNeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        Application.logMessageReceived += LogUtils.LogToFile;
    }

    void Start()
    {
        ILRuntimeUtils.LoadHotFixWithInit();
        LogUtils.Log(Application.persistentDataPath);
        UIDataInit();
        LoadNecessaryBundle();
        PageManager.Instance.OpenPage<StartPage>();
    }

    void UIDataInit()
    {
        canvasWidth = rootCanvas.rect.width;
        canvasHeight = rootCanvas.rect.height;
        screenWidthScale = rootCanvas.localScale.x;
        screenHeightScale = rootCanvas.localScale.y;
        Vector2 vec2 = rootCanvas.GetComponent<CanvasScaler>().referenceResolution;
        pixelWidth = (int)vec2.x;
        pixelHeight = (int)vec2.y;
        canvasWidthScale = canvasWidth / pixelWidth;
        canvasHeightScale = canvasHeight / pixelHeight;
    }

    /// <summary> 加载必要资源 </summary>
    void LoadNecessaryBundle()
    {
        if (!commonBasicspriteAb) commonBasicspriteAb = BundleManager.Instance.GetSpriteBundle("Common/BasicSprite");
        if (!commonButtonAb) commonButtonAb = BundleManager.Instance.GetSpriteBundle("Common/Button");

        BundleManager.Instance.GetBundle("Font/Default");

        if (!imageColorChangeAb)
        {
            imageColorChangeAb = BundleManager.Instance.GetBundle("Shader/imagecolorchange");
            if (imageColorChangeAb)
            {
                Material imagecolorchangeMat = imageColorChangeAb.LoadAsset<Material>("imagecolorchange");
                imagecolorchangeMat.shader = Shader.Find(imagecolorchangeMat.shader.name);
            }
        }
        if (!imageColorChangeVAb)
        {
            imageColorChangeVAb = BundleManager.Instance.GetBundle("Shader/imagecolorchangev");
            if (imageColorChangeVAb)
            {
                Material imagecolorchangevMat = imageColorChangeVAb.LoadAsset<Material>("imagecolorchangev");
                imagecolorchangevMat.shader = Shader.Find(imagecolorchangevMat.shader.name);
            }
        }
    }
}

public enum AndroidBuildChannel
{
    Test,
    QiHu360,
    WanDouJia
}

public enum IOSBuildChannel
{
    Test,
    AppStore
}