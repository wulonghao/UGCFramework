using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using XLua;

[Hotfix]
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
                GameObject go = new GameObject();
                instance = go.AddComponent<ShareManager>();
                go.name = instance.GetType().ToString();
                ThirdPartySdkManager.Instance.RegisterAppWechat();
            }
            return instance;
        }
    }

    void OnDestory()
    {
        instance = null;
    }
    #endregion

    #region wechat
    public enum WechatShareScene
    {
        // 好友
        WXSceneSession = 0,
        // 朋友圈
        WXSceneTimeline = 1,
        // 收藏
        WXSceneFavorite = 2,
    }
#if UNITY_IPHONE
    [DllImport("__Internal")]
    static extern void ShareImage_iOS(int scene, IntPtr ptr, int size, IntPtr ptrThumb, int sizeThumb);
    [DllImport("__Internal")]
    static extern void ShareText_iOS(int scene, string content);
    [DllImport("__Internal")]
    static extern void ShareUrl_iOS(int scene, string url, string title, string content, IntPtr ptrThumb, int sizeThumb);
#elif UNITY_ANDROID
    const string WeChatShareUtils = "com.my.ugcf.wechat.ShareUtils";
#endif

    /// <summary>
    /// 分享图片
    /// </summary>
    /// <param name="scene">分享的场景</param>
    /// <param name="shareImage">分享的图片</param>
    public void ShareImage(WechatShareScene scene, Texture2D shareImage)
    {
        byte[] data = shareImage.EncodeToJPG();
        byte[] dataThumb = MiscUtils.SizeTextureBilinear(shareImage, Vector2.one * 150).EncodeToJPG(40);
#if UNITY_IPHONE
		IntPtr array = Marshal.AllocHGlobal (data.Length);
		Marshal.Copy (data, 0, array, data.Length);
		IntPtr arrayThumb = Marshal.AllocHGlobal (dataThumb.Length);
		Marshal.Copy (dataThumb, 0, arrayThumb, dataThumb.Length);
		ShareImage_iOS ((int)scene, array, data.Length, arrayThumb, dataThumb.Length);
#elif UNITY_ANDROID
        AndroidJavaClass utils = new AndroidJavaClass(WeChatShareUtils);
        utils.CallStatic("ShareImage", (int)scene, data, dataThumb);
#endif
    }

    /// <summary>
    /// 分享文本
    /// </summary>
    /// <param name="scene">分享的场景</param>
    /// <param name="content">分享的文本内容</param>
    public void ShareText(WechatShareScene scene, string content)
    {
#if UNITY_IPHONE
        ShareText_iOS((int)scene, content);
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
    /// <param name="content">分享链接的文本描述</param>
    /// <param name="thumb">缩略图</param>
    public void ShareUrl(WechatShareScene scene, string url, string title, string content, Texture2D shareImageThumb)
    {
        byte[] thumb = MiscUtils.SizeTextureBilinear(shareImageThumb, Vector2.one * 150).EncodeToJPG(40);
#if UNITY_IPHONE
        IntPtr arrayThumb = Marshal.AllocHGlobal(thumb.Length);
        Marshal.Copy(thumb, 0, arrayThumb, thumb.Length);
        ShareUrl_iOS((int)scene, url, title, content, arrayThumb, thumb.Length);
#elif UNITY_ANDROID
        AndroidJavaClass utils = new AndroidJavaClass(WeChatShareUtils);
        utils.CallStatic("ShareWebPage", (int)scene, url, title, content, thumb);
#endif
    }

    public void WechatCallBack(string errCode)
    {
        WechatErrCode code = (WechatErrCode)int.Parse(errCode);
        TipManager.Instance.OpenTip(TipType.SimpleTip, code == WechatErrCode.Success ? "分享成功" : "分享失败");
    }

    #endregion

    void OnDestroy()
    {
        instance = null;
    }
}
