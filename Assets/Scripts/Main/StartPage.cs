using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[Hotfix]
public class StartPage : Page
{
    LuaEnv luaenv = new LuaEnv();
    public GameObject logo;
    public override void Init()
    {
        base.Init();
        logo.GetComponent<CommonAnimation>().alphaEndAction = () =>
        {
            LoadingNode.OpenLoadingNode(LoadingType.Progress);
            StartCoroutine(VersionUpdate());
        };
    }

    static long allFilesLength;
    /// <summary>
    /// 版本/文件更新
    /// </summary>
    /// <returns></returns>
    IEnumerator VersionUpdate()
    {
        WWW www = new WWW(ConstantUtils.bundleTipsUrl);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            List<BundleManager.BundleInfo> bmbis = new List<BundleManager.BundleInfo>();
            JsonData jds = JsonMapper.ToObject(www.text);
            string md5, file, path;
            int length;
            for (int i = 0; i < jds["files"].Count; i++)
            {
                JsonData jd = jds["files"][i];
                file = jd.TryGetString("file");
                path = ConstantUtils.AssetBundleFolderPath + file;
                md5 = MiscUtils.GetMD5HashFromFile(path);
                if (string.IsNullOrEmpty(md5) || md5 != jd.TryGetString("md5"))
                {
                    bmbis.Add(new BundleManager.BundleInfo()
                    {
                        _url = ConstantUtils.bundleDownLoadUrl + file,
                        _path = path,
                    });
                    length = int.Parse(jd.TryGetString("fileLength"));
                    allFilesLength += length;
                }
            }
            if (bmbis.Count > 0)
            {
                StartCoroutine(BundleManager.Instance.DownloadBundleFiles(bmbis,
                    (progress) => { LoadingNode.OpenLoadingNode(LoadingType.Progress, "自动更新中...", progress, allFilesLength); },
                    (isFinish) =>
                    {
                        if (isFinish)
                            StartCoroutine(VersionUpdateFinish());
                        else
                        {
                            TipManager.Instance.OpenTip(TipType.SimpleTip, "部分文件更新失败，正在准备重试...");
                            StartCoroutine(VersionUpdate());
                        }
                    }));
            }
            else
                StartCoroutine(VersionUpdateFinish());
        }
        else
        {
            UIUtils.Log(www.error);
            LoadingNode.CloseLoadingNode();
        }
    }

    /// <summary>
    /// 更新结束后执行的函数
    /// </summary>
    IEnumerator VersionUpdateFinish()
    {
#if !UNITY_EDITOR
        string luaScript = BundleManager.Instance.GetLuaScript();
        if (!string.IsNullOrEmpty(luaScript))
            luaenv.DoString(luaScript);
#endif
        LoadingNode.OpenLoadingNode(LoadingType.Progress, null, 1);
        yield return ConstantUtils.frameWait;
        LoadNecessaryBundle();
        MiscUtils.StartGPS();
        PageManager.Instance.OpenPage<TestPage>();
    }

    /// <summary>
    /// 加载必要资源
    /// </summary>
    void LoadNecessaryBundle()
    {
        PageManager.commonSpriteAB = BundleManager.Instance.GetSpriteBundle("common");
        AssetBundle ab = BundleManager.Instance.GetBundle("shader/imagetogrey");
        BundleManager.Instance.GetBundle("font/default");
        Material mat = ab.LoadAsset<Material>("imagetogrey");
        mat.shader = Shader.Find(mat.shader.name);
    }
}
