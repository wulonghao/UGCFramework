using UnityEngine;
using System.Collections;

public class GlobalVariableUtils
{
    public static string LogErrorPath = Application.persistentDataPath + "/LogError/";
    public static string ConfigFolderPath = Application.persistentDataPath + "/config/";
    public static string AssetBundleFolderPath = Application.persistentDataPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
    public static string StreamingAssetBundleFolderPath
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
                return "jar:file://" + Application.dataPath + "!/assets/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/" + MiscUtils.GetChannel() + "/";
            else
                return Application.streamingAssetsPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/" + MiscUtils.GetChannel() + "/";
        }
    }
}