using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UGCF.Manager;
using UGCF.Utils;

public class LoadingPnl : MonoBehaviour
{
    public static LoadingPnl instance;
    public GameObject main;
    public Text describeTxt;

    /// <summary>
    /// 打开一个LoadingNode
    /// </summary>
    /// <param name="describe">描述</param>
    public static void OpenLoading(string describe = null)
    {
        if (!instance)
        {
            GameObject go = BundleManager.Instance.GetGameObject(typeof(LoadingPnl).ToString());
            if (go)
            {
                instance = go.GetComponent<LoadingPnl>();
                go.transform.SetParent(UGCFMain.Instance.RootCanvas);
                go.transform.localScale = Vector3.one;

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
            }
        }
        instance.gameObject.SetActive(true);
        instance.describeTxt.gameObject.SetActive(!string.IsNullOrEmpty(describe));
        instance.describeTxt.text = describe;
        if (instance.delayShow != null)
            instance.StopCoroutine(instance.delayShow);
        instance.delayShow = instance.StartCoroutine(DelayShow());

        if (instance.delayClose != null)
            instance.StopCoroutine(instance.delayClose);
        instance.delayClose = instance.StartCoroutine(DelayAutoClose());
    }

    Coroutine delayShow = null;
    static IEnumerator DelayShow()
    {
        yield return WaitForUtils.WaitForSecondsRealtime(0.5f);
        instance.main.SetActive(true);
    }

    Coroutine delayClose = null;
    static IEnumerator DelayAutoClose()
    {
        yield return WaitForUtils.WaitForSecondsRealtime(10);
        CloseLoading();
    }

    public static void CloseLoading()
    {
        if (instance != null)
            instance.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        instance.main.SetActive(false);
    }

    void OnDestroy()
    {
        instance = null;
    }
}

