using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using LitJson;


public class EditorCreateBundle : Editor
{
    public static string m_BundleDirectory = Application.dataPath + "/Editor Default Resources/AssetBundle/";
    public static bool isRelease;

    [MenuItem("自定义工具/Bundle/Debug/IOS打Bundle")]
    static void BuildDebugIOSBundle()
    {
        isRelease = false;
        BuildBundleTest(BuildTarget.iOS);
    }

    [MenuItem("自定义工具/Bundle/Debug/Android打Bundle")]
    static void BuildDebugAndroidBundle()
    {
        isRelease = false;
        BuildBundleTest(BuildTarget.Android);
    }

    [MenuItem("自定义工具/Bundle/Debug/Windows64打Bundle")]
    static void BuildDebugWindowsBundle()
    {
        isRelease = false;
        BuildBundleTest(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("自定义工具/Bundle/Release/IOS打Bundle")]
    static void BuildReleaseIOSBundle()
    {
        isRelease = true;
        //BuildBundleTest(BuildTarget.iOS);
    }

    [MenuItem("自定义工具/Bundle/Release/Android打Bundle")]
    static void BuildReleaseAndroidBundle()
    {
        isRelease = true;
        //BuildBundleTest(BuildTarget.Android);
    }

    [MenuItem("自定义工具/Bundle/Release/Windows64打Bundle")]
    static void BuildReleaseWindowsBundle()
    {
        isRelease = true;
        //BuildBundleTest(BuildTarget.StandaloneWindows64);
    }

    static void BuildBundleTest(BuildTarget target)
    {
        Caching.CleanCache();
        string[] filePaths = Directory.GetDirectories(m_BundleDirectory, "*.*", SearchOption.TopDirectoryOnly);
        string path = GetTempPath(target);
        DeleteTempBundles(target);
        SetAssetBundleName(filePaths);
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, target);
        CreateBundleVersionNumber(path, target);
        AssetDatabase.Refresh();
    }

    private static Dictionary<string, string> m_BundleMD5Map = new Dictionary<string, string>();
    static void DeleteTempBundles(BuildTarget target)
    {
        string[] bundleFiles = GetAllFilesFromBundleDirectory(target);
        foreach (string s in bundleFiles)
        {
            //File.Delete(s);
        }
    }

    static string[] GetAllFilesFromBundleDirectory(BuildTarget target)
    {
        string path = GetTempPath(target);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string[] bundleFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        return bundleFiles;
    }

    static void SetAssetBundleName(string[] topDirectories)
    {
        foreach (string path in topDirectories)
        {
            string[] childPaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            string childPathName, extension, directoryName;
            foreach (string childPath in childPaths)
            {
                extension = Path.GetExtension(childPath);
                if (extension != ".meta" && extension != ".DS_Store")
                {
                    childPathName = Path.GetFileNameWithoutExtension(childPath);
                    directoryName = Path.GetDirectoryName(childPath).Replace("\\", "/");

                    AssetImporter temp = AssetImporter.GetAtPath(childPath.Replace(Application.dataPath, "Assets"));
                    temp.assetBundleName = null;

                    if (directoryName.ToLower().IndexOf("sprite") >= 0)
                    {
                        AssetImporter ai = AssetImporter.GetAtPath(directoryName.Replace(Application.dataPath, "Assets"));
                        ai.assetBundleName = directoryName.Replace(m_BundleDirectory, "");
                    }
                    else
                        temp.assetBundleName = directoryName.Replace(m_BundleDirectory, "") + "/" + childPathName;
                }
            }
        }
    }

    static void CreateBundleVersionNumber(string bundlePath, BuildTarget target)
    {
        JsonData serverJson = new JsonData();
        string[] contents = Directory.GetFiles(bundlePath, "*.*", SearchOption.AllDirectories);
        string extension;
        string fileName = "";
        string fileMD5 = "";
        long allLength = 0;
        int fileLen;
        m_BundleMD5Map.Clear();
        for (int i = 0; i < contents.Length; i++)
        {
            fileName = contents[i].Replace(GetTempPath(target), "").Replace("\\", "/");
            extension = Path.GetExtension(contents[i]);
            if (extension != ".meta")
            {
                fileMD5 = MiscUtils.GetMD5HashFromFile(contents[i]);
                fileLen = File.ReadAllBytes(contents[i]).Length;
                allLength += fileLen;
                m_BundleMD5Map.Add(fileName, fileMD5 + "+" + fileLen);
            }
        }

        JsonData files = new JsonData();
        foreach (KeyValuePair<string, string> kv in m_BundleMD5Map)
        {
            JsonData jd = new JsonData();
            jd["file"] = kv.Key;
            string[] nAndL = kv.Value.Split('+');
            jd["md5"] = nAndL[0];
            jd["fileLength"] = nAndL[1];
            files.Add(jd);
        }
        serverJson["length"] = allLength;
        serverJson["files"] = files;

        File.WriteAllText(GetTempPath(target) + "Bundle.txt", serverJson.ToJson());

        m_BundleMD5Map.Clear();
        //MiscUtils.CopyDirectory(Application.streamingAssetsPath + "/AssetBundle", Application.dataPath.Replace("Assets", ""), "*.*", true);
        //Directory.Delete(Application.streamingAssetsPath + "/AssetBundle", true);
    }

    static string GetTempPath(BuildTarget target)
    {
        if (isRelease)
            return Application.dataPath.Replace("Assets", "") + target.ToString() + "/";
        else
            return Application.streamingAssetsPath + "/AssetBundle/" + target.ToString() + "/";
    }
}

