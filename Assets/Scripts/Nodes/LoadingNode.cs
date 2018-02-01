using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using XLua;

public enum LoadingType
{
    Common,
    Progress
}

[Hotfix]
public class LoadingNode : Node
{
    public static LoadingNode instance;
    public GameObject commonLoadingPnl, downLoadingPnl;
    public RectTransform downLoadingProgressRtf;
    public Text describe, progressTxt;

    static int downLoadingWidth = 1300;
    static int downLoadingHeight;

    static string defaultLoadingDes = "加载中...";

    /// <summary>
    /// 打开一个LoadingNode
    /// </summary>
    /// <param name="type">Loading类型</param>
    /// <param name="describe">描述</param>
    /// <param name="progress">加载进度</param>
    public static void OpenLoadingNode(LoadingType type, string describe = null, float progress = 0, long fileAllLength = 0)
    {
        instance = NodeManager.OpenFloatNode<LoadingNode>();
        if (instance.transform.parent != PageManager.Instance.transform)
            NodeManager.DontDestroyAtChangePage(instance);
        switch (type)
        {
            case LoadingType.Common:
                instance.commonLoadingPnl.SetActive(true);
                instance.downLoadingPnl.SetActive(false);
                break;
            case LoadingType.Progress:
                instance.commonLoadingPnl.SetActive(false);
                instance.downLoadingPnl.SetActive(true);
                instance.downLoadingProgressRtf.sizeDelta = new Vector2(downLoadingWidth * progress, downLoadingHeight);
                string allLength = MiscUtils.GetMillionFromByte(fileAllLength).ToString("F2");
                string currlength = MiscUtils.GetMillionFromByte((long)(fileAllLength * progress)).ToString("F2");
                instance.progressTxt.text = "<color=#8CF976FF>" + currlength + "MB</color>/<color=#FEE646FF>" + allLength + "MB</color>";
                break;
        }
        if (!string.IsNullOrEmpty(describe))
            instance.describe.text = describe;
        else
            instance.describe.text = defaultLoadingDes;
    }

    public static void CloseLoadingNode()
    {
        if (instance != null)
            instance.Close();
    }

    public static bool IsActivity()
    {
        if (instance != null)
            return instance.gameObject.activeSelf;
        return false;
    }

    void OnEnable()
    {
        downLoadingHeight = (int)downLoadingProgressRtf.GetComponent<RectTransform>().rect.height;
    }

    void OnDestroy()
    {
        instance = null;
    }
}

