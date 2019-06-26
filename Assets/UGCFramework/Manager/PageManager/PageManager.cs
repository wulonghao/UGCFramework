using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class PageManager : MonoBehaviour
{
    public static PageManager Instance;
    // 当前Page
    [HideInInspector]
    public Page currentPage;
    // Page历史，用于返回时检索
    static List<string> pageHistory = new List<string>();
    const string DefaultPageDirectoryPath = "UIResources/Page";

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 根据Page子类类型打开特定的Page
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public T OpenPage<T>() where T : Page
    {
        return (T)OpenPageAc(GetPageType<T>());
    }

    public Page OpenPage(string pageName)
    {
        return OpenPageAc(pageName);
    }

    // 返回上一个Page
    public void OpenLastPage()
    {
        if (pageHistory.Count > 1)
        {
            string page = pageHistory[pageHistory.Count - 2].ToString();
            OpenPageAc(page.ToString());
        }
    }

    #region ...内部函数
    /// <summary>
    /// 打开特定类型的Page
    /// </summary>
    /// <param name="pagePath"></param>
    /// <param name="finishCallback"></param>
    Page OpenPageAc(string pagePath)
    {
        Page page = CreatePage(pagePath);
        if (page)
        {
            page.transform.SetAsFirstSibling();
            page.Open();
            if (currentPage != page)
                AudioManager.Instance.PlayMusic(Path.GetFileNameWithoutExtension(pagePath), DefaultPageDirectoryPath + "/" + pagePath);
        }
        return page;
    }

    /// <summary>
    /// 销毁当前的Page并刷新历史
    /// </summary>
    /// <param name="newPageName"></param>
    void DestroyCurrentPage(string newPageName)
    {
        if (currentPage != null)
        {
            DestroyPage(currentPage);
            NodeManager.ClearNodeHistory();
        }
        RefreshHistory(newPageName);
    }

    /// <summary>
    /// 销毁指定Page
    /// </summary>
    /// <param name="page"></param>
    void DestroyPage(Page page)
    {
        page.Close();
        if (currentPage.GetSpriteAB() != null)
            currentPage.GetSpriteAB().Unload(true);
        AudioManager.Instance.ClearAllTempAudio();
        Destroy(page.gameObject);
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 创建一个新的特定类型的Page
    /// </summary>
    /// <param name="pagePath"></param>
    /// <returns></returns>
    Page CreatePage(string pagePath)
    {
        if (string.IsNullOrEmpty(pagePath))
            return null;
        string pageName = Path.GetFileNameWithoutExtension(pagePath);
        if (currentPage != null && pageName == currentPage.name)
            return currentPage;

        Page page = GetComponentInChildren<Page>(true);
        if (page == null || page.name != pagePath)
        {
            string path = DefaultPageDirectoryPath + "/" + pagePath;
            AssetBundle ab = null;
            GameObject go = BundleManager.Instance.GetGameObjectByUI(path);
            if (!go)
            {
                ab = BundleManager.Instance.GetSpriteBundle("DefaultLoad", path);
                go = BundleManager.Instance.GetGameObjectByUI(path + "/Prefab/" + pageName);
                if (!go)
                {
                    LogUtils.LogError("找不到路径: " + path + "/Prefab/" + pageName);
                    return null;
                }
            }
            page = go.GetComponent<Page>();
            if (page)
            {
                UIUtils.AttachAndReset(go, transform);
                page.InitData(ab, path);

                DestroyCurrentPage(page.name);
                currentPage = page;
                page.Init();
            }
            else
            {
                LogUtils.LogError("找不到包含的Page组件: " + pageName);
                return null;
            }
        }
        return page;
    }

    /// <summary>
    /// 刷新Page历史
    /// </summary>
    /// <param name="type"></param>
    void RefreshHistory(string type)
    {
        int index = pageHistory.IndexOf(type);
        if (index >= 0 && index < pageHistory.Count - 1)
            pageHistory.RemoveRange(index + 1, pageHistory.Count - index - 1);
        else
            pageHistory.Add(type);
    }

    string GetPageType<T>() where T : Page
    {
        return typeof(T).ToString();
    }
    #endregion
}