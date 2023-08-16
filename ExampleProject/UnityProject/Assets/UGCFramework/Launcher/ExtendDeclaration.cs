using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ExtendDeclaration
{
    #region ...Share
#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void ShareImageWx_iOS(int scene, IntPtr ptr, int size, IntPtr ptrThumb, int sizeThumb);
    [DllImport("__Internal")]
    public static extern void ShareTextWx_iOS(int scene, string content);
    [DllImport("__Internal")]
    public static extern void ShareUrlWx_iOS(int scene, string url, string title, string content, IntPtr ptrThumb, int sizeThumb);

    [DllImport("__Internal")]
    public static extern void ShareImgByQQ(string imgPath);
    [DllImport("__Internal")]
    public static extern void ShareUrlByQQ(string title, string desc, string iconUrl, string url);
#endif
    #endregion

    #region ...Wechat
#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void RegisterApp_iOS(string appId, string appSecret);
    [DllImport("__Internal")]
    public static extern bool IsWechatInstalled_iOS();
    [DllImport("__Internal")]
    public static extern void OpenWechat_iOS(string state);
#endif
    #endregion

    #region ...QQ
#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void InitQQ(string appId);
    [DllImport("__Internal")]
    public static extern bool IsQQInstalled_iOS();
    [DllImport("__Internal")]
    public static extern void LoginByQQ();
#endif
    #endregion

    #region ...ApplePay
#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void RequestApplePay(string skuId);
#endif
    #endregion

    #region ...Tool
#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void CopyTextToClipboard_iOS(string input);
#endif
    #endregion

    #region ...SignInWithApple
#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void UnitySignInWithApple_Login(IntPtr callback);

    [DllImport("__Internal")]
    public static extern void UnitySignInWithApple_GetCredentialState(string userID, IntPtr callback);
#endif
    #endregion
}
