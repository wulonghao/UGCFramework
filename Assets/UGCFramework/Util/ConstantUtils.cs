using UnityEngine;
using System.Collections;

public partial class ConstantUtils
{
    public static string BundleIdentifier { get { return Application.identifier; } }
    public const string AudioBtnClick = "BtnClick";
    public const string BundleInfoConfigName = "Bundle.txt";
    public const string CommonResourcesFolderName = "CommonResources";
    public const string OtherSundryFolderName = "OtherSundry";

    public static string AssetBundleFolderPath = Application.persistentDataPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
    public static string StreamingAssetBundleFolderPath
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
                return "jar:file://" + Application.dataPath + "!/assets/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
            else
                return Application.streamingAssetsPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/";
        }
    }

    public static string ConfigFolderPath = Application.persistentDataPath + "/config/";
    public static string SpriteFolderPath = Application.persistentDataPath + "/sprite/";
    public static string ArchiveFolderPath = Application.persistentDataPath + "/archive/";
    public static string APKFolderPath = Application.persistentDataPath + "/apk/";
    public static string AudioFolderPath = Application.persistentDataPath + "/audio/";

    public const string LOGIN_FAIL = "登录失败";
    public const string SHARE_SUCCESS = "分享成功";
    public const string SHARE_FAILURE = "分享失败";
    public const string SHARE_CANCEL = "取消分享";
    public const string PAY_RESULT_CANCEL = "交易取消";
    public const string PAY_RESULT_FAILE = "交易失败";
    public const string PAY_RESULT_SUCCESS = "交易成功";
    public const string ONE_CLICK_LOGIN_1 = "当前无法使用一键登录功能，建议尝试其他登录方式";
    public const string ONE_CLICK_LOGIN_2 = "当前未检测到sim卡，请您插入sim卡后重试";
    public const string ONE_CLICK_LOGIN_3 = "检测到蜂窝网络未开启，请开启移动网络后重试";
    public const string ONE_CLICK_LOGIN_4 = "检测到运营商升级中，一键登录功能不可用，请尝试其他登录方式";
    public const string ONE_CLICK_LOGIN_5 = "检测到您点击登录按钮后切换过运营商，请您维持当前运营商不变，尝试重新登录";
}