using UnityEngine;
using System.Collections;
using XLua;

[Hotfix]
public class ConstantUtils
{
    public static string httpDownLoadUrl = "http://www.xxxx.com/" + MiscUtils.GetCurrentPlatform() + "/";
    public static string bundleTipsUrl = bundleDownLoadUrl + "Bundle.txt";
    public static string bundleDownLoadUrl
    {
        get
        {
            if (PageManager.Instance.isLocalVersion)
                if (Application.platform == RuntimePlatform.Android)
                    return "jar:file://" + Application.dataPath + "!/assets/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
                else
                    return "file://" + Application.streamingAssetsPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
            else
                return httpDownLoadUrl;
        }
    }

    public static string ConfigFolderPath = Application.persistentDataPath + "/config/";
    public static string AssetBundleFolderPath = Application.persistentDataPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
    public static string SpriteFolderPath = Application.persistentDataPath + "/sprite/";
    public static string ArchiveFolderPath = Application.persistentDataPath + "/Archive/";

    //语音文件存放地址
    public static string VoiceRecordPath = Application.persistentDataPath + "/Voice/Recording.dat";
    public static string VoiceDownloadPath = Application.persistentDataPath + "/Voice/Download.dat";

    //游戏json，bundle类型
    public static string errorCodeConfigName = "ErrorCodeConfig";
    public static string englishToChineseConfigName = "EnglishToChineseConfig";
    public static string gameDataConfigName = "GameDataConfig";

    //系统json
    public static string versionConfigPath = ConfigFolderPath + "Version.json";
    public static string setConfigPath = ConfigFolderPath + "Set.json";
    public static string loginConfigPath = ConfigFolderPath + "Login.json";
    public static string tonkenConfigPath = ConfigFolderPath + "Token.json";

    //bundle名
    public static string expressionBundleName = "chatexpression";
    public static string defaultHeadIconName = "def_head_icon";

    //存档

    //常用常量
    public static int const0 = 0;
    public static int const1 = 1;
    public static int const90 = 90;
    public static int const180 = 180;
    public static int const360 = 360;

    public static Vector3 vecForward90 = Vector3.forward * const90;
    public static Vector3 vecForward180 = Vector3.forward * const180;
    public static Vector3 vecForward360 = Vector3.forward * const360;
    public static Vector3 vecUp90 = Vector3.up * const90;
    public static Vector3 vecUp180 = Vector3.up * const180;
    public static Vector3 vecUp360 = Vector3.up * const360;

    public static WaitForEndOfFrame frameWait = new WaitForEndOfFrame();

    #region ...Tag
    #endregion

    #region ...Layer
    #endregion
}


