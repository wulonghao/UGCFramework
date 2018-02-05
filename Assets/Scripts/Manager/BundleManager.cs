using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using LitJson;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Events;
#endif
using XLua;

[Hotfix]
public class BundleManager : MonoBehaviour
{
    private static BundleManager instance = null;
    public static BundleManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject();
                instance = go.AddComponent<BundleManager>();
                go.name = instance.GetType().ToString();
            }
            return instance;
        }
    }

    public void OnDestory()
    {
        instance = null;
    }

    public struct BundleInfo
    {
        public string _url { get; set; }

        public string _path { get; set; }

        public override bool Equals(object bi)
        {
            return bi is BundleInfo && _url == ((BundleInfo)bi)._url;
        }

        public override int GetHashCode()
        {
            return _url.GetHashCode();
        }
    }

    public IEnumerator DownloadBundleFile(BundleInfo info, Action<bool> callback = null)
    {
        WWW www = new WWW(info._url);
        yield return www;
        bool success;
        if (string.IsNullOrEmpty(www.error))
        {
            try
            {
                string filePath = info._path;
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(filePath, www.bytes);
                success = true;
            }
            catch (Exception e)
            {
                UIUtils.Log("下载失败：url: " + info._url + ", error: " + e.Message);
                success = false;
            }
        }
        else
        {
            UIUtils.Log("下载失败：url: " + info._url + ", error: " + www.error);
            success = false;
        }
        if (callback != null)
            callback(success);
    }

    public IEnumerator DownloadBundleFiles(List<BundleInfo> infos, Action<float> loopCallback = null, Action<bool> callback = null)
    {
        int num = 0;
        string dir;
        for (int i = 0; i < infos.Count; i++)
        {
            BundleInfo info = infos[i];
            WWW www = new WWW(info._url);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                try
                {
                    string filePath = info._path;
                    dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllBytes(filePath, www.bytes);
                    num++;
                    if (loopCallback != null)
                        loopCallback((float)num / infos.Count);
                }
                catch (Exception e)
                {
                    UIUtils.Log("下载失败：url: " + info._url + ", error: " + e.Message);
                }
            }
            else
            {
                UIUtils.Log("下载失败：url: " + info._url + ", error: " + www.error);
            }
        }
        if (callback != null)
            callback(num == infos.Count);
    }

    #region get bundle
    const string commonBundleFolderName = "Common";
    public string GetLuaScript(string luaName = "hotfixScript")
    {
        TextAsset textAsset = GetBundleFile<TextAsset>(luaName, "LuaScript");
        if (textAsset != null)
            return textAsset.text;
        else
            return null;
    }

    public string GetJson(string jsonName, string pageName = commonBundleFolderName)
    {
#if UNITY_EDITOR
        TextAsset textAsset = (TextAsset)EditorGUIUtility.Load("AssetBundle/" + pageName + "/Config/" + jsonName + ".json");
#else
        TextAsset textAsset = GetBundleFile<TextAsset>(jsonName, pageName + "/Config");
#endif
        if (textAsset != null)
            return textAsset.text;
        else
            return null;
    }

    public string GetArchive(string archiveName)
    {
        string path = ConstantUtils.ArchiveFolderPath + archiveName;
        if (File.Exists(path))
            return File.ReadAllText(path);
        else
            return null;
    }

    public AudioClip GetAudioClip(string soundName, string soundFolder, string pageName = commonBundleFolderName)
    {
        return GetBundleFile<AudioClip>(soundFolder + "/" + soundName, pageName + "/Sound");
    }

    public AssetBundle GetBundle(string bundlePath)
    {
        string path = ConstantUtils.AssetBundleFolderPath + bundlePath.ToLower();
        if (File.Exists(path))
            return AssetBundle.LoadFromFile(path);
        else
            return null;
    }

    /// <summary>
    /// 获取sprite的bundle资源,并保留在内存中
    /// </summary>
    /// <param name="spriteName"></param>
    /// <returns></returns>
    public AssetBundle GetSpriteBundle(string spriteName, string pageName = commonBundleFolderName)
    {
        spriteName = spriteName.ToLower();
        return GetBundle(pageName + "/Sprite/" + spriteName);
    }

    /// <summary>
    /// 获取Sprite
    /// </summary>
    /// <param name="spriteName"></param>
    /// <param name="spriteBundle"></param>
    /// <returns></returns>
    public Sprite GetSprite(string spriteName, AssetBundle spriteBundle)
    {
        return spriteBundle.LoadAsset<Sprite>(spriteName.ToLower());
    }

    public Sprite GetSprite(string spriteName, string pageName = commonBundleFolderName)
    {
        return GetBundleFile<Sprite>(spriteName, pageName + "/Sprite");
    }

    public Sprite[] GetSprites(AssetBundle spriteBundle, bool isUnload = true)
    {
        Sprite[] s = spriteBundle.LoadAllAssets<Sprite>();
        if (isUnload)
            spriteBundle.Unload(false);
        return s;
    }

    List<Sprite> ls = new List<Sprite>();
    Sprite[] sps;
    string tempName;
    public Sprite[] GetAnimationSprites(string name, Sprite[] allAnimationSprites)
    {
        ls.Clear();
        name = name.ToLower();
        for (int i = 0; i < allAnimationSprites.Length; i++)
        {
            tempName = allAnimationSprites[i].name;
            if (tempName == name || tempName.Substring(0, tempName.LastIndexOf('_')) == name)
                ls.Add(allAnimationSprites[i]);
        }
        sps = new Sprite[ls.Count];
        ls.Sort((a, b) =>
        {
            int aIndex = int.Parse(a.name.Substring(a.name.LastIndexOf('_') + 1));
            int bIndex = int.Parse(b.name.Substring(b.name.LastIndexOf('_') + 1));
            return aIndex.CompareTo(bIndex);
        });
        ls.CopyTo(sps);
        return sps;
    }

    /// <summary>
    /// 获取一个预制体
    /// </summary>
    /// <param name="gameObjectName"></param>
    /// <returns></returns>
    public GameObject GetGameObject(string gameObjectName, string pageName = commonBundleFolderName)
    {
        GameObject pre = GetBundleFile<GameObject>(gameObjectName, pageName + "/Prefab");
        if (pre != null)
        {
            GameObject go = Instantiate(pre);
            go.name = MiscUtils.GetFileName(gameObjectName);
            return go;
        }
        return null;
    }

    T GetBundleFile<T>(string fileName, string folder) where T : UnityEngine.Object
    {
        try
        {
            T asset = null;
            string fileNameNew = (folder + "/" + fileName).ToLower();
            string path = ConstantUtils.AssetBundleFolderPath + fileNameNew;
            if (!File.Exists(path))
            {
                fileNameNew = (commonBundleFolderName + "/" + MiscUtils.GetFileName(folder) + "/" + fileName).ToLower();
                path = ConstantUtils.AssetBundleFolderPath + fileNameNew;
                if (!File.Exists(path))
                {
                    path = folder + "/" + fileName;
                    asset = Resources.Load<T>(path);
                    if (asset == null)
                        asset = Resources.Load<T>(path.Remove(0, path.IndexOf("/") + 1));
                    return asset;
                }
                else
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(path);
                    AssetBundleRequest abr = ab.LoadAssetAsync<T>(MiscUtils.GetFileName(fileNameNew));
                    asset = (T)abr.asset;
                    ab.Unload(false);
                    return asset;
                }
            }
            else
            {
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                AssetBundleRequest abr = ab.LoadAssetAsync<T>(MiscUtils.GetFileName(fileNameNew));
                asset = (T)abr.asset;
                ab.Unload(false);
                return asset;
            }
        }
        catch (Exception e)
        {
            UIUtils.Log(e.Message);
            return null;
        }
    }

    /// <summary>
    /// 获取bundle是否存在
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool ExistsBundle(string fileName)
    {
        bool had = false;
        fileName = fileName.ToLower();
        string path = ConstantUtils.AssetBundleFolderPath + fileName;
        if (File.Exists(path))
            had = true;
        else
        {
            object o = Resources.Load(fileName);
            had = o != null;
            o = null;
        }
        return had;
    }
    #endregion

    void OnDestroy()
    {
        instance = null;
    }
}
