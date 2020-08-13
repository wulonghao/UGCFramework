﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using LitJson;
using UGCF.Manager;
using UGCF.Utils;

namespace UGCF.Editor
{
    public class EditorCreateBundle : UnityEditor.Editor
    {
        public static string m_BundleDirectory = Application.dataPath + "/Editor Default Resources/AssetBundle/";
        public static bool isRelease;

        [MenuItem("自定义工具/Bundle/Debug/IOS打Bundle")]
        public static void BuildDebugIOSBundle()
        {
            isRelease = false;
#if UNITY_IOS
        BuildBundleTest(BuildTarget.iOS);
#endif
        }

        [MenuItem("自定义工具/Bundle/Debug/Android打Bundle")]
        public static void BuildDebugAndroidBundle()
        {
            isRelease = false;
#if UNITY_ANDROID
            BuildBundleTest(BuildTarget.Android);
#endif
        }

        [MenuItem("自定义工具/Bundle/Debug/Windows64打Bundle")]
        static void BuildDebugWindowsBundle()
        {
            isRelease = false;
#if UNITY_WINDOWS
        BuildBundleTest(BuildTarget.StandaloneWindows64);
#endif
        }

        [MenuItem("自定义工具/Bundle/Release/IOS打Bundle")]
        public static void BuildReleaseIOSBundle()
        {
            isRelease = true;
#if UNITY_IOS
        BuildBundleTest(BuildTarget.iOS);
#endif
        }

        [MenuItem("自定义工具/Bundle/Release/Android打Bundle")]
        public static void BuildReleaseAndroidBundle()
        {
            isRelease = true;
#if UNITY_ANDROID
            BuildBundleTest(BuildTarget.Android);
#endif
        }

        [MenuItem("自定义工具/Bundle/Release/Windows64打Bundle")]
        static void BuildReleaseWindowsBundle()
        {
            isRelease = true;
#if UNITY_WINDOWS
        BuildBundleTest(BuildTarget.StandaloneWindows64);
#endif
        }

        static void BuildBundleTest(BuildTarget target)
        {
            Caching.ClearCache();
            string[] filePaths = Directory.GetDirectories(m_BundleDirectory, "*.*", SearchOption.TopDirectoryOnly);
            string path = GetChannelPath(target);
            //DeleteBundleFiles(target, isRelease);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            SetAssetBundleName(filePaths);
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, target);
            CreateBundleVersionNumber(path, target);
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, string> m_BundleMD5Map = new Dictionary<string, string>();
        public static void DeleteBundleFiles(BuildTarget target, bool isRelease = false)
        {
            string path;
            if (isRelease)
                path = GetChannelPath(target);
            else
                path = GetPlatformDirectoryPath(target);
            if (!Directory.Exists(path))
                return;
            Directory.Delete(path, true);
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

                        if (directoryName.ToLower().IndexOf("sprite") >= 0 || directoryName.IndexOf("HotFix") >= 0)
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
            File.Delete(GetChannelPath(target) + ConstantUtils.BundleInfoConfigName);
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
                fileName = contents[i].Replace(GetChannelPath(target), "").Replace("\\", "/");
                extension = Path.GetExtension(contents[i]);
                if (extension == ".manifest")
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    continue;
                }
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
            serverJson["version"] = Application.version;
            serverJson["length"] = allLength;
            serverJson["files"] = files;

            File.WriteAllText(GetChannelPath(target) + ConstantUtils.BundleInfoConfigName, serverJson.ToJson());

            m_BundleMD5Map.Clear();
        }

        static string GetChannelPath(BuildTarget target)
        {
            if (isRelease)
                return Application.dataPath.Replace("Assets", "Release/") + target.ToString() + "/" + ChannelManager.Instance.GetCurrentChannel() + "/";
            else
                return Application.streamingAssetsPath + "/AssetBundle/" + target.ToString() + "/" + ChannelManager.Instance.GetCurrentChannel() + "/";
        }

        static string GetPlatformDirectoryPath(BuildTarget target)
        {
            if (isRelease)
                return Application.dataPath.Replace("Assets", "Release/") + target.ToString();
            else
                return Application.streamingAssetsPath + "/AssetBundle/" + target.ToString();
        }
    }
}
