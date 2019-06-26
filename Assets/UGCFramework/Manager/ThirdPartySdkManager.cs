using LitJson;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using protocol;
using System.Collections;
using System;

public class ThirdPartySdkManager : MonoBehaviour
{
    static ThirdPartySdkManager instance;
    public static ThirdPartySdkManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject();
                instance = go.AddComponent<ThirdPartySdkManager>();
                go.name = instance.GetType().ToString();
                DontDestroyOnLoad(go);
                instance.Init();
            }
            return instance;
        }
    }
    public bool isRegisterToWechat = false;
    public bool isRegisterToQQ = false;
    public AndroidJavaClass tool;
    public AndroidJavaObject currentActivity;

    void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        tool = new AndroidJavaClass("com.my.ugcf.Tool");
#endif
        RegisterAppWechat();
        RegisterAppQQ();
    }

    #region ...wechat
    public const string WXAppID = "你的微信APPID";
    const string WXAppSecret = "你的微信AppSecret";

#if UNITY_IOS
    [DllImport("__Internal")]
    static extern void RegisterApp_iOS(string appId, string appSecret);
    [DllImport("__Internal")]
    static extern bool IsWechatInstalled_iOS();
    [DllImport("__Internal")]
    static extern void OpenWechat_iOS(string state);
#endif

    /// <summary> 注册微信 </summary>
    public bool RegisterAppWechat()
    {
        if (!isRegisterToWechat)
        {
#if UNITY_EDITOR
            isRegisterToWechat = true;
#elif UNITY_IOS
            RegisterApp_iOS(WXAppID, WXAppSecret);
#elif UNITY_ANDROID
            tool.CallStatic<bool>("RegisterToWechat", currentActivity, WXAppID, WXAppSecret);
#endif
        }
        return isRegisterToWechat;
    }

    /// <summary> 是否安装了微信 </summary>
    public bool IsWechatInstalled()
    {
        bool isRegister = isRegisterToWechat;
#if UNITY_EDITOR
        isRegister = false;
#else
#if UNITY_IOS
        isRegister = IsWechatInstalled_iOS();
#elif UNITY_ANDROID
        //isRegister = tool.CallStatic<bool>("IsWechatInstalled");
        isRegister = true;
#endif
#endif
        return isRegister;
    }

    #region ...微信登录
    /// <summary> 微信登录 </summary>
    public void WechatLogin()
    {
#if UNITY_EDITOR

#elif UNITY_IOS
        OpenWechat_iOS("app_wechat");
#elif UNITY_ANDROID
        AndroidJavaClass loginC = new AndroidJavaClass("com.my.ugcf.wechat.WechatLogin");
        loginC.CallStatic("LoginWeChat", "app_wechat");//后期改为随机数加session来校验
#endif
    }

    /// <summary> 微信登录回调 </summary>
    public void WechatLoginCallback(string callBackInfo)
    {
        //openid	    普通用户的标识，对当前开发者帐号唯一
        //nickname	    普通用户昵称
        //sex	        普通用户性别，1为男性，2为女性
        //province	    普通用户个人资料填写的省份
        //city	        普通用户个人资料填写的城市
        //country	    国家，如中国为CN
        //headimgurl    用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空
        //privilege	    用户特权信息，json数组，如微信沃卡用户为（chinaunicom）
        //unionid	    用户统一标识。针对一个微信开放平台帐号下的应用，同一用户的unionid是唯一的。多app数据互通保存该值
        //access_token  用户当前临时token值，自主添加的值
        if (!string.IsNullOrEmpty(callBackInfo))
        {
            JsonData jd = JsonMapper.ToObject(callBackInfo);
            if (!string.IsNullOrEmpty(jd.TryGetString("errcode")))
            {
                TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.LOGIN_FAIL);
                LoadingPnl.CloseLoading();
                return;
            }
            //TODO 登录 callBackInfo
        }
        else
            TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.LOGIN_FAIL);
    }
    #endregion

    #region ...微信支付
    // <summary> 发起微信支付请求 </summary>
    public void SendWechatPay(string payCode)
    {
#if UNITY_EDITOR

#elif UNITY_ANDROID
        JsonData jd = JsonMapper.ToObject(payCode);
        AndroidJavaClass utils = new AndroidJavaClass("com.my.ugcf.wechat.WechatPay");
        utils.CallStatic("SendPay", jd.TryGetString("appid"), jd.TryGetString("partnerid"), jd.TryGetString("prepayid"),
            jd.TryGetString("noncestr"), jd.TryGetString("timestamp"), jd.TryGetString("package"), jd.TryGetString("sign"));
#endif
    }

    /// <summary> 支付回调 </summary>
    public void WechatPayCallback(string retCode)
    {
        switch (int.Parse(retCode))
        {
            case -2:
                TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.PAY_RESULT_CANCEL);
                break;
            case -1:
                TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.PAY_RESULT_FAILE);
                break;
            case 0:
                TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.PAY_RESULT_SUCCESS);
                break;
        }
    }
    #endregion

    #endregion

    #region ...QQ
    public const string QQAppID = "你的QQAppID";

#if UNITY_IOS
    [DllImport("__Internal")]
    static extern void InitQQ(string appId);
    [DllImport("__Internal")]
    static extern bool IsQQInstalled_iOS();
    [DllImport("__Internal")]
    static extern void LoginByQQ();
#endif

    public bool IsQQInstalled()
    {
        bool isRegister = isRegisterToQQ;
#if UNITY_EDITOR
        isRegister = false;
#else
#if UNITY_IOS
        isRegister = IsQQInstalled_iOS();
#elif UNITY_ANDROID
        isRegister = tool.CallStatic<bool>("IsQQInstalled", currentActivity);
#endif
#endif
        return isRegister;
    }

    /// <summary> 注册QQ </summary>
    public void RegisterAppQQ()
    {
        if (!isRegisterToQQ)
        {
#if UNITY_EDITOR

#elif UNITY_IOS
            InitQQ(QQAppID);
#elif UNITY_ANDROID
            tool.CallStatic<bool>("RegisterToQQ", currentActivity, QQAppID);
#endif
            isRegisterToQQ = true;
        }
    }

    public void QQLogin()
    {
        if (!isRegisterToQQ)
            return;
#if UNITY_EDITOR

#elif UNITY_IOS
        LoginByQQ();
#elif UNITY_ANDROID
        AndroidJavaClass loginC = new AndroidJavaClass("com.my.ugcf.qq.QQLogin");
        loginC.CallStatic("LoginQQ");
#endif
    }

    public void QQLoginCallback(string callBackInfo)
    {
        //openid	    普通用户的标识，对当前开发者帐号唯一
        //nickname	    普通用户昵称
        //sex	        普通用户性别，1为男性，2为女性
        //figureurl_qq_1	    大小为40×40像素的QQ头像URL
        //figureurl_qq_2	    普大小为100×100像素的QQ头像URL。需要注意，不是所有的用户都拥有QQ的100x100的头像，但40x40像素则是一定会有
        if (!string.IsNullOrEmpty(callBackInfo))
        {
            //TODO 登录
        }
        else
            TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.LOGIN_FAIL);
    }
    #endregion

    #region ...alipay
    public const string AlipayAppID = "你的AlipayAppID";

    // <summary> 发起支付宝支付请求 </summary>
    public void SendAliPay(string payCode)
    {
#if UNITY_EDITOR

#elif UNITY_ANDROID
        AndroidJavaObject utils = new AndroidJavaObject("com.my.ugcf.alipay.AliPay");
        utils.Call("SendPay", payCode, currentActivity);
#endif
    }

    /// <summary> 支付回调 </summary>
    public void AliPayCallback(string paySuccess)
    {
        bool payResult = bool.Parse(paySuccess);
        TipManager.Instance.OpenTip(TipType.SimpleTip, payResult ? ConstantUtils.PAY_RESULT_SUCCESS : ConstantUtils.PAY_RESULT_FAILE);
    }
    #endregion

    #region ...苹果支付
#if UNITY_IOS
    [DllImport("__Internal")]
    static extern void RequestApplePay(string skuId);
#endif

    public void SendApplyPay(string skuId)
    {
#if UNITY_IOS
        RequestApplePay(skuId);
#endif
    }

    public void ApplePayCallBack(string result)
    {
#if UNITY_IOS
        if (result.Length == 1)
        {
            int code = int.Parse(result);
            TipManager.Instance.OpenTip(TipType.SimpleTip, code == 0 ? ConstantUtils.PAY_RESULT_FAILE : ConstantUtils.PAY_RESULT_CANCEL);
        }
        else
        {
            LogUtils.Log(result);
            TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.PAY_RESULT_SUCCESS);
        }
#endif
    }
    #endregion

    #region 其他功能
#if UNITY_IOS
    [DllImport("__Internal")]
    static extern void CopyTextToClipboard_iOS(string input);
    [DllImport("__Internal")]
    static extern float GetBattery_iOS();
#endif
    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    public void CopyToClipboard(string input)
    {
#if UNITY_EDITOR
        TextEditor t = new TextEditor();
        t.text = input;
        t.OnFocus();
        t.Copy();
#elif UNITY_IOS
        CopyTextToClipboard_iOS(input);  
#elif UNITY_ANDROID
        tool.CallStatic("CopyTextToClipboard", currentActivity, input);
#endif
    }

    /// <summary>
    /// 获取电量
    /// </summary>
    public int GetBattery()
    {
#if UNITY_EDITOR
        return 50;
#elif UNITY_IOS
        return (int)(GetBattery_iOS() * 100);
#elif UNITY_ANDROID
        return tool.CallStatic<int>("GetBattery");
#endif
    }

    /// <summary>
    /// 判断文件路径是否存在，主要针对安卓真机
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool FileExistByStreaming(string path)
    {
#if UNITY_EDITOR
        return File.Exists(path);
#elif UNITY_ANDROID
        string androidPath = path.Replace("jar:file://" + Application.dataPath + "!/assets/", "");
        return tool.CallStatic<bool>("FileExist", androidPath);
#else
        return File.Exists(path);
#endif
    }
    #endregion
}

public enum WechatErrorCode
{
    Success = 0,
    ErrorCommon = -1,
    ErrorUserCancel = -2,
    ErrorSentFail = -3,
    ErrorAuthDenied = -4,
    ErrorUnsupport = -5,
    ErrorBan = -6,
}

public enum QQErrorCode
{
    Success = 0,
    UserCancel,
    Failure
}