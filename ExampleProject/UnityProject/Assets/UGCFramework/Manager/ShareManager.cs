using System;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace UGCF.Manager
{
    public class ShareManager : MonoBehaviour
    {
        #region instance
        static ShareManager instance;
        public static ShareManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<ShareManager>();
                    instance.name = instance.GetType().ToString();
                    DontDestroyOnLoad(instance);
                    instance.Init();
                }
                return instance;
            }
        }
        #endregion
        public static string shareComponent;

        void Init()
        {
            ThirdPartySdkManager.Instance.RegisterAppWechat();
            ThirdPartySdkManager.Instance.RegisterAppQQ();
        }

        #region wechat
        public enum WechatShareScene
        {
            // 好友
            Friend = 0,
            // 朋友圈
            FriendRound = 1,
            // 收藏
            Favorite = 2,
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void ShareImageWx_iOS(int scene, IntPtr ptr, int size, IntPtr ptrThumb, int sizeThumb);
        [DllImport("__Internal")]
        static extern void ShareTextWx_iOS(int scene, string content);
        [DllImport("__Internal")]
        static extern void ShareUrlWx_iOS(int scene, string url, string title, string content, IntPtr ptrThumb, int sizeThumb);
#elif UNITY_ANDROID
        static string WeChatShareUtils = ConstantUtils.BundleIdentifier + ".wechat.WechatShareUtil";
#endif

        /// <summary>
        /// 分享微信回调
        /// </summary>
        public UnityAction<WechatErrorCode> ShareWechatCallback { get; set; }

        /// <summary>
        /// 分享图片
        /// </summary>
        /// <param name="scene">分享的场景</param>
        /// <param name="shareImage">分享的图片</param>
        public void WechatShareImage(WechatShareScene scene, Texture2D shareImage)
        {
#if !UNITY_EDITOR
            byte[] data = shareImage.EncodeToJPG();
            byte[] dataThumb = UIUtils.SizeTextureBilinear(shareImage, Vector2.one * 150).EncodeToJPG(40);
#if UNITY_IOS
            IntPtr array = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, array, data.Length);
            IntPtr arrayThumb = Marshal.AllocHGlobal(dataThumb.Length);
            Marshal.Copy(dataThumb, 0, arrayThumb, dataThumb.Length);
            ShareImageWx_iOS((int)scene, array, data.Length, arrayThumb, dataThumb.Length);
#elif UNITY_ANDROID
            AndroidJavaClass utils = new AndroidJavaClass(WeChatShareUtils);
            utils.CallStatic("ShareImage", (int)scene, data, dataThumb);
#endif
#endif
        }

        /// <summary>
        /// 分享文本
        /// </summary>
        /// <param name="scene">分享的场景</param>
        /// <param name="content">分享的文本内容</param>
        public void WechatShareText(WechatShareScene scene, string content)
        {
#if UNITY_IOS
            ShareTextWx_iOS((int)scene, content);
#elif UNITY_ANDROID
            AndroidJavaClass utils = new AndroidJavaClass(WeChatShareUtils);
            utils.CallStatic("ShareText", (int)scene, content);
#endif
        }

        /// <summary>
        /// 分享链接
        /// </summary>
        /// <param name="scene">分享的场景</param>
        /// <param name="url">分享的链接地址</param>
        /// <param name="title">分享链接的标题</param>
        /// <param name="shareThumbIcon">分享链接的缩略图</param>
        /// <param name="content">分享链接的文本描述</param>
        public void WechatShareUrl(WechatShareScene scene, string url, string title, Texture2D shareThumbIcon, string content)
        {
#if !UNITY_EDITOR
            byte[] thumb = UIUtils.SizeTextureBilinear(shareThumbIcon, Vector2.one * 150).EncodeToJPG(40);
#if UNITY_IOS
            IntPtr arrayThumb = Marshal.AllocHGlobal(thumb.Length);
            Marshal.Copy(thumb, 0, arrayThumb, thumb.Length);
            ShareUrlWx_iOS((int)scene, url, title, content, arrayThumb, thumb.Length);
#elif UNITY_ANDROID
            AndroidJavaClass utils = new AndroidJavaClass(WeChatShareUtils);
            utils.CallStatic("ShareUrl", (int)scene, url, title, content, thumb);
#endif
#endif
        }

        /// <summary>
        /// 微信分享回调，由原生逻辑自动调用
        /// </summary>
        /// <param name="errCode"></param>
        public void WechatCallBack(string errCode)
        {
            try
            {
                WechatErrorCode code = (WechatErrorCode)Enum.Parse(typeof(WechatErrorCode), errCode);
                if (code == WechatErrorCode.Success)
                {
                    TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.SHARE_SUCCESS);
                    if (ShareWechatCallback != null)
                    {
                        ShareWechatCallback(code);
                        ShareWechatCallback = null;
                    }
                }
                else
                {
                    TipManager.Instance.OpenTip(TipType.SimpleTip, code == WechatErrorCode.ErrorUserCancel ? ConstantUtils.SHARE_CANCEL : ConstantUtils.SHARE_FAILURE);
                }
            }
            catch
            {
                TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.SHARE_FAILURE);
            }
        }
        #endregion

        #region ...QQ
        /// <summary> QQ分享 回调 </summary>
        public UnityAction<QQErrorCode> ShareQQCallback { get; set; }

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void ShareImgByQQ(string imgPath);
        [DllImport("__Internal")]
        static extern void ShareUrlByQQ(string title, string desc, string iconUrl, string url);
#elif UNITY_ANDROID
        static string QQShareUtils = ConstantUtils.BundleIdentifier + ".qq.QQShareUtil";
#endif

        string printScreenPath = Application.persistentDataPath + "/shareIcon.jpg";
        /// <summary>
        /// 分享链接
        /// </summary>
        /// <param name="url">链接地址</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="shareThumbIcon">分享的缩略图</param>
        /// <param name="imagePath">分享显示的缩略图Path</param>
        public void QQShareUrl(string url, string title, string content, Texture2D shareThumbIcon = null, string imagePath = null)
        {
            if (string.IsNullOrEmpty(imagePath))
                imagePath = printScreenPath;
#if UNITY_IOS
            ShareUrlByQQ(title, content, imagePath, url);
#elif UNITY_ANDROID
            if (shareThumbIcon != null)
                MiscUtils.CreateBytesFile(imagePath, shareThumbIcon.GetRawTextureData());
            AndroidJavaClass utils = new AndroidJavaClass(QQShareUtils);
            utils.CallStatic("QQShareUrl", title, content, url, imagePath, Application.productName);
#endif
        }

        /// <summary>
        /// 分享纯图片
        /// </summary>
        /// <param name="imagePath">图片本地路径</param>
        public void QQShareImage(Texture2D shareImg)
        {
            if (MiscUtils.CreateBytesFile(printScreenPath, shareImg.EncodeToJPG()))
            {
#if UNITY_IOS
                ShareImgByQQ(ConstantUtils.PrintScreenPath);
#elif UNITY_ANDROID
                AndroidJavaClass utils = new AndroidJavaClass(QQShareUtils);
                utils.CallStatic("QQShareImage", printScreenPath, Application.productName);
#endif
            }
        }

        /// <summary>
        /// QQ分享回调，由原生逻辑自动调用
        /// </summary>
        /// <param name="errCode"></param>
        public void QQCallBack(string errCode)
        {
            QQErrorCode code = (QQErrorCode)int.Parse(errCode);
            switch (code)
            {
                case QQErrorCode.Success:
                    TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.SHARE_SUCCESS);//分享成功
                    if (ShareQQCallback != null)
                    {
                        ShareQQCallback(code);
                        ShareQQCallback = null;
                    }
                    break;
                case QQErrorCode.UserCancel:
                    TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.SHARE_CANCEL);//取消分享
                    break;
                case QQErrorCode.Failure:
                    TipManager.Instance.OpenTip(TipType.SimpleTip, ConstantUtils.SHARE_FAILURE);//分享失败
                    break;
            }
        }
        #endregion

        void OnDestroy()
        {
            instance = null;
        }

        public enum SharePlatform
        {
            QQ = 0,//qq
            Wechat,//微信
        }

        public enum ShareType
        {
            Image,
            Url,
            Text
        }
    }
}