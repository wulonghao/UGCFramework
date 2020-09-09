using LitJson;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SignInWithApple;
using UGCF.Utils;

namespace UGCF.Manager
{
    public class ThirdPartySdkManager : MonoBehaviour
    {
        static ThirdPartySdkManager instance;
        public static ThirdPartySdkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<ThirdPartySdkManager>();
                    instance.name = instance.GetType().Name;
                    DontDestroyOnLoad(instance);
                    instance.Init();
                }
                return instance;
            }
        }
        public bool IsRegisterToWechat { get; set; }
        public bool IsRegisterToQQ { get; set; }
        public AndroidJavaClass Tool { get; set; }
        public AndroidJavaObject CurrentActivity { get; set; }

        void Init()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            CurrentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            Tool = new AndroidJavaClass(ConstantUtils.BundleIdentifier + ".Tool");
#endif
            RegisterAppWechat();
            RegisterAppQQ();
        }

        #region ...wechat
        private const string WXAppID = "你的微信APPID";
        private const string WXAppSecret = "你的微信AppSecret";

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void RegisterApp_iOS(string appId, string appSecret);
        [DllImport("__Internal")]
        static extern bool IsWechatInstalled_iOS();
        [DllImport("__Internal")]
        static extern void OpenWechat_iOS(string state);
#elif UNITY_ANDROID
        private static string WechatToolStr = ConstantUtils.BundleIdentifier + ".wechat.WechatTool";
        private static string WechatLoginStr = ConstantUtils.BundleIdentifier + ".wechat.WechatLogin";
        private static string WechatPayStr = ConstantUtils.BundleIdentifier + ".wechat.WechatPay";
#endif

        /// <summary> 注册微信 </summary>
        public bool RegisterAppWechat()
        {
            if (!IsRegisterToWechat)
            {
#if UNITY_EDITOR
                IsRegisterToWechat = true;
#elif UNITY_IOS
            RegisterApp_iOS(WXAppID, WXAppSecret);
#elif UNITY_ANDROID
            AndroidJavaClass wechatTool = new AndroidJavaClass(WechatToolStr);
            wechatTool.CallStatic<bool>("RegisterToWechat", CurrentActivity, WXAppID, WXAppSecret);
#endif
            }
            return IsRegisterToWechat;
        }

        /// <summary> 是否安装了微信 </summary>
        public bool IsWechatInstalled()
        {
            bool isRegister = IsRegisterToWechat;
#if UNITY_EDITOR
            isRegister = false;
#elif UNITY_IOS
        isRegister = IsWechatInstalled_iOS();
#elif UNITY_ANDROID
        AndroidJavaClass wechatTool = new AndroidJavaClass(WechatToolStr);
        isRegister = wechatTool.CallStatic<bool>("IsWechatInstalled");
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
        AndroidJavaClass loginC = new AndroidJavaClass(WechatLoginStr);
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
                    return;
                }
                //TODO 登录
            }
            else
                TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.LOGIN_FAIL);//登录失败请重试
        }
        #endregion

        #region ...微信支付

#if UNITY_ANDROID
        Queue<string> wechatPayQueues = new Queue<string>();
        // <summary> 发起微信支付请求 </summary>
        public void SendWechatPay(string payCode, string orderNum, bool qrCode)
        {
#if !UNITY_EDITOR
        JsonData jd = JsonMapper.ToObject(payCode);
        string appId = jd.TryGetString("appid");
        string partKey = jd.TryGetString("partnerid");
        string prePayId = jd.TryGetString("prepayid");
        string nonceStr = jd.TryGetString("noncestr");
        string mch_id = jd.TryGetString("mch_id");
        string price = jd.TryGetString("price");
        string title = jd.TryGetString("title");
        string qrUrl = jd.TryGetString("code_url");
        AndroidJavaClass utils = new AndroidJavaClass(WechatPayStr);
        utils.CallStatic("SendPay", appId, partKey, prePayId, nonceStr, jd.TryGetString("timestamp"), jd.TryGetString("package"), jd.TryGetString("sign"));
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
#endif

        #endregion

        #endregion

        #region ...QQ
        private const string QQAppID = "你的QQAppID";

#if UNITY_IOS
    [DllImport("__Internal")]
    static extern void InitQQ(string appId);
    [DllImport("__Internal")]
    static extern bool IsQQInstalled_iOS();
    [DllImport("__Internal")]
    static extern void LoginByQQ();
#elif UNITY_ANDROID
        private static string QQToolStr = ConstantUtils.BundleIdentifier + ".qq.QQTool";
        private static string QQLoginStr = ConstantUtils.BundleIdentifier + ".qq.QQLogin";
#endif

        public bool IsQQInstalled()
        {
            bool isRegister = IsRegisterToQQ;
#if UNITY_EDITOR
            isRegister = false;
#else
#if UNITY_IOS
        isRegister = IsQQInstalled_iOS();
#elif UNITY_ANDROID
        AndroidJavaClass qqTool = new AndroidJavaClass(QQToolStr);
        isRegister = qqTool.CallStatic<bool>("IsQQInstalled", CurrentActivity);
#endif
#endif
            return isRegister;
        }

        /// <summary> 注册QQ </summary>
        public void RegisterAppQQ()
        {
            if (!IsRegisterToQQ)
            {
#if UNITY_EDITOR

#elif UNITY_IOS
            InitQQ(QQAppID);
#elif UNITY_ANDROID
            AndroidJavaClass qqTool = new AndroidJavaClass(QQToolStr);
            qqTool.CallStatic<bool>("RegisterToQQ", CurrentActivity, QQAppID);
#endif
                IsRegisterToQQ = true;
            }
        }

        public void QQLogin()
        {
            if (!IsRegisterToQQ)
                return;
#if UNITY_EDITOR

#elif UNITY_IOS
        LoginByQQ();
#elif UNITY_ANDROID
        AndroidJavaClass loginC = new AndroidJavaClass(QQLoginStr);
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

        #region ...ali
        #region ...alipay

#if UNITY_ANDROID
        private const string AlipayAppID = "你的AlipayAppID";
        // <summary> 发起支付宝支付请求 </summary>
        public void SendAliPay(string payCode)
        {
#if !UNITY_EDITOR
        AndroidJavaObject utils = new AndroidJavaObject(ConstantUtils.BundleIdentifier + ".alipay.AliPay");
        utils.Call("SendPay", payCode, CurrentActivity);
#endif
        }

        /// <summary> 支付回调 </summary>
        public void AliPayCallback(string paySuccess)
        {
            bool payResult = bool.Parse(paySuccess);
            TipManager.Instance.OpenTip(TipType.SimpleTip, payResult ? ConstantUtils.PAY_RESULT_SUCCESS : ConstantUtils.PAY_RESULT_FAILE);
        }
#endif
        #endregion

        #region ...本机号码一键登录
        private AndroidJavaObject oneClickLogin;

        /// <summary>
        /// 初始化一键登录界面
        /// </summary>
        public void InitOneClick()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        if(oneClickLogin == null)
            oneClickLogin = new AndroidJavaObject(ConstantUtils.BundleIdentifier + ".oneclick.OneClickLogin");
        oneClickLogin.Call("init");
#endif
        }

        /// <summary>
        /// 打开一键登录界面
        /// </summary>
        public void OpenOneClickLoginPage()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        oneClickLogin.Call("openLoginPage");
#endif
        }

        public void OneClickCallback(string token)
        {
            if (token.Length > 100)
            {
                //TODO 登录
            }
            else
            {
                switch (token)
                {
                    case "600013":
                    case "600014":
                        TipManager.Instance.OpenTip(TipType.AlertTip, ConstantUtils.ONE_CLICK_LOGIN_4);
                        break;
                    case "600021":
                        TipManager.Instance.OpenTip(TipType.AlertTip, ConstantUtils.ONE_CLICK_LOGIN_5);
                        break;
                    case "700000":
                    case "700001":
                        //点击返回 用户取消免密登录
                        break;
                    default:
                        TipManager.Instance.OpenTip(TipType.AlertTip, ConstantUtils.ONE_CLICK_LOGIN_1);
                        break;
                }
            }
        }
        #endregion
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
        RuntimeTool.RecordTrackByHttp("收到苹果支付返回结果，凭据长度：" + result.Length);
        if (result.Length == 1)
        {
            int code = int.Parse(result);
            TipManager.Instance.OpenTip(TipType.AlertTip, code == 0 ? ConstantUtils.PAY_RESULT_FAILE : ConstantUtils.PAY_RESULT_CANCEL);
        }
        else
        {
            LogUtils.Log(result);
            TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.PAY_RESULT_SUCCESS);
        }
#endif
        }
        #endregion

        #region ...苹果登录   

        /// <summary>
        /// 苹果登录
        /// </summary>
        public void LoginWithApple()
        {
            if (!gameObject.GetComponent<SignInWithApple>())
            {
                gameObject.AddComponent<SignInWithApple>();
            }
            GetComponent<SignInWithApple>().Login(OnAppleLogin);
        }

        /// <summary>
        /// 苹果登录回调
        /// </summary>
        /// <param name="args"></param>
        private void OnAppleLogin(SignInWithApple.CallbackArgs args)
        {
            if (args.error != null || string.IsNullOrEmpty(args.userInfo.userId))
            {
                TipManager.Instance.OpenTip(TipType.AlertTip, ConstantUtils.LOGIN_FAIL, 0, LoginWithApple);//登录失败请重试
            }
            else
            {
                //TODO 登录
            }
        }
        #endregion

        #region ...其他功能
#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void CopyTextToClipboard_iOS(string input);
#endif
        public string GetAndroid_OAID()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        return Tool.CallStatic<string>("GetOAID");
#else
            return string.Empty;
#endif
        }

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
            Tool.CallStatic("CopyTextToClipboard", input);
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
            return Tool.CallStatic<bool>("FileExist", androidPath);
#else
            return File.Exists(path);
#endif
        }

        public string GetFileByStreaming(string path)
        {
            try
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                string androidPath = path.Replace("jar:file://" + Application.dataPath + "!/assets/", "");
                return Tool.CallStatic<string>("GetTextFile", androidPath);
#else
                if (File.Exists(path))
                    return File.ReadAllText(path);
                else
                    return null;
#endif
            }
            catch
            {
                LogUtils.Log("文件读取失败！path：" + path);
                return null;
            }
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
}