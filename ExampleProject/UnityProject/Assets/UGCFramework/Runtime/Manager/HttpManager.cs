using System;
using System.Collections;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Networking;

public class HttpManager : MonoBehaviour
{
    /// <summary> 
    /// 发起带参的Get请求
    /// </summary>
    public static void SendHttpGetRequest(string url, Action<UnityWebRequest> requestSuccessCallback, Action<UnityWebRequest> requestFailCallback = null, int timeout = 20)
    {
        if (!MiscUtils.IsNetworkConnecting() || string.IsNullOrEmpty(url))
        {
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(null);
            return;
        }
        UGCFMain.Instance.StartCoroutine(SendHttpGetRequestAc(url, requestSuccessCallback, requestFailCallback, timeout));
    }

    /// <summary> 
    /// 发起带参的Post请求
    /// </summary>
    public static void SendHttpPostRequest(string url, WWWForm wwwf, Action<UnityWebRequest> requestSuccessCallback = null, Action<UnityWebRequest> requestFailCallback = null, int timeout = 20)
    {
        if (!MiscUtils.IsNetworkConnecting() || string.IsNullOrEmpty(url))
        {
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(null);
            return;
        }
        UGCFMain.Instance.StartCoroutine(SendHttpPostRequestAc(url, wwwf, requestSuccessCallback, requestFailCallback, timeout));
    }

    /// <summary> 
    /// 发起带参的Put请求
    /// </summary>
    public static void SendHttpPutRequest(string url, string bytes, Action<UnityWebRequest> requestSuccessCallback = null, Action<UnityWebRequest> requestFailCallback = null, int timeout = 20)
    {
        if (!MiscUtils.IsNetworkConnecting() || string.IsNullOrEmpty(url))
        {
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(null);
            return;
        }
        UGCFMain.Instance.StartCoroutine(SendHttpPutRequestAc(url, bytes, requestSuccessCallback, requestFailCallback, timeout));
    }

    private static IEnumerator SendHttpGetRequestAc(string url, Action<UnityWebRequest> requestSuccessCallback, Action<UnityWebRequest> requestFailCallback, int timeout)
    {
        LogUtils.Log("发起Get请求，URL：" + url);
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        uwr.timeout = timeout;
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            LogUtils.LogError(uwr.error + "，异常地址：" + url);
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(uwr);
        }
        else
        {
            if (requestSuccessCallback != null && requestSuccessCallback.Target != null)
                requestSuccessCallback(uwr);
        }
    }

    private static IEnumerator SendHttpPostRequestAc(string url, WWWForm wwwf, Action<UnityWebRequest> requestSuccessCallback, Action<UnityWebRequest> requestFailCallback, int timeout)
    {
        LogUtils.Log("发起Post请求，URL：" + url);
        UnityWebRequest uwr = UnityWebRequest.Post(url, wwwf);
        uwr.timeout = timeout;
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            LogUtils.LogError(uwr.error + "，异常地址：" + url);
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(uwr);
        }
        else
        {
            if (requestSuccessCallback != null && requestSuccessCallback.Target != null)
                requestSuccessCallback(uwr);
        }
    }

    private static IEnumerator SendHttpPutRequestAc(string url, string bytes, Action<UnityWebRequest> requestSuccessCallback, Action<UnityWebRequest> requestFailCallback, int timeout)
    {
        LogUtils.Log("发起Put请求，URL：" + url);
        UnityWebRequest uwr = UnityWebRequest.Put(url, bytes);
        uwr.method = "POST";
        uwr.timeout = timeout;
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            LogUtils.LogError(uwr.error + "，异常地址：" + url);
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(uwr);
        }
        else
        {
            if (requestSuccessCallback != null && requestSuccessCallback.Target != null)
                requestSuccessCallback(uwr);
        }
    }
}
