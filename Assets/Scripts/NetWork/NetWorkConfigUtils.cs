using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetWorkConfigUtils
{
    public static string BundleDownLoadUrl
    {
        get
        {
            if (EternalGameObject.Instance.isLocalVersion)
            {
                if (Application.platform == RuntimePlatform.Android)
                    return "jar:file://" + Application.dataPath + "!/assets/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/" + MiscUtils.GetChannel() + "/";
                else
                    return "file://" + Application.streamingAssetsPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/" + MiscUtils.GetChannel() + "/";
            }
            else
                return "https://www.xxx.com/" + MiscUtils.GetCurrentPlatform() + "/" + MiscUtils.GetChannel() + "/";
        }
    }
}